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

namespace SteamKit3
{

    struct JobInfo
    {
        public EMsg MsgType { get; set; }
        public Type Type { get; set; }
        public JobType JobType { get; set; }
    }

    public enum JobType
    {
        Job,
        ClientJob,
    }

    public class JobMgr
    {
        ulong nextJobId;
        Dictionary<ulong, Job> jobMap;

        SteamClient client;


        static Dictionary<EMsg, JobInfo> registeredJobs;


        static JobMgr()
        {
            registeredJobs = new Dictionary<EMsg, JobInfo>();

            // register our built-in jobs
            RegisterJobs( Assembly.GetExecutingAssembly() );
        }

        public JobMgr( SteamClient steamClient )
        {
            jobMap = new Dictionary<ulong, Job>();

            this.client = steamClient;
        }


        public static void RegisterJobs( Assembly assembly )
        {
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


        public ulong GetNextJobID()
        {
            return ++nextJobId;
        }

        public void RouteMsgToJob( IPacketMsg clientMsg )
        {
            var waitingJob = FindJobByID( clientMsg.TargetJobID );

            if ( waitingJob != null )
            {
                // the job is waiting for this message, lets pass it along
                PassMsgToJob( waitingJob, clientMsg );
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

        public void LaunchJob( Job job, object param = null )
        {
            jobMap.Add( job.JobID, job );

            job.Start( param );
        }


        Job FindJobByID( ulong jobId )
        {
            if ( !jobMap.ContainsKey( jobId ) )
            {
                return null;
            }

            Debug.Assert( jobId != ulong.MaxValue, "JobMgr is tracking an invalid job." );

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
            // give the job the message it's waiting for
            job.WaitedPacket = clientMsg;

            // wake up, damnit
            job.PacketWait.Set();
        }
        void LaunchJobFromMsg( IPacketMsg clientMsg )
        {
            var newJob = CreateJob( clientMsg.MsgType );

            if ( newJob == null )
            {
                // this can happen when no job is registered for a message type, which is okay
                // todo: log this?
                return;
            }

            jobMap.Add( newJob.JobID, newJob );

            newJob.StartFromNetMsg( clientMsg );
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
        internal void EndJob( Job job )
        {
            jobMap.Remove( job.JobID );
        }
    }
}
