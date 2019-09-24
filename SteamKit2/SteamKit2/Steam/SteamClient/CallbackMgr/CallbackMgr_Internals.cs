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
        internal abstract void Run( object callback );
    }

    interface ICallbackMgrInternals
    {
        void Register( CallbackBase callback );
        void Unregister( CallbackBase callback );
    }
        
    sealed class Callback<TCall> : Internal.CallbackBase, IDisposable
        where TCall : class, ICallbackMsg
    {
        ICallbackMgrInternals? mgr;
            
        public JobID JobID { get; set; }
            
        public Action<TCall> OnRun { get; set; }

        internal override Type CallbackType { get { return typeof( TCall ); } }
            
        public Callback(Action<TCall> func, ICallbackMgrInternals? mgr = null)
            : this ( func, mgr, JobID.Invalid )
        {
        }

        public Callback(Action<TCall> func, ICallbackMgrInternals? mgr, JobID jobID)
        {
            this.JobID = jobID;
            this.OnRun = func;

            AttachTo(mgr);
        }
            
        void AttachTo( ICallbackMgrInternals? mgr )
        {
            if ( mgr == null )
                return;

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

            System.GC.SuppressFinalize( this );
        }


        internal override void Run( object callback )
        {
            var cb = callback as TCall;
            if (cb != null && (cb.JobID == JobID || JobID == JobID.Invalid) && OnRun != null)
            {
                OnRun(cb);
            }
        }

        static Action<TCall, JobID> CreateJoblessAction(Action<TCall> func)
        {
            return delegate(TCall callback, JobID jobID)
            {
                func(callback);
            };
        }
    }
}
