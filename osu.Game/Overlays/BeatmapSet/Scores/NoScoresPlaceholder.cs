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

            switch (scope)
            {
                default:
                    text.Text = @"还没有成绩呢... 考虑当个第一名owo?";
                    break;

                case BeatmapLeaderboardScope.Friend:
                    text.Text = @"你的好友们还没玩过这张图> <";
                    break;

                case BeatmapLeaderboardScope.Country:
                    text.Text = @"你所在的国家/区域内好像只有你有这张图> <.";
                    break;
            }
        }
    }
}
