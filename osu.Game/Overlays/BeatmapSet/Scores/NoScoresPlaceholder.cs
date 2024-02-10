// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public partial class NoScoresPlaceholder : Container
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
                    text.Text = BeatmapsetsStrings.ShowScoreboardNoScoresGlobal;
                    break;

                case BeatmapLeaderboardScope.Friend:
                    text.Text = BeatmapsetsStrings.ShowScoreboardNoScoresFriend;
                    break;

                case BeatmapLeaderboardScope.Country:
                    text.Text = BeatmapsetsStrings.ShowScoreboardNoScoresCountry;
                    break;
            }
        }
    }
}
