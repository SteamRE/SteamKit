/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


namespace SteamKit2
{
    /// <summary>
    /// Represents the base object all callbacks are based off.
    /// </summary>
    public abstract class CallbackMsg
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackMsg"/> class.
        /// </summary>
        protected CallbackMsg()
        {
            JobID = JobID.Invalid;
        }

        /// <summary>
        /// Gets or sets the job ID this callback refers to. If it is not a job callback, it will be <see cref="P:JobID.Invalid" />.
        /// </summary>
        public JobID JobID { get; set; }
    }
}
