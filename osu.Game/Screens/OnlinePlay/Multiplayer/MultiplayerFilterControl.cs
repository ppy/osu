// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerFilterControl : FilterControl
    {
        protected override FilterCriteria CreateCriteria()
        {
            var criteria = base.CreateCriteria();
            criteria.Category = "realtime";
            return criteria;
        }
    }
}
