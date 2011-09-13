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

namespace SteamKit3
{
    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    public sealed class JobAttribute : Attribute
    {
        public EMsg MsgType { get; set; }
        public JobType JobType { get; set; }


        public JobAttribute()
        {
        }
        public JobAttribute( EMsg eMsg, JobType jobType )
        {
            this.MsgType = eMsg;
            this.JobType = jobType;
        }
    }

    public abstract class Job
    {
        public ulong JobID { get; set; }

        protected JobMgr JobMgr { get; private set; }

        // jobmgr accessors
        internal ManualResetEventSlim PacketWait { get; private set; }

        internal IPacketMsg WaitedPacket { get; set; }
        internal EMsg WaitedMsgType { get; private set; }


        public Job( JobMgr jobManager )
        {
            this.JobMgr = jobManager;

            this.JobID = jobManager.GetNextJobID();

            PacketWait = new ManualResetEventSlim();
        }

        protected Task<IPacketMsg> YieldingWaitForMsg( EMsg eMsg )
        {
            WaitedMsgType = eMsg;

            return Task.Factory.StartNew( () =>
            {
                PacketWait.Wait();
                PacketWait.Reset();

                return WaitedPacket;
            } );
        }


        public async void Start( object param = null )
        {
            await YieldingRunJob( param );

            JobMgr.EndJob( this );
        }
        public async void StartFromNetMsg( IPacketMsg clientMsg )
        {
            await YieldingRunJobFromMsg( clientMsg );

            JobMgr.EndJob( this );
        }


        protected async virtual Task YieldingRunJob( object param )
        {
        }
        protected async virtual Task YieldingRunJobFromMsg( IPacketMsg clientMsg )
        {
        }
    }

    public abstract class ClientTypedJob<T> : ClientJob
    {
        public ClientTypedJob( SteamClient client )
            : base( client )
        {
        }

        protected async override Task YieldingRunJob( object param )
        {
            await YieldingRunJob( ( T )param );
        }

        protected abstract Task YieldingRunJob( T param );
    }

    public abstract class ClientJob : Job
    {
        protected SteamClient Client { get; private set; }


        public ClientJob( SteamClient steamClient )
            : base( steamClient.JobMgr )
        {
            this.Client = steamClient;
        }

        public void SendMessage( IClientMsg msg )
        {
            msg.SourceJobID = this.JobID;

            Client.Send( msg );
        }
    }
}
