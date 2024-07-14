// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Online
{
    /// <summary>
    /// Common interface for clients of "stateful user hubs", i.e. server-side hubs
    /// that preserve user state.
    /// In the case of such hubs, concurrency constraints are enforced (only one client
    /// can be connected at a time).
    /// </summary>
    public interface IStatefulUserHubClient
    {
        /// <summary>
        /// Invoked when the server requests a client to disconnect.
        /// </summary>
        /// <remarks>
        /// When this request is received, the client must presume any and all further requests to the server
        /// will either fail or be ignored.
        /// This method is ONLY to be used for the purposes of:
        /// <list type="bullet">
        /// <item>actually physically disconnecting from the server,</item>
        /// <item>cleaning up / setting up any and all required local client state.</item>
        /// </list>
        /// </remarks>
        Task DisconnectRequested();
    }
}
