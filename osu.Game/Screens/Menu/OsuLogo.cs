//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Menu
{
    /// <summary>
    /// osu! logo and its attachments (pulsing, visualiser etc.)
    /// </summary>
    public partial class OsuLogo : Container
    {
        public Color4 OsuPink = OsuColour.FromHex(@"e967a1");

        private Sprite logo;
        private CircularContainer logoContainer;
        private Container logoBounceContainer;
        private Container logoHoverContainer;
        private MenuVisualisation vis;

        private Container colourAndTriangles;

        public Action Action;

        public float SizeForFlow => logo == null ? 0 : logo.DrawSize.X * logo.Scale.X * logoBounceContainer.Scale.X * logoHoverContainer.Scale.X * 0.78f;

        private Sprite ripple;

        private Container rippleContainer;

        public bool Triangles
        {
            set
            {
                colourAndTriangles.Alpha = value ? 1 : 0;
            }
        }

        public override bool Contains(Vector2 screenSpacePos)
        {
            return logoContainer.Contains(screenSpacePos);
        }

        public bool Ripple
        {
            get { return rippleContainer.Alpha > 0; }
            set
            {
                rippleContainer.Alpha = value ? 1 : 0;
            }
        }

        public bool Interactive = true;
        private Box colourLayer;
        private Box flashLayer;

        public OsuLogo()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                logoBounceContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        logoHoverContainer = new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new BufferedContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        logoContainer = new CircularContainer
                                        {
                                            Anchor = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Scale = new Vector2(0.8f),
                                            Children = new Drawable[]
                                            {
                                                colourAndTriangles = new Container
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Children = new Drawable[]
                                                    {
                                                        colourLayer = new Box
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Colour = OsuPink,
                                                        },
                                                        new OsuLogoTriangles
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                        },
                                                    }
                                                },
                                                flashLayer = new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    BlendingMode = BlendingMode.Additive,
                                                    Colour = Color4.White,
                                                    Alpha = 0,
                                                },
                                            },
                                        },
                                        logo = new Sprite
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Scale = new Vector2(0.5f),
                                        },
                                    }
                                },
                                rippleContainer = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Children = new Drawable[]
                                    {
                                        ripple = new Sprite()
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            BlendingMode = BlendingMode.Additive,
                                            Scale = new Vector2(0.5f),
                                            Alpha = 0.15f
                                        }
                                    }
                                },
                                vis = new MenuVisualisation
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = logo.Size,
                                    BlendingMode = BlendingMode.Additive,
                                    Alpha = 0.2f,
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            logo.Texture = textures.Get(@"Menu/logo@2x");
            ripple.Texture = textures.Get(@"Menu/logo@2x");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ripple.ScaleTo(ripple.Scale * 1.1f, 500);
            ripple.FadeOut(500);
            ripple.Loop(300);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (!Interactive) return false;

            logoBounceContainer.ScaleTo(0.9f, 1000, EasingTypes.Out);
            return true;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {

            logoBounceContainer.ScaleTo(1f, 500, EasingTypes.OutElastic);
            return true;
        }

        protected override bool OnClick(InputState state)
        {
            if (!Interactive) return false;

            flashLayer.ClearTransformations();
            flashLayer.Alpha = 0.4f;
            flashLayer.FadeOut(1500, EasingTypes.OutExpo);

            Action?.Invoke();
            return true;
        }

        protected override bool OnHover(InputState state)
        {
            if (!Interactive) return false;
            logoHoverContainer.ScaleTo(1.2f, 500, EasingTypes.OutElastic);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            logoHoverContainer.ScaleTo(1, 500, EasingTypes.OutElastic);
        }

        class OsuLogoTriangles : Triangles
        {
            public Color4 OsuPinkLight = OsuColour.FromHex(@"ff7db7");
            public Color4 OsuPinkDark = OsuColour.FromHex(@"de5b95");

            public OsuLogoTriangles()
            {
                TriangleScale = 4;
                Alpha = 1;

            }
            
            protected override Triangle CreateTriangle()
            {
                var triangle = base.CreateTriangle();
                triangle.Alpha = 1;
                triangle.Colour = getTriangleShade();
                return triangle;
            }

            private Color4 getTriangleShade()
            {
                float val = RNG.NextSingle();
                return Interpolation.ValueAt(val,
                    OsuPinkDark,
                    OsuPinkLight,
                    0, 1);
            }
        }
    }
}