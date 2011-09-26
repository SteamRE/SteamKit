/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using log4net;
using System.Diagnostics.Contracts;

namespace SteamKit3
{

    struct JobInfo
    {
        public EMsg MsgType { get; set; }
        public Type Type { get; set; }
        public JobType JobType { get; set; }
    }

    /// <summary>
    /// Represents the possible job types that could exist.
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// A basic job with no special functionality.
        /// </summary>
        Job,
        /// <summary>
        /// A client job that recieves a <see cref="SteamClient"/> instance for sending messages through.
        /// </summary>
        ClientJob,
    }

    /// <summary>
    /// This class is responsible for handling all of the network messages a <see cref="SteamClient"/> recieves.
    /// It is tasked with launching new jobs from incoming messages, or routing messages to jobs which are waiting for them.
    /// </summary>
    public class JobMgr
    {
        ulong nextJobId;
        Dictionary<ulong, Job> jobMap;

        SteamClient client;

        static readonly ILog log = LogManager.GetLogger( typeof( JobMgr ) );

        static Dictionary<EMsg, JobInfo> registeredJobs;


        static JobMgr()
        {
            registeredJobs = new Dictionary<EMsg, JobInfo>();

            // register our built-in jobs
            RegisterJobs( Assembly.GetExecutingAssembly() );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobMgr"/> class.
        /// </summary>
        /// <param name="steamClient">The <see cref="SteamClient"/> that owns this <see cref="JobMgr"/>.</param>
        public JobMgr( SteamClient steamClient )
        {
            Contract.Requires( steamClient != null );

            jobMap = new Dictionary<ulong, Job>();

            this.client = steamClient;
        }


        /// <summary>
        /// Registers all available jobs in the specified <see cref="Assembly"/>.
        /// The jobs must have a <see cref="JobAttribute"/>.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public static void RegisterJobs( Assembly assembly )
        {
            Contract.Requires( assembly != null );

            foreach ( var type in assembly.GetTypes() )
            {
                var attribs = type.GetCustomAttributes( typeof( JobAttribute ), false ) as JobAttribute[];

                if ( attribs == null || attribs.Length == 0 )
                    continue;

                EMsg msgType = attribs[ 0 ].MsgType;

                JobInfo jobInfo = new JobInfo()
                {
                    MsgType = msgType,
                    Type = type,
                    JobType = attribs[ 0 ].JobType,
                };

                registeredJobs.Add( msgType, jobInfo );
            }
        }


        /// <summary>
        /// Manually launches the given job.
        /// </summary>
        /// <param name="job">The job to launch.</param>
        /// <param name="param">The user provided state object.</param>
        public void LaunchJob( Job job, object param = null )
        {
            Contract.Requires( job != null );

            log.DebugFormat( "Launching job {0}", job );

            jobMap.Add( job.JobID, job );

            job.YieldingStart( param );
        }


        internal ulong GetNextJobID()
        {
            return ++nextJobId;
        }
        internal void RouteMsgToJob( IPacketMsg clientMsg )
        {
            Contract.Requires( clientMsg != null );

            if ( clientMsg.TargetJobID != ulong.MaxValue )
            {
                // if the job has a jobid, lets see if we have a job waiting for it
                var waitingJob = FindJobByID( clientMsg.TargetJobID );

                if ( waitingJob != null )
                {
                    // the job is waiting for this message, lets pass it along
                    PassMsgToJob( waitingJob, clientMsg );
                    return;
                }

                Contract.Assert( false, string.Format( "No job waiting for clientmsg {0}", clientMsg.MsgType ) );
                return;
            }

            // maybe some jobs are waiting on a specific EMsg
            var waitingJobs = FindJobsWaitingForMsg( clientMsg.MsgType );

            if ( waitingJobs.Count == 0 )
            {
                // no target job, so lets launch a new one
                LaunchJobFromMsg( clientMsg );
            }
            else
            {
                // otherwise route the message to these jobs
                foreach ( var job in waitingJobs )
                {
                    PassMsgToJob( job, clientMsg );
                }
            }
        }
        internal void EndJob( Job job )
        {
            Contract.Requires( job != null );

            log.DebugFormat( "Ending job {0} with id {1}", job.GetType(), job.JobID );

            jobMap.Remove( job.JobID );
        }


        Job FindJobByID( ulong jobId )
        {
            Contract.Assert( jobId != ulong.MaxValue, "JobMgr is tracking an invalid job" );

            if ( !jobMap.ContainsKey( jobId ) )
            {
                return null;
            }

            return jobMap[ jobId ];
        }
        List<Job> FindJobsWaitingForMsg( EMsg eMsg )
        {
            List<Job> matchingJobs = new List<Job>();

            foreach ( var kvp in jobMap )
            {
                if ( kvp.Value.WaitedMsgType == eMsg )
                {
                    matchingJobs.Add( kvp.Value );
                }
            }

            return matchingJobs;
        }

        void PassMsgToJob( Job job, IPacketMsg clientMsg )
        {
            Contract.Requires( job != null );
            Contract.Requires( clientMsg != null );

            log.DebugFormat( "Passing msg {0} (id: {1}) to job {2} (id: {3})", clientMsg.MsgType, clientMsg.TargetJobID, job.GetType(), job.JobID );

            // give the job the message it's waiting for
            job.WaitedPacket = clientMsg;

            // wake up, damnit
            job.PacketWait.Set();
        }
        void LaunchJobFromMsg( IPacketMsg clientMsg )
        {
            Contract.Requires( clientMsg != null );

            var newJob = CreateJob( clientMsg.MsgType );

            if ( newJob == null )
            {
                // no job for this message

                log.WarnFormat( "Got msg {0} (IsProto: {1}) with no registered job", clientMsg.MsgType, clientMsg.IsProto );
                return;
            }

            log.DebugFormat( "Launching job {0} (id: {1}) from msg {2}", newJob.GetType(), newJob.JobID, clientMsg.MsgType );

            jobMap.Add( newJob.JobID, newJob );

            newJob.YieldingStartFromNetMsg( clientMsg );
        }

        Job CreateJob( EMsg eMsg )
        {
            if ( !registeredJobs.ContainsKey( eMsg ) )
                return null;

            JobInfo jobInfo = registeredJobs[ eMsg ];

            object jobObject = null;

            switch ( jobInfo.JobType )
            {
                case JobType.Job:
                    jobObject = Activator.CreateInstance( jobInfo.Type, new object[] { this } );
                    break;

                case JobType.ClientJob:
                    jobObject = Activator.CreateInstance( jobInfo.Type, new object[] { client } );
                    break;
            }

            return jobObject as Job;
        }
    }
}
