// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public partial class RankedPlayUserDisplay : CompositeDrawable
    {
        public readonly BindableInt Health = new BindableInt
        {
            MaxValue = 1_000_000,
            MinValue = 0,
            Value = 1_000_000,
        };

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private UserLookupCache users { get; set; } = null!;

        private readonly int userId;
        private readonly Anchor contentAnchor;
        private readonly RankedPlayColourScheme colourScheme;

        private BufferedContainer grayScaleContainer = null!;

        [Resolved]
        private RankedPlayCornerPiece? cornerPiece { get; set; }

        public RankedPlayUserDisplay(int userId, Anchor contentAnchor, RankedPlayColourScheme colourScheme)
        {
            this.userId = userId;
            this.contentAnchor = contentAnchor;
            this.colourScheme = colourScheme;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            APIUser user = users.GetUserAsync(userId).GetResultSafely()!;

            InternalChildren =
            [
                new CircularContainer
                {
                    Name = "Avatar",
                    Size = new Vector2(72),
                    Masking = true,
                    Anchor = contentAnchor,
                    Origin = contentAnchor,
                    Children =
                    [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourScheme.Surface,
                            Alpha = 0.5f,
                        },
                        grayScaleContainer = new BufferedContainer(cachedFrameBuffer: false, pixelSnapping: true)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = new UpdateableAvatar(user)
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        }
                    ]
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = (contentAnchor & Anchor.x0) != 0 ? new MarginPadding { Left = 72 } : new MarginPadding { Right = 72 },
                    Direction = FillDirection.Vertical,
                    Children =
                    [
                        new HealthBar(colourScheme, (contentAnchor & Anchor.x0) != 0)
                        {
                            Health = { BindTarget = Health },
                            RelativeSizeAxes = Axes.X,
                            Height = 22,
                            Anchor = contentAnchor,
                            Origin = contentAnchor,
                        },
                        new OsuSpriteText
                        {
                            Name = "Username",
                            Text = user.Username,
                            Anchor = contentAnchor,
                            Origin = contentAnchor,
                            Padding = new MarginPadding { Horizontal = 4, Vertical = 6 },
                            Font = OsuFont.GetFont(size: 24, weight: FontWeight.SemiBold),
                            UseFullGlyphHeight = false,
                        },
                    ]
                }
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchRoomStateChanged += onRoomStateChanged;

            Health.BindValueChanged(e =>
            {
                grayScaleContainer.GrayscaleTo(e.NewValue <= 0 ? 1 : 0, 300);
                cornerPiece?.OnHealthChanged(e.NewValue);
            });
        }

        private void onRoomStateChanged(MatchRoomState state) => Scheduler.Add(() =>
        {
            if (state is not RankedPlayRoomState rankedPlayState)
                return;

            Health.Value = rankedPlayState.Users[userId].Life;
        });

        protected override void Dispose(bool isDisposing)
        {
            client.MatchRoomStateChanged -= onRoomStateChanged;

            base.Dispose(isDisposing);
        }

        private partial class HealthBar : CompositeDrawable
        {
            private readonly bool leftToRight;

            public readonly BindableInt Health = new BindableInt
            {
                MaxValue = 1_000_000,
                MinValue = 0,
                Value = 1_000_000,
            };

            private readonly BindableInt healthTextValue = new BindableInt();

            /// <summary>
            /// relative health threshold below which the health bar starts flashing red
            /// </summary>
            public float HealthFlashThreshold { get; set; } = 0.3f;

            private readonly ColourInfo healthBarColour;

            private readonly Container healthBar;
            private readonly Box healthBarBackground;
            private readonly Container damageIndicator;
            private readonly TrianglesV2 triangles;
            private readonly SpriteIcon heartIcon;
            private readonly OsuSpriteText healthText;

            public HealthBar(RankedPlayColourScheme colourScheme, bool leftToRight)
            {
                this.leftToRight = leftToRight;

                Shear = OsuGame.SHEAR;

                Anchor contentAnchor = leftToRight ? Anchor.CentreLeft : Anchor.CentreRight;

                BufferedContainer content;

                InternalChildren =
                [
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 3,
                        BorderThickness = 1f,
                        BorderColour = ColourInfo.GradientVertical(colourScheme.Surface, colourScheme.SurfaceBorder),
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourScheme.Surface,
                            Alpha = 0.8f,
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = 2.2f, Vertical = 2 }, // slightly different ratio to account for shear
                        Children =
                        [
                            healthBar = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = 2,
                                Anchor = contentAnchor,
                                Origin = contentAnchor,
                                Children =
                                [
                                    healthBarBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0.8f,
                                        Colour = healthBarColour = leftToRight
                                            ? ColourInfo.GradientHorizontal(colourScheme.PrimaryDarker, colourScheme.Primary)
                                            : ColourInfo.GradientHorizontal(colourScheme.Primary, colourScheme.PrimaryDarker),
                                    },
                                    triangles = new TrianglesV2
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Anchor = contentAnchor,
                                        Origin = contentAnchor,
                                        SpawnRatio = 0.5f,
                                        ScaleAdjust = 0.75f,
                                        Alpha = 0.1f,
                                        Blending = BlendingParameters.Additive,
                                        Colour = leftToRight
                                            ? ColourInfo.GradientHorizontal(Color4.Transparent, Color4.White)
                                            : ColourInfo.GradientHorizontal(Color4.White, Color4.Transparent),
                                    },
                                ],
                            },
                        ]
                    },
                    content = new BufferedContainer(pixelSnapping: true)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Shear = -OsuGame.SHEAR,
                        BackgroundColour = Color4.White.Opacity(0), // workaround for non-premultiplied alpha blending of white content on transparent background
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Spacing = new Vector2(3),
                            Padding = new MarginPadding { Horizontal = 10 },
                            Children =
                            [
                                new Container
                                {
                                    Size = new Vector2(10),
                                    Anchor = contentAnchor,
                                    Origin = contentAnchor,
                                    Child = heartIcon = new SpriteIcon
                                    {
                                        Icon = FontAwesome.Solid.Heart,
                                        RelativeSizeAxes = Axes.Both,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                    }
                                },
                                healthText = new OsuSpriteText
                                {
                                    Text = "1,000,000",
                                    Anchor = contentAnchor,
                                    Origin = contentAnchor,
                                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Medium, fixedWidth: true),
                                    Spacing = new Vector2(-1, 0),
                                    UseFullGlyphHeight = false,
                                    Padding = new MarginPadding { Top = 1 },
                                    Shadow = false,
                                }
                            ]
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = 2.2f, Vertical = 2 }, // slightly different ratio to account for shear
                        Children =
                        [
                            damageIndicator = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                Anchor = contentAnchor,
                                Origin = contentAnchor,
                                Masking = true,
                                CornerRadius = 2,
                                Alpha = 0,
                                EdgeEffect = new EdgeEffectParameters
                                {
                                    Type = EdgeEffectType.Glow,
                                    Radius = 25,
                                    Colour = Color4Extensions.FromHex("FF171B").Opacity(0.5f),
                                    Roundness = 10,
                                    Hollow = true,
                                },
                                Children =
                                [
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    content.CreateView().With(d =>
                                    {
                                        d.SynchronisedDrawQuad = true;
                                        d.Colour = Color4.Red;
                                    })
                                ],
                            },
                        ]
                    },
                ];
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Health.BindValueChanged(onHealthChanged, true);

                healthTextValue.BindValueChanged(e => healthText.Text = FormattableString.Invariant($"{e.NewValue:N0}"), true);

                FinishTransforms(true);

                Scheduler.AddDelayed(flashHealth, 1000, true);
            }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
            private float normalizedHealth;
            private float normalizedHealthWithDamage;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

            private void onHealthChanged(ValueChangedEvent<int> e)
            {
                this.TransformBindableTo(healthTextValue, e.NewValue, 500, Easing.OutExpo);

                bool isHealthDecrease = e.NewValue < e.OldValue;

                if (isHealthDecrease)
                {
                    damageIndicator.FadeIn(50)
                                   .Then(delay: 1100)
                                   .FadeOut(200);

                    healthBarBackground.FadeColour(Color4.Red, 100)
                                       .Then()
                                       .FadeColour(healthBarColour, 1000);

                    this.TransformTo(nameof(normalizedHealthWithDamage), Health.NormalizedValue, 400, Easing.OutExpo)
                        .Then(500)
                        .TransformTo(nameof(normalizedHealth), Health.NormalizedValue, 800, Easing.OutExpo);
                }

                else
                {
                    this.TransformTo(nameof(normalizedHealthWithDamage), Health.NormalizedValue, 800, Easing.OutExpo)
                        .TransformTo(nameof(normalizedHealth), Health.NormalizedValue, 800, Easing.OutExpo);
                }
            }

            protected override void Update()
            {
                base.Update();

                triangles.Width = DrawWidth;
                healthBar.Width = normalizedHealth;

                damageIndicator.X = leftToRight ? normalizedHealthWithDamage : -normalizedHealthWithDamage;
                damageIndicator.Width = float.Clamp(normalizedHealth - normalizedHealthWithDamage, 0, 1);
            }

            private void flashHealth()
            {
                if (Health.NormalizedValue > HealthFlashThreshold)
                    return;

                var almostRed = Interpolation.ValueAt(0.75, healthBarColour, ColourInfo.SingleColour(Color4.Red), 0.0, 1.0);

                healthBarBackground.FadeColour(almostRed, 150)
                                   .Then()
                                   .FadeColour(healthBarColour, 800);

                heartIcon
                    .ScaleTo(0.8f, 150, Easing.Out)
                    .Then()
                    .ScaleTo(1f, 400, Easing.OutElasticHalf);
            }
        }
    }
}
