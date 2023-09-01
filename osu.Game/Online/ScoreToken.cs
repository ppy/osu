// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online
{
    /// <summary>
    /// Represents a score token that can later be exchanged with web for a score ID.
    /// </summary>
    /// <remarks>
    /// The correct way of exchanging the token for a score ID depends on the <see cref="Type"/>.
    /// </remarks>
    [Serializable]
    [MessagePackObject]
    public record ScoreToken(
        [property: Key(0)] long ID,
        [property: Key(1)] ScoreTokenType Type);

    public enum ScoreTokenType
    {
        Solo,
        Multiplayer,
    }
}
