// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// An element anchored to the right-hand area of a screen that provides streamer level controls.
    /// Should be off-screen.
    /// </summary>
    public class ControlPanel : Container
    {
        private readonly FillFlowContainer buttons;

        protected override Container<Drawable> Content => buttons;

        public ControlPanel()
        {
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Width = 0.15f;
            Anchor = Anchor.TopRight;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(54, 54, 54, 255)
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    Text = "Control Panel",
                    TextSize = 22f,
                    Font = "Exo2.0-Bold"
                },
                buttons = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.75f,

                    Position = new Vector2(0, 35f),

                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5f),
                },
            };
        }

        public class Spacer : CompositeDrawable
        {
            public Spacer(float height = 20)
            {
                RelativeSizeAxes = Axes.X;
                Height = height;
                AlwaysPresent = true;
            }
        }
    }
}
