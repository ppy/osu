// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK;

namespace osu.Game.Screens.Play.Break
{
    public class BreakSettingsOverlay : VisibilityContainer
    {
        private const int fade_duration = 200;

        public BreakSettingsOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Child = new FillFlowContainer<PlayerSettingsGroup>
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Margin = new MarginPadding { Top = 100, Right = 10 },
                Children = new PlayerSettingsGroup[]
                {
                    new VisualSettings()// { Expanded = false }
                }
            };
        }

        protected override void PopIn() => this.FadeIn(fade_duration);
        protected override void PopOut() => this.FadeOut(fade_duration);
    }
}
