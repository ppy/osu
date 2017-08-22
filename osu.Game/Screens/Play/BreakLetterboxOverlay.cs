// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Play
{
    public class BreakLetterboxOverlay : Container, IStateful<Visibility>
    {
        private const int letterbox_height = 80;

        public double FadeDuration;

        private Visibility state;
        public Visibility State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;

                switch (state)
                {
                    case Visibility.Visible:
                        this.FadeIn(FadeDuration, Easing.OutQuint);
                        break;
                    case Visibility.Hidden:
                        this.FadeOut(FadeDuration, Easing.OutQuint);
                        break;
                }
            }
        }

        public BreakLetterboxOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = letterbox_height,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    }
                },
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = letterbox_height,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            State = Visibility.Hidden;
        }
    }
}
