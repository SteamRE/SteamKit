/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using log4net;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace SteamKit3
{
    /// <summary>
    /// This attribute must be applied to all jobs that which wish to be registered with a <see cref="JobMgr"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    public sealed class JobAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the network message type this job will handle.
        /// The job will be launched when this message arrives.
        /// </summary>
        /// <value>
        /// The network message type.
        /// </value>
        public EMsg MsgType { get; set; }
        /// <summary>
        /// Gets or sets the job type.
        /// </summary>
        /// <value>
        /// The job type.
        /// </value>
        public JobType JobType { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="JobAttribute"/> class.
        /// </summary>
        public JobAttribute()
        {
            MsgType = EMsg.Invalid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobAttribute"/> class.
        /// </summary>
        /// <param name="eMsg">The network message this job will handle. The job will be launched when this message arrives.</param>
        /// <param name="jobType">The job type.</param>
        public JobAttribute( EMsg eMsg, JobType jobType )
            : this()
        {
            this.MsgType = eMsg;
            this.JobType = jobType;
        }
    }

    /// <summary>
    /// This base class all jobs must inherit.
    /// </summary>
    public abstract class Job
    {
        /// <summary>
        /// Gets the job id that was assigned for this job.
        /// </summary>
        /// <value>
        /// The job id.
        /// </value>
        public ulong JobID { get; private set; }

        /// <summary>
        /// Gets the <see cref="JobMgr"/> that is in charge of overseeing this job.
        /// </summary>
        protected JobMgr JobMgr { get; private set; }

        // jobmgr accessors
        internal ManualResetEventSlim PacketWait { get; private set; }

        internal IPacketMsg WaitedPacket { get; set; }
        internal EMsg WaitedMsgType { get; private set; }

        /// <summary>
        /// Gets the log4net logging context for this job.
        /// </summary>
        protected ILog Log { get; private set; }


        readonly TimeSpan NetworkTimeout = TimeSpan.FromSeconds( 10 );


        /// <summary>
        /// Initializes a new instance of the <see cref="Job"/> class.
        /// </summary>
        /// <param name="jobManager">The <see cref="JobMgr">job manager</see> that oversees this job.</param>
        public Job( JobMgr jobManager )
        {
            JobMgr = jobManager;

            // asign a new job id to this job
            JobID = jobManager.GetNextJobID();

            PacketWait = new ManualResetEventSlim();

            Log = LogManager.GetLogger( this.GetType() );
        }


        /// <summary>
        /// Causes the job to wait for a specific network message with a specific timeout.
        /// This function will yield execution to other jobs.
        /// If the timeout elapses without the message coming in, this function returns <c>null</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This function will cause the <see cref="JobMgr"/> to pass all responses with this message type to this job,
        /// and any other job that is waiting on the same message type.
        /// This function is used for client messages that don't respect job ids.
        /// The <see cref="JobMgr"/> respects incoming messages with job ids first, and will not route a message of
        /// the requested type to this function if a job id is assigned.
        /// </para>
        /// <para>
        /// If possible, job code should attempt to use the <see cref="ClientJob.YieldingSendMsgAndGetReply"/> or 
        /// <see cref="ClientJob.YieldingSendMsgAndWaitForMsg"/> functions instead of calling
        /// <see cref="ClientJob.SendMessage"/> and then calling this function, because otherwise the response packet
        /// may come in before the job is assigned to wait for it.
        /// </para>
        /// </remarks>
        /// <param name="eMsg">The message type to wait for.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> value indicating how long this job should wait for a response to come in.</param>
        /// <returns>An <see cref="IPacketMsg"/> when the waited for message comes in, or null if the timeout elapses.</returns>
        protected Task<IPacketMsg> YieldingWaitForMsg( EMsg eMsg, TimeSpan? timeout = null )
        {
            // flag what kind of message we're waiting for
            this.WaitedMsgType = eMsg;

            return Task.Factory.StartNew( () =>
            {
                TimeSpan waitTime = timeout.GetValueOrDefault( NetworkTimeout );

                // wait for the packet to come in, or timeout
                if ( !this.PacketWait.Wait( waitTime ) )
                {
                    Log.ErrorFormat( "Timed out while waiting for {0}", eMsg );
                    return null;
                }

                // reset our wait state, in case we'll want to wait on another message
                this.PacketWait.Reset();

                // hand off the packet
                return this.WaitedPacket;
            } );
        }



        internal async void YieldingStart( object param = null )
        {
            await YieldingRunJob( param );

            JobMgr.EndJob( this );
        }
        internal async void YieldingStartFromNetMsg( IPacketMsg clientMsg )
        {
            await YieldingRunJobFromMsg( clientMsg );

            JobMgr.EndJob( this );
        }

        /// <summary>
        /// Called when the job has been launched from a client action.
        /// This function can yield execution to other jobs.
        /// </summary>
        /// <param name="param">A user state object.</param>
        /// <returns><c>void</c></returns>
        protected async virtual Task YieldingRunJob( object param )
        {
            Contract.Assert( false, "the base YieldingRunJob should not be called." );
        }
        /// <summary>
        /// Called when this job has been launched from a network message.
        /// This function can yield execution to other jobs.
        /// </summary>
        /// <param name="clientMsg">The packet message meant for this job.</param>
        /// <returns><c>void</c></returns>
        protected async virtual Task YieldingRunJobFromMsg( IPacketMsg clientMsg )
        {
            Contract.Assert( false, "the base YieldingRunJobFromMsg should not be called." );
        }


        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} (id: {1})", this.GetType(), this.JobID );
        }
    }


    /// <summary>
    /// Base class all <see cref="SteamClient"/> related jobs should inherit.
    /// This job type provides access to the <see cref="SteamClient"/> that the <see cref="JobMgr"/> that handles this job is for.
    /// This provides access to sending messages and other network related activities.
    /// </summary>
    public abstract class ClientJob : Job
    {
        /// <summary>
        /// Gets the <see cref="SteamClient"/> associated with this job.
        /// </summary>
        protected SteamClient Client { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientJob"/> class.
        /// </summary>
        /// <param name="steamClient">The <see cref="SteamClient"/> for this job.</param>
        public ClientJob( SteamClient steamClient )
            : base( steamClient.JobMgr )
        {
            this.Client = steamClient;
        }

        /// <summary>
        /// Sends a client message to the remote server.
        /// This message will assign the job id to the message.
        /// </summary>
        /// <remarks>
        /// Some client message job ids aren't respected, and sometimes a reply meant for this job 
        /// will come in with a -1 jobid. To wait on messages such as this, use <see cref="Job.YieldingWaitForMsg"/>.
        /// </remarks>
        /// <param name="msg">The client message to send.</param>
        public void SendMessage( IClientMsg msg )
        {
            // assign our job id to the message
            msg.SourceJobID = this.JobID;

            Client.Send( msg );
        }

        /// <summary>
        /// Causes this job to wait for a network message that is meant for only this job with a specific timeout.
        /// This function will yield execution to other jobs.
        /// If the timeout elapses without the message coming in, this function returns <c>null</c>.
        /// </summary>
        /// <remarks>
        /// This function will wait on a message that has this job as it's target. The <see cref="JobMgr"/> will assure that only messages
        /// meant for this job are sent to this job. Messages that don't respect job ids will not be correctly received.
        /// </remarks>
        /// <seealso cref="Job.YieldingWaitForMsg"/>
        /// <param name="msg">The client message to send.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> value indicating how long this job should wait for a response to come in.</param>
        /// <returns>An <see cref="IPacketMsg"/> when the waited for message comes in, or <c>null</c> if the timeout elapses.</returns>
        protected async Task<IPacketMsg> YieldingSendMsgAndGetReply( IClientMsg msg, TimeSpan? timeout = null )
        {
            // we wait for invalid, because the response is expected to have a jobid, and the jobmgr's route logic will hand us the message
            var waitMsgTask = YieldingWaitForMsg( EMsg.Invalid, timeout );

            SendMessage( msg );

            return await waitMsgTask;
        }
        /// <summary>
        /// Causes this job to wait for a network message of a specific message type with a specific timeout.
        /// This function will yield execution to other jobs.
        /// If the timeout elapses without the message coming in, this function returns <c>null</c>.
        /// </summary>
        /// <remarks>
        /// This function will wait on a message of a specific type. The <see cref="JobMgr"/> will assure that only messages of
        /// the specified message type are sent to this job.
        /// This function is used for client messages that don't respect job ids.
        /// The <see cref="JobMgr"/> respects incoming messages with job ids first, and will not route a message of
        /// the requested type to this function if a job id is assigned.
        /// </remarks>
        /// <seealso cref="Job.YieldingWaitForMsg"/>
        /// <param name="msg">The client message to send.</param>
        /// <param name="eMsg">The message type to wait for.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> value indicating how long this job should wait for a response to come in.</param>
        /// <returns>An <see cref="IPacketMsg"/> when the waited for message comes in, or <c>null</c> if the timeout elapses.</returns>
        protected async Task<IPacketMsg> YieldingSendMsgAndWaitForMsg( IClientMsg msg, EMsg eMsg, TimeSpan? timeout = null )
        {
            var waitMsgTask = YieldingWaitForMsg( eMsg, timeout );

            SendMessage( msg );

            return await waitMsgTask;
        }
    }
}
