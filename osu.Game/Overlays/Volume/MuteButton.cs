using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Volume
{
    public class MuteButton : Container, IHasCurrentValue<bool>
    {
        public Bindable<bool> Current { get; } = new Bindable<bool>();

        private Color4 hoveredColour, unhoveredColour;

        public MuteButton()
        {
            Masking = true;
            BorderThickness = 3;
            CornerRadius = 20;
            Size = new Vector2(100, 40);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            hoveredColour = colours.YellowDark;
            BorderColour = unhoveredColour = colours.Gray1.Opacity(0.9f);

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray1,
                    Alpha = 0.9f,
                },
            });
        }

        protected override bool OnHover(InputState state)
        {
            this.TransformTo<MuteButton, SRGBColour>("BorderColour", hoveredColour, 500, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            this.TransformTo<MuteButton, SRGBColour>("BorderColour", unhoveredColour, 500, Easing.OutQuint);
        }
    }
}
