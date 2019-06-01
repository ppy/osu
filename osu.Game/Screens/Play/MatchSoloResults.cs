// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Graphics;
using osu.Game.Scoring;
using osu.Game.Screens.Multi;

namespace osu.Game.Screens.Play
{
    public class MatchSoloResults : SoloResults, IMultiplayerSubScreen
    {
        public string ShortTitle => "solo results";
        public override string Title => ShortTitle.Humanize();

        public MatchSoloResults(ScoreInfo score)
            : base(score)
        {
            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
        }
    }
}
