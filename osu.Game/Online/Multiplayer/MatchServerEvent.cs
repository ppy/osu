// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// An event from the server to allow clients to update gameplay to an expected state.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public abstract class MatchServerEvent
    {
    }
}
