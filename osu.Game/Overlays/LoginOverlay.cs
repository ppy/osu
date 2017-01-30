//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics;
using osu.Game.Overlays.Options.General;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    class LoginOverlay : OverlayContainer
    {
        private LoginOptions optionsSection;

        public LoginOverlay()
        {
            Width = 360;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.8f,
                },
                optionsSection = new LoginOptions()
                {
                    Padding = new MarginPadding(10),
                },
                new Box {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Colour = colours.Yellow,
                    Alpha = 1,
                },
            };
        }

        protected override void PopIn()
        {
            optionsSection.ScaleTo(new Vector2(1, 1), 300, EasingTypes.OutExpo);
            FadeIn(200);
        }

        protected override void PopOut()
        {
            optionsSection.ScaleTo(new Vector2(1, 0), 300, EasingTypes.OutExpo);
            FadeOut(200);
        }
    }
}
