/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SteamKit2
{
// warning CS0660: 'SteamKit2.JobID' defines operator == or operator != but does not override Object.Equals(object o)
// this is disabled because our base UInt64Handle class handles Object.Equals for us
#pragma warning disable 0660
#pragma warning disable 0661

    /// <summary>
    /// Represents an identifier of a network task known as a job.
    /// </summary>
    [DebuggerDisplay( "{Value}" )]
    public sealed class JobID : UInt64Handle
    {
        /// <summary>
        /// Represents an invalid JobID.
        /// </summary>
        public static readonly JobID Invalid = new JobID();


        /// <summary>
        /// Initializes a new instance of the <see cref="JobID"/> class.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        public JobID( ulong jobId = ulong.MaxValue )
            : base( jobId )
        {
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit2.JobID"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="job">The job ID.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ulong( JobID job )
        {
            return job.Value;
        }
        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="SteamKit2.JobID"/>.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator JobID( ulong jobId )
        {
            return new JobID( jobId );
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The first job ID.</param>
        /// <param name="b">The second job ID.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==( JobID a, JobID b )
        {
            if ( object.ReferenceEquals( a, b ) )
                return true;

            if ( ( ( object )a == null ) || ( ( object )b == null ) )
                return false;

            return a.Value == b.Value;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The first job ID.</param>
        /// <param name="b">The second job ID.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=( JobID a, JobID b )
        {
            return !( a == b );
        }
    }

#pragma warning restore 0660
#pragma warning disable 0661

}
