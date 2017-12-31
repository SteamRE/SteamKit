/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamKit2
{
    static class TaskExtensions
    {
        public static async Task IgnoringCancellation(this Task task, CancellationToken token)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
        }
    }
}