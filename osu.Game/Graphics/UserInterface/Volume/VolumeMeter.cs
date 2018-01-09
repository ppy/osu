// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;

namespace osu.Game.Graphics.UserInterface.Volume
{
    public class VolumeMeter : Container, IKeyBindingHandler<GlobalAction>
    {
        private readonly Box meterFill;
        public BindableDouble Bindable { get; } = new BindableDouble();

        public VolumeMeter(string meterName)
        {
            Size = new Vector2(40, 180);
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 0.9f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.DarkGray,
                            RelativeSizeAxes = Axes.Both
                        },
                        meterFill = new Box
                        {
                            Colour = Color4.White,
                            Scale = new Vector2(1, 0),
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre
                        }
                    }
                },
                new OsuSpriteText
                {
                    Text = meterName,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre
                }
            };

            Bindable.ValueChanged += delegate { updateFill(); };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateFill();
        }

        public double Volume
        {
            get { return Bindable.Value; }
            private set
            {
                Bindable.Value = value;
            }
        }

        public void Increase()
        {
            Volume += 0.05f;
        }

        public void Decrease()
        {
            Volume -= 0.05f;
        }

        private void updateFill() => meterFill.ScaleTo(new Vector2(1, (float)Volume), 300, Easing.OutQuint);

        public bool OnPressed(GlobalAction action)
        {
            if (!IsHovered) return false;

            switch (action)
            {
                case GlobalAction.DecreaseVolume:
                    Decrease();
                    return true;
                case GlobalAction.IncreaseVolume:
                    Increase();
                    return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;
    }
}
