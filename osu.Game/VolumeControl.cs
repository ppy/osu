using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Input;
using osu.Framework.Graphics.Transformations;
using osu.Framework;

namespace osu.Game
{
    internal class VolumeControl : Container
    {
        private Box meterFill;
        private Container meterContainer;

        public BindableDouble VolumeGlobal { get; set; }
        public BindableDouble VolumeSample { get; set; }
        public BindableDouble VolumeTrack { get; set; }

        public VolumeControl()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
            Children = new Drawable[]
            {
                meterContainer = new Container {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Position = new Vector2(10, 10),
                    Size = new Vector2(40, 180),
                    Alpha = 0,
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
                                meterFill = new Box
                                {
                                    Colour = Color4.White,
                                    RelativeSizeAxes = Axes.Both,
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.BottomCentre
                                },
                            }
                        }
                    }
                }
            };

            updateFill();
        }

        protected override bool OnWheelDown(InputState state)
        {
            appear();

            VolumeGlobal.Value -= 0.05f;
            updateFill();

            return base.OnWheelDown(state);
        }

        protected override bool OnWheelUp(InputState state)
        {
            appear();

            VolumeGlobal.Value += 0.05f;
            updateFill();

            return base.OnWheelUp(state);
        }

        private void updateFill()
        {
            meterFill.ScaleTo(new Vector2(1, (float)VolumeGlobal.Value), 300, EasingTypes.OutQuint);
        }

        private void appear()
        {
            meterContainer.ClearTransformations();
            meterContainer.FadeIn(100);
            meterContainer.Delay(1000);
            meterContainer.FadeOut(100);
        }
    }
}