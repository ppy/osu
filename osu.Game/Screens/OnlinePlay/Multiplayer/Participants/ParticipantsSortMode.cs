// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public enum ParticipantsSortMode
    {
        [Description("Rank")]
        Rank,

        [Description("Alphabetical")]
        Alphabetical,

        [Description("Country")]
        Country
    }
}
