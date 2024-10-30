/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;

namespace SteamKit2.Internal
{
    /// <summary>
    /// This is the base class for the utility <see cref="Callback&lt;TCall&gt;" /> class.
    /// This is for internal use only, and shouldn't be used directly.
    /// </summary>
    abstract class CallbackBase
    {
        internal abstract Type CallbackType { get; }
        internal abstract void Run( CallbackMsg callback );
    }

    sealed class Callback<TCall> : CallbackBase, IDisposable
        where TCall : CallbackMsg
    {
        CallbackManager? mgr;

        public JobID JobID { get; set; }

        public Action<TCall> OnRun { get; set; }

        internal override Type CallbackType => typeof( TCall );

        public Callback( Action<TCall> func, CallbackManager mgr, JobID jobID )
        {
            this.JobID = jobID;
            this.OnRun = func;
            this.mgr = mgr;

            mgr.Register( this );
        }

        ~Callback()
        {
            Dispose();
        }

        public void Dispose()
        {
            mgr?.Unregister( this );
            mgr = null;

            System.GC.SuppressFinalize( this );
        }

        internal override void Run( CallbackMsg callback )
        {
            var cb = callback as TCall;
            if ( cb != null && ( cb.JobID == JobID || JobID == JobID.Invalid ) && OnRun != null )
            {
                OnRun( cb );
            }
        }
    }
}
