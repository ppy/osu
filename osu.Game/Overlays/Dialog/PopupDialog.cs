// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Dialog
{
    public class PopupDialog : OverlayContainer
    {
        private const float header_body_offset = 4;
        private readonly Vector2 ring_size = new Vector2(100f);
        private readonly Vector2 ring_minified_size = new Vector2(20f);
        private readonly Vector2 buttons_enter_spacing = new Vector2(0f, 100f);

        private const float enter_duration = 500;
        private readonly EasingTypes enter_easing = EasingTypes.OutQuint;

        private const float exit_duration = 500;
        private const float button_fade_duration = 200;
        private readonly EasingTypes exit_easing = EasingTypes.InSine;

        private PopupDialogTriangles triangles;
        private Container dialogContainer, iconRing, headerBodyContainer;
        private TextAwesome iconText;
        private OsuSpriteText contextLabel, headerLabel, bodyLabel;
        private FlowContainer buttonsContainer;

        public FontAwesome Icon
        {
            get
            {
                return iconText.Icon;
            }
            set
            {
                iconText.Icon = value;
            }
        }

        public string ContextText
        {
            get
            {
                return contextLabel.Text;
            }
            set
            {
                contextLabel.Text = value.ToUpper();
            }
        }

        public string HeaderText
        {
            get
            {
                return headerLabel.Text;
            }
            set
            {
                headerLabel.Text = value;
            }
        }

        public string BodyText
        {
            get
            {
                return bodyLabel.Text;
            }
            set
            {
                bodyLabel.Text = value;
            }
        }

        public PopupDialogButton[] Buttons
        {
            get
            {
                // buttonsContainer cannot be a FlowContainer<PopupDialogButton> because there is a crash on TransformSpacing if it is (probably a bug and will be fixed)

                var buttons = new List<PopupDialogButton>();
                foreach (Container c in buttonsContainer.Children)
                {
                    var button = (PopupDialogButton)c;
                    if (button != null) buttons.Add(button);
                }
                return buttons.ToArray();
            }
            set
            {
                buttonsContainer.Children = value;

                // Simple way to allow direct action setting on the button but we can still call our own logic here
                foreach (PopupDialogButton b in value)
                {
                    b.AlwaysPresent = true;
                    var action = b.Action;
                    b.Action = () =>
                    {
                        fadeOutAllBut(b);
                        Hide();
                        action?.Invoke();
                    };
                }
            }
        }

        protected override void PopIn()
        {
            // Reset various animations, but only if the entire dialog animation completed
            if (dialogContainer.Alpha == 0)
            {
                iconRing.ResizeTo(ring_minified_size);
                buttonsContainer.TransformSpacingTo(buttons_enter_spacing);
                headerBodyContainer.Alpha = 0;
            }

            foreach (PopupDialogButton b in Buttons)
                b.FadeIn(button_fade_duration, enter_easing);

            triangles.SlideIn();
            dialogContainer.FadeIn(enter_duration, enter_easing);
            iconRing.ResizeTo(ring_size, enter_duration, enter_easing);
            headerBodyContainer.FadeIn(enter_duration, enter_easing);
            buttonsContainer.MoveToY(0, enter_duration, enter_easing);
            buttonsContainer.TransformSpacingTo(new Vector2(0f), enter_duration, enter_easing);
        }

        protected override void PopOut()
        {
            triangles.SlideOut();
            dialogContainer.FadeOut(exit_duration, exit_easing);
            headerBodyContainer.FadeOut(exit_duration, exit_easing);
        }

        private void fadeOutAllBut(PopupDialogButton button)
        {
            foreach (PopupDialogButton b in Buttons)
            {
                if (b != button)
                {
                    b.FadeOut(button_fade_duration, exit_easing);
                }
            }
        }

        public PopupDialog()
        {
            Children = new Drawable[]
            {
                triangles = new PopupDialogTriangles
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Width = 0.5f,
                },
                dialogContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Width = 0.4f,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(200),
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.5f,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.BottomCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        new FlowContainer
                                        {
                                            Origin = Anchor.BottomCentre,
                                            Anchor = Anchor.TopCentre,
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FlowDirections.Vertical,
                                            Spacing = new Vector2(0f, 15f),
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    Origin = Anchor.TopCentre,
                                                    Anchor = Anchor.TopCentre,
                                                    Size = ring_size,
                                                    Children = new Drawable[]
                                                    {
                                                        iconRing = new CircularContainer
                                                        {
                                                            Origin = Anchor.Centre,
                                                            Anchor = Anchor.Centre,
                                                            BorderColour = Color4.White,
                                                            BorderThickness = 10f,
                                                            Size = ring_minified_size,
                                                            Children = new Drawable[]
                                                            {
                                                                new Box
                                                                {
                                                                    RelativeSizeAxes = Axes.Both,
                                                                    Colour = Color4.Black.Opacity(0),
                                                                },
                                                                iconText = new TextAwesome
                                                                {
                                                                    Origin = Anchor.Centre,
                                                                    Anchor = Anchor.Centre,
                                                                    Icon = FontAwesome.fa_close,
                                                                    TextSize = 50,
                                                                },
                                                            },
                                                        },
                                                    }
                                                },
                                                contextLabel = new OsuSpriteText
                                                {
                                                    Origin = Anchor.TopCentre,
                                                    Anchor = Anchor.TopCentre,
                                                    Text = @"CONTEXT",
                                                    Font = @"Exo2.0-Bold",
                                                },
                                            },
                                        },
                                        headerBodyContainer = new Container
                                        {
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                            RelativeSizeAxes = Axes.X,
                                            Height = 100,
                                            Alpha = 0,
                                            Children = new Drawable[]
                                            {
                                                headerLabel = new OsuSpriteText
                                                {
                                                    Origin = Anchor.BottomCentre,
                                                    Anchor = Anchor.Centre,
                                                    Position = new Vector2(0f, -header_body_offset),
                                                    Text = @"Header",
                                                    Font = @"Exo2.0-Bold",
                                                    TextSize = 18,
                                                    Alpha = 0.75f,
                                                    BlendingMode = BlendingMode.Additive,
                                                },
                                                bodyLabel = new OsuSpriteText
                                                {
                                                    Origin = Anchor.TopCentre,
                                                    Anchor = Anchor.Centre,
                                                    Position = new Vector2(0f, header_body_offset),
                                                    Text = @"Body",
                                                    Font = @"Exo2.0-BoldItalic",
                                                    TextSize = 18,
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                        },
                        buttonsContainer = new FlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.5f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Direction = FlowDirections.Vertical,
                            Spacing = buttons_enter_spacing,
                            Position = new Vector2(0f, buttons_enter_spacing.Y),
                        },
                    },
                },
            };
        }
    }

    class PopupDialogTriangles : Triangles
    {
        private const float enter_duration = 500;
        private const float exit_duration = 500;
        private const float triangle_enter_speed = 100;
        private const float triangle_exit_speed = 300;
        private const float triangle_normal_speed = 20000;
        private float triangleMoveSpeed;

        private Color4[] colours;

        protected override float SpawnRatio => spawnRatio;
        private float spawnRatio = 0f;

        protected override Color4 GetTriangleShade() => colours[RNG.Next(0, colours.Length - 1)];

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            colours = new Color4[]
            {
                colour.BlueLight,
                colour.Blue,
                colour.PinkLight,
                colour.Pink,
                colour.YellowLight,
                colour.Yellow,
                colour.PurpleLight,
                colour.Purple,
            };
        }

        protected override void Update()
        {
            base.Update();
            foreach (Drawable d in Children)
                d.Position -= new Vector2(0, (float)(d.Scale.X * (Time.Elapsed / triangleMoveSpeed)));
        }

        public void SlideIn()
        {
            triangleMoveSpeed = triangle_enter_speed;
            TransformFloatTo(spawnRatio, 1f, enter_duration, EasingTypes.None, new TransformFloatSpawnRatio());
            TransformFloatTo(triangleMoveSpeed, triangle_normal_speed, enter_duration, EasingTypes.InExpo, new TransformFloatSpeed());
        }

        public void SlideOut()
        {
            TransformFloatTo(spawnRatio, 0f, exit_duration, EasingTypes.None, new TransformFloatSpawnRatio());
            TransformFloatTo(triangleMoveSpeed, triangle_exit_speed, exit_duration, EasingTypes.OutExpo, new TransformFloatSpeed());
        }

        class TransformFloatSpeed : TransformFloat
        {
            public override void Apply(Drawable d)
            {
                base.Apply(d);
                PopupDialogTriangles t = d as PopupDialogTriangles;
                t.triangleMoveSpeed = CurrentValue;
            }
        }

        class TransformFloatSpawnRatio : TransformFloat
        {
            public override void Apply(Drawable d)
            {
                base.Apply(d);
                PopupDialogTriangles t = d as PopupDialogTriangles;
                t.spawnRatio = CurrentValue;
            }
        }

        protected override void LoadComplete()
        {
            // override so the triangles don't do the initial fill
        }

        public PopupDialogTriangles()
        {
            TriangleScale = 2f;
        }
    }
}