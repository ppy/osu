using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game
{
    internal class VolumeMeter : Container
    {
        public Box MeterFill { get; set; }

        public BindableDouble Volume { get; set; }

        public VolumeMeter(string meterName, BindableDouble volume)
        {
            Volume = volume;
            Size = new Vector2(40, 180);
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
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
                            RelativeSizeAxes = Axes.Both,
                        },
                        MeterFill = new Box
                        {
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre
                        },
                    }
                },
                new SpriteText {Text = meterName, Anchor = Anchor.BottomCentre,Origin = Anchor.BottomCentre,Position = new Vector2(0,-20)}
            };
        }

        public override void Load()
        {
            base.Load();
            updateFill();
        }

        protected override bool OnWheelUp(InputState state)
        {
            Volume.Value += 0.05f;
            updateFill();
            return base.OnWheelUp(state);
        }

        protected override bool OnWheelDown(InputState state)
        {
            Volume.Value -= 0.05f;
            updateFill();
            return base.OnWheelDown(state);
        }

        private void updateFill() => MeterFill.ScaleTo(new Vector2(1, (float)Volume.Value), 300, EasingTypes.OutQuint);
    }
}