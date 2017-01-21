using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SpinnerProgress : CircularContainer
    {
        private CircularContainer gray;
        private Container white;

        public SpinnerProgress(Spinner s)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;
            Alpha = 1;

            Children = new Drawable[]
            {
                gray = new CircularContainer
                {
                    Size = new Vector2(195),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    BorderThickness = 1,
                    BorderColour = Color4.Gray,
                    Alpha = 0.64f,
                    Children = new[]
                    {
                        new Box
                        {
                            Colour = Color4.Gray,
                            Alpha = 0.01f,
                            Width = 195,
                            Height = 195,
                        }
                    }
                },
                white = new Container
                {
                    Size = new Vector2(197.5f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 1f,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Masking = true,
                            Size = new Vector2(5,3),
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            CornerRadius = 1.5f,
                            Children = new[]
                            {
                                new Box
                                {
                                    Colour = Color4.White,
                                    Alpha = 1f,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(5,3)
                                }
                            }
                        }
                    }
                },
                new Container
                {
                    Size = new Vector2(197.5f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 1f,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Masking = true,
                            Size = new Vector2(5,3),
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            CornerRadius = 1.5f,
                            Children = new[]
                            {
                                new Box
                                {
                                    Colour = Color4.White,
                                    Alpha = 1f,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(5,3)
                                }
                            }
                        }
                    }
                }
            };

        }

        public float Progress = 0;
        public bool IsSpinningLeft;

        protected override void Update()
        {
            base.Update();
            white.RotateTo(Progress * 360,100);
            if (IsSpinningLeft)
                RotateTo(Progress * -360, 100);
            else
                RotateTo(0, 100);
        }
    }
}
