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
using osu.Game.Graphics.Backgrounds;

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
        private PopUpDialogTriangles backingTriangles;
        private TextAwesome iconCircle;
        private SpriteText titleText;
        protected virtual string title => string.Empty;

        private FlowContainer header;
        private FlowContainer body;
        private FlowContainer content;

        protected OsuGameBase osuGameBase;
        protected virtual FontAwesome icon => FontAwesome.fa_warning;

        public override bool HandleInput => (Transforms.Count == 0);

        public PopUpDialog()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Width = width;
            RelativeSizeAxes = Axes.Y;
            State = Visibility.Hidden;
            Depth = float.MinValue;
            Children = new Drawable[]
            {
                backingTriangles = new PopUpDialogTriangles
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
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
        private void load(OsuGameBase osuGame)
        {
            osuGameBase = osuGame;
            content.Add(new Drawable[]
            {
                header = CreateHeader(),
                body = CreateBody(),
            });
            titleText.Text = title;
        }

        protected virtual FlowContainer CreateHeader()
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

        protected virtual FlowContainer CreateBody()
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

        public void Nest(PopUpDialog nestee, bool removeThis = true)
        {
            Container parent = Parent as Container;
            parent.Add(nestee);
            Hide();
            nestee.Delay(transition_length);
            nestee.Show();
            if (removeThis)
                Delay(transition_length);
                Expire();
        }

        protected override void PopIn()
        {
            iconCircle.Flush();
            iconCircle.Scale = new Vector2(0, 0);
            iconCircle.ScaleTo(1, transition_length * 1.5, EasingTypes.In);

            body.TransformSpacingTo(Vector2.Zero, transition_length * 1.5, EasingTypes.In);
            body.MoveTo(Vector2.Zero, transition_length, EasingTypes.In);

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

            body.Delay(transition_length);
            body.TransformSpacingTo(new Vector2(0, 40), transition_length, EasingTypes.Out);
            body.MoveTo(new Vector2(0, 60), transition_length, EasingTypes.Out);
        }

        class PopUpDialogTriangles : Triangles
        {
            private const float triangle_moving_speed = 720;
            private const float triangle_normal_speed = 2880;
            private const float triangle_size = 100;
            private float triangleMoveSpeed;
            private Vector2 size = new Vector2(triangle_size, triangle_size * 0.866f);

            public PopUpDialogTriangles()
            {
                Alpha = 1;
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

            protected override Framework.Graphics.Sprites.Triangle CreateTriangle()
            {
                var triangle = base.CreateTriangle();
                triangle.Alpha = RNG.NextSingle() * 0.3f;
                triangle.Size = size;
                triangle.Scale = new Vector2(RNG.NextSingle() * 0.6f + 0.2f);
                triangle.Colour = new Color4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 255);
                return triangle;
            }

            public void SlideIn(double duration)
            {
                triangleMoveSpeed = triangle_moving_speed;
                TransformFloatTo(triangleMoveSpeed, triangle_normal_speed, duration, EasingTypes.In, new TransformFloatSpeed());
            }

            public void SlideOut(double duration)
            {
                triangleMoveSpeed = triangle_moving_speed;
                TransformFloatTo(triangleMoveSpeed, triangle_normal_speed, duration, EasingTypes.In, new TransformFloatSpeed());
            }

            public class TransformFloatSpeed : TransformFloat
            {
                public override void Apply(Drawable d)
                {
                    base.Apply(d);
                    PopUpDialogTriangles bt = d as PopUpDialogTriangles;
                    bt.triangleMoveSpeed = CurrentValue;
                }
            }
        }
    }
}
