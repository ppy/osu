using osu.Framework;
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
        private Box meterFill;
        private BindableDouble volume;

        public VolumeMeter(string meterName, BindableDouble volume)
        {
            this.volume = volume;
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
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre
                        }
                    }
                },
                new SpriteText {Text = meterName, Anchor = Anchor.BottomCentre,Origin = Anchor.BottomCentre,Position = new Vector2(0,-20)}
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
            updateFill();
        }

        protected override bool OnWheelUp(InputState state)
        {
            volume.Value += 0.05f;
            updateFill();
            return true;
        }

        protected override bool OnWheelDown(InputState state)
        {
            volume.Value -= 0.05f;
            updateFill();
            return true;
        }

        private void updateFill() => meterFill.ScaleTo(new Vector2(1, (float)volume.Value), 300, EasingTypes.OutQuint);
    }
}