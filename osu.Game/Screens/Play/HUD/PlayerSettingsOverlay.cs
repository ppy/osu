// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Screens.Play.HUD
{
    public partial class PlayerSettingsOverlay : VisibilityContainer
    {
        private const int fade_duration = 200;

        public readonly VisualSettings VisualSettings;

        protected override Container<Drawable> Content => content;

        private readonly FillFlowContainer content;

        public PlayerSettingsOverlay()
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            AutoSizeAxes = Axes.Both;

            InternalChild = content = new FillFlowContainer
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new PlayerSettingsGroup[]
                {
                    VisualSettings = new VisualSettings { Expanded = { Value = false } },
                    new AudioSettings { Expanded = { Value = false } }
                }
            };
        }

        protected override void PopIn() => this.FadeIn(fade_duration);
        protected override void PopOut() => this.FadeOut(fade_duration);
    }
}
