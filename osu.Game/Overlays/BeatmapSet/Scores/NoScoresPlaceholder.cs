// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class NoScoresPlaceholder : Container
    {
        private readonly SpriteText text;

        public NoScoresPlaceholder()
        {
            AutoSizeAxes = Axes.Both;
            Child = text = new OsuSpriteText();
        }

        public override void Show() => this.FadeIn(200, Easing.OutQuint);

        public override void Hide() => this.FadeOut(200, Easing.OutQuint);

        public void ShowWithScope(BeatmapLeaderboardScope scope)
        {
            Show();

            text.Text = scope switch
            {
                BeatmapLeaderboardScope.Friend => @"None of your friends have set a score on this map yet.",
                BeatmapLeaderboardScope.Country => @"No one from your country has set a score on this map yet.",
                _ => @"No scores have been set yet. Maybe you can be the first!",
            };
        }
    }
}
