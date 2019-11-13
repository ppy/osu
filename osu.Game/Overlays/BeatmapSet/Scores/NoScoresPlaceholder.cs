// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class NoScoresPlaceholder : Container
    {
        public readonly Bindable<BeatmapLeaderboardScope> Scope = new Bindable<BeatmapLeaderboardScope>();

        private readonly SpriteText text;

        public NoScoresPlaceholder()
        {
            AutoSizeAxes = Axes.Both;
            Child = text = new SpriteText
            {
                Font = OsuFont.GetFont(),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scope.BindValueChanged(scope => text.Text = getText(scope.NewValue), true);
        }

        private string getText(BeatmapLeaderboardScope scope)
        {
            switch (scope)
            {
                default:
                case BeatmapLeaderboardScope.Global:
                return @"No scores yet. Maybe should try setting some?";

                case BeatmapLeaderboardScope.Friend:
                return @"None of your friends has set a score on this map yet!";

                case BeatmapLeaderboardScope.Country:
                return @"No one from your country has set a score on this map yet!";
            }
        }
    }
}
