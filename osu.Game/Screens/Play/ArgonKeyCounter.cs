// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.Play
{
    public partial class ArgonKeyCounter : KeyCounter
    {
        private Circle inputIndicator = null!;
        private OsuSpriteText keyNameText = null!;
        private OsuSpriteText countText = null!;

        // These values were taken from Figma
        private const float line_height = 3;
        private const float name_font_size = 10;
        private const float count_font_size = 14;

        // Make things look bigger without using Scale
        private const float scale_factor = 1.5f;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public ArgonKeyCounter(InputTrigger trigger)
            : base(trigger)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                inputIndicator = new Circle
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = line_height * scale_factor,
                    Alpha = 0.5f
                },
                keyNameText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(0, -13) * scale_factor,
                    Font = OsuFont.Torus.With(size: name_font_size * scale_factor, weight: FontWeight.Bold),
                    Colour = colours.Blue0,
                    Text = Trigger.Name
                },
                countText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Font = OsuFont.Torus.With(size: count_font_size * scale_factor, weight: FontWeight.Bold),
                },
            };

            // Values from Figma didn't match visually
            // So these were just eyeballed
            Height = 30 * scale_factor;
            Width = 35 * scale_factor;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CountPresses.BindValueChanged(e => countText.Text = e.NewValue.ToString(@"#,0"), true);
        }

        protected override void Activate(bool forwardPlayback = true)
        {
            base.Activate(forwardPlayback);

            keyNameText
                .FadeColour(Colour4.White, 10, Easing.OutQuint);

            inputIndicator
                .FadeIn(10, Easing.OutQuint)
                .MoveToY(0)
                .Then()
                .MoveToY(4, 60, Easing.OutQuint);
        }

        protected override void Deactivate(bool forwardPlayback = true)
        {
            base.Deactivate(forwardPlayback);

            keyNameText
                .FadeColour(colours.Blue0, 200, Easing.OutQuart);

            inputIndicator
                .MoveToY(0, 250, Easing.OutQuart)
                .FadeTo(0.5f, 250, Easing.OutQuart);
        }
    }
}
