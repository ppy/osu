// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class NoScoresPlaceholder : Container
    {
        private readonly SpriteText text;

        public NoScoresPlaceholder()
        {
            AutoSizeAxes = Axes.Both;
            Child = text = new SpriteText
            {
                Font = OsuFont.GetFont(),
            };
        }

        public void UpdateText(BeatmapLeaderboardScope scope)
        {
            switch (scope)
            {
                default:
                case BeatmapLeaderboardScope.Global:
                text.Text = @"No scores yet. Maybe should try setting some?";
                return;

                case BeatmapLeaderboardScope.Friend:
                text.Text = @"None of your friends has set a score on this map yet!";
                return;

                case BeatmapLeaderboardScope.Country:
                text.Text = @"No one from your country has set a score on this map yet!";
                return;
            }
        }
    }
}
