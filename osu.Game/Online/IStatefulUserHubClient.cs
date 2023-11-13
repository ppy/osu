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
        Task DisconnectRequested();
    }
}
