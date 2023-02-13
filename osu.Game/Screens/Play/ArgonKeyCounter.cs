// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Play
{
    public partial class ArgonKeyCounter : KeyCounter
    {
        private Circle inputIndicator = null!;
        private OsuSpriteText countText = null!;

        private const float figma_line_height = 3;
        private const float figma_name_font_size = 10;
        private const float scale_factor = 1.5f;
        private const float figma_count_font_size = 14;

        public ArgonKeyCounter(InputTrigger trigger)
            : base(trigger)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                inputIndicator = new Circle
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = figma_line_height * scale_factor,
                    Alpha = 0.5f
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(0, -13) * scale_factor,
                    Font = OsuFont.Torus.With(size: figma_name_font_size * scale_factor, weight: FontWeight.Bold),
                    Colour = colours.Blue0,
                    Text = Name
                },
                countText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Font = OsuFont.Torus.With(size: figma_count_font_size * scale_factor, weight: FontWeight.Bold),
                    Text = "0"
                },
            };
            Height = 30 * scale_factor;
            Width = 35 * scale_factor;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            IsLit.BindValueChanged(e => inputIndicator.Alpha = e.NewValue ? 1 : 0.5f, true);
            PressesCount.BindValueChanged(e => countText.Text = e.NewValue.ToString(@"#,0"), true);
        }
    }
}
