// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailAreaLeaderboardTabItem<TScope> : BeatmapDetailAreaTabItem
        where TScope : Enum
    {
        public override string Name => Scope.ToString();

        public override bool FilterableByMods => true;

        public readonly TScope Scope;

        public BeatmapDetailAreaLeaderboardTabItem(TScope scope)
        {
            Scope = scope;
        }
    }
}
