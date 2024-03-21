// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// An element anchored to the right-hand area of a screen that provides streamer level controls.
    /// Should be off-screen.
    /// </summary>
    public partial class ControlPanel : Container
    {
        private readonly FillFlowContainer buttons;

        protected override Container<Drawable> Content => buttons;

        public ControlPanel()
        {
            RelativeSizeAxes = Axes.Y;
            AlwaysPresent = true;
            Width = TournamentSceneManager.CONTROL_AREA_WIDTH;
            Anchor = Anchor.TopRight;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(54, 54, 54, 255)
                },
                new TournamentSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = "Control Panel",
                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 22)
                },
                buttons = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Position = new Vector2(0, 35f),
                    Padding = new MarginPadding(5),
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5f),
                },
            };
        }

        public partial class Spacer : CompositeDrawable
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
