//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Audio;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Framework.MathUtils;
using System;
using System.Linq;

namespace osu.Game.Overlays.PopUpDialogs
{
    public class PopUpDialog : OverlayContainer
    {

        protected const float width = 500;
        protected const float background_box_height = 3000;
        protected const float button_width = 400;
        protected const float button_background_width = 500;
        protected const float button_height = 50;
        protected const int transition_length = 400;
        protected const float icon_circle_width = 90;
        protected const float icon_circle_height = 90;

        private Box backgroundBox;
        private BackingTriangles backingTriangles;
        private TextAwesome iconCircle;
        private SpriteText titleText;
        protected virtual string title => string.Empty;

        private Container<Drawable> header;
        private Container<Drawable> body;
        private FlowContainer content;

        protected OsuGameBase osuGameBase;
        protected virtual FontAwesome icon => FontAwesome.fa_warning;

        public PopUpDialog()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Width = width;
            RelativeSizeAxes = Axes.Y;
            State = Visibility.Hidden;
            Depth = float.MaxValue;
            Children = new Drawable[]
            {
                backingTriangles = new BackingTriangles
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                backgroundBox = new Box
                {
                    Colour = new Color4(0, 0, 0, 255),
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Height = background_box_height,
                    Alpha = 0.80f,
                },
                content = new FlowContainer
                {
                    Direction = FlowDirection.VerticalOnly,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Width = icon_circle_width, //Fixed size so that we don't change size during iconCircle's animation
                            Height = icon_circle_height,
                            Padding = new MarginPadding
                            {
                                Bottom = 40,
                            },
                            Children = new Drawable[]
                            {
                                iconCircle = new TextAwesome
                                {
                                    Icon = FontAwesome.fa_circle_thin,
                                    TextSize = 90,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                                new TextAwesome
                                {
                                    Icon = icon,
                                    TextSize = 40,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                                titleText = new SpriteText
                                {
                                    Text = string.Empty,
                                    TextSize = 17,
                                    Font = @"Exo2.0-Bold",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Position = new Vector2(0, 50),
                                },
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame, Database.BeatmapDatabase beatmaps, AudioManager audio, TextureStore textures)
        {
            osuGameBase = osuGame;
            content.Add(new Drawable[]
            {
                header = CreateHeader(),
                body = CreateBody(),
            });
            titleText.Text = title;
        }

        protected virtual Container<Drawable> CreateHeader()
        {
            FlowContainer headCont = new FlowContainer
            {
                Direction = FlowDirection.VerticalOnly,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
                Position = new Vector2(0, -40),
            };
            return headCont;
        }

        protected virtual Container<Drawable> CreateBody()
        {
            FlowContainer bodyCont = new FlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FlowDirection.VerticalOnly,
            };
            return bodyCont;
        }

        public void Nest(PopUpDialog nestee)
        {
            Container parent = Parent as Container;
            parent.Add(nestee);
            Hide();
            nestee.Position = new Vector2(0, ScreenSpaceDrawQuad.Height);
            nestee.Delay(transition_length);
            nestee.Show();
        }

        protected override void PopIn()
        {
            iconCircle.Flush();
            iconCircle.Scale = new Vector2(0, 0);
            iconCircle.ScaleTo(1, transition_length * 1.5, EasingTypes.In);
            Flush();
            Position = new Vector2(0, ScreenSpaceDrawQuad.Height);
            backingTriangles.SlideIn(transition_length * 1.5);
            MoveToY(0, transition_length, EasingTypes.In);
            FadeIn(transition_length, EasingTypes.In);
        }

        protected override void PopOut()
        {
            backingTriangles.SlideOut(transition_length * 1.5);
            Delay(transition_length);
            MoveToY(-ScreenSpaceDrawQuad.Height, transition_length, EasingTypes.Out);
            FadeOut(transition_length, EasingTypes.Out);
        }

        class BackingTriangles : Container
        {
            private Texture triangle;
            private const int num_triangles = 300;
            private const float triangleMovingSpeed = 720;
            private const float triangleNormalSpeed = 2880;
            private float triangleMoveSpeed;

            public BackingTriangles()
            {
                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                triangle = textures.Get(@"Play/osu/triangle@2x");
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                for (int i = 0; i < num_triangles; i++)
                {
                    Add(new Sprite
                    {
                        Texture = triangle,
                        Origin = Anchor.Centre,
                        RelativePositionAxes = Axes.Both,
                        Position = new Vector2(RNG.NextSingle() + RNG.NextSingle(-0.1f, 0.1f), RNG.NextSingle() + RNG.NextSingle(-0.1f, 0.1f)),
                        Scale = new Vector2(RNG.NextSingle() * 0.4f + 0.2f),
                        Alpha = RNG.NextSingle() * 0.3f,
                        Colour = new Color4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 255),
                    });
                }
            }

            protected override void Update()
            {
                base.Update();

                foreach (Drawable d in Children)
                {
                    d.Position -= new Vector2(0, (float)(d.Scale.X * (Time.Elapsed / triangleMoveSpeed)));
                    if (d.DrawPosition.Y + d.DrawSize.Y * d.Scale.Y < 0)
                        d.MoveToY(1);
                }
            }

            public void SlideIn(double duration)
            {
                triangleMoveSpeed = triangleMovingSpeed;
                TransformFloatTo(triangleMoveSpeed, triangleNormalSpeed, duration, EasingTypes.In, new TransformFloatSpeed());
            }

            public void SlideOut(double duration)
            {
                triangleMoveSpeed = triangleMovingSpeed;
                TransformFloatTo(triangleMoveSpeed, triangleNormalSpeed, duration, EasingTypes.In, new TransformFloatSpeed());
            }


            public class TransformFloatSpeed : TransformFloat
            {
                public override void Apply(Drawable d)
                {
                    base.Apply(d);
                    BackingTriangles bt = d as BackingTriangles;
                    bt.triangleMoveSpeed = CurrentValue;
                }
            }
        }
    }
}
