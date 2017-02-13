using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    class SpinnerCursorTrail : Container
    {
        public override bool HandleInput => true;
        private Spinner s;
        private Container whiteBar;
        private CircularContainer followCircle;

        public SpinnerCursorTrail(Spinner spinner)
        {
            s = spinner;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 0;
            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                whiteBar = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(300,5),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Position = new Vector2(45,0),
                            Colour = Color4.White,
                            ColourInfo = ColourInfo.GradientHorizontal(new Color4(255,255,255,0), Color4.White),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(50,5),
                        },
                        new Box
                        {
                            Position = new Vector2(95,0),
                            Colour = Color4.White,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(1080,5),
                        }
                    }
                },
                followCircle = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    BorderColour = Color4.White,
                    BorderThickness = 3,
                    Position = new Vector2(whiteBar.Size.X - 2,0),
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(20),
                            Colour = Color4.White,
                            Alpha = 0.01f,
                        }
                    }
                }
                
            };
        }

        private bool canCurrentlyTrack => Time.Current >= s.StartTime && Time.Current < s.EndTime;
        
        public float MousePosition;
        public float? MouseAngle;
        private bool tracking = false;
        public bool Tracking
        {
            get { return tracking; }
            set
            {
                if (value == tracking) return;
                else if (value == true)
                    FadeIn(100);
                else FadeOut(100);

                tracking = value;
            }
        }
        
        protected override void Update()
        {
            base.Update();

            whiteBar.Size = new Vector2(MousePosition - 8, 5);
            followCircle.Position = new Vector2(whiteBar.Size.X - 2, 0);
            RotateTo(MouseAngle ?? 0);
        
        }
    }
}
