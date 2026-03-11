// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RankedPlay;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public partial class RankedPlayStageDisplay : VisibilityContainer
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private readonly RankedPlayColourScheme colourScheme;

        private Drawable headingTextBackground = null!;
        private Drawable progressBar = null!;
        private OsuSpriteText progressText = null!;

        private OsuSpriteText? headingText;
        private OsuSpriteText? captionText;

        private DateTimeOffset countdownStartTime;
        private DateTimeOffset countdownEndTime;

        public RankedPlayStageDisplay(RankedPlayColourScheme colourScheme)
        {
            this.colourScheme = colourScheme;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float phase_text_background_height = 55;
            Vector2 progressBarSize = new Vector2(300, 25);
            MarginPadding progressBarMargin = new MarginPadding
            {
                Left = 40,
                Top = phase_text_background_height - progressBarSize.Y / 2
            };

            InternalChildren = new Drawable[]
            {
                new BufferedContainer
                {
                    AutoSizeAxes = Axes.Both,
                    BackgroundColour = colourScheme.Surface.Opacity(0),
                    Alpha = 0.7f,
                    Children = new[]
                    {
                        headingTextBackground = new Container
                        {
                            Height = phase_text_background_height,
                            Shear = OsuGame.SHEAR,
                            Masking = true,
                            CornerRadius = 3,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourScheme.Surface.Darken(0.1f),
                                Alpha = 0.8f
                            }
                        },
                        new Container
                        {
                            Size = progressBarSize,
                            Margin = progressBarMargin,
                            Shear = OsuGame.SHEAR,
                            Masking = true,
                            CornerRadius = 3,
                            BorderThickness = 1f,
                            BorderColour = ColourInfo.GradientVertical(colourScheme.Surface, colourScheme.SurfaceBorder),
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourScheme.Surface,
                            }
                        },
                    }
                },
                headingText = new OsuSpriteText
                {
                    Margin = new MarginPadding
                    {
                        Top = 5,
                        Left = 20,
                    },
                    Text = Heading,
                    Font = OsuFont.TorusAlternate.With(size: 34),
                    Shadow = false,
                },
                new Container
                {
                    Size = progressBarSize,
                    Shear = OsuGame.SHEAR,
                    Padding = new MarginPadding { Horizontal = 2.2f, Vertical = 2 },
                    Margin = progressBarMargin,
                    Children =
                    [
                        progressBar = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 2,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children =
                            [
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.8f,
                                    Colour = ColourInfo.GradientHorizontal(colourScheme.PrimaryDarker, colourScheme.Primary)
                                },
                                new TrianglesV2
                                {
                                    Width = progressBarSize.X,
                                    RelativeSizeAxes = Axes.Y,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    SpawnRatio = 0.5f,
                                    ScaleAdjust = 0.75f,
                                    Alpha = 0.1f,
                                    Blending = BlendingParameters.Additive,
                                    Colour = ColourInfo.GradientHorizontal(Color4.Transparent, Color4.White)
                                },
                            ],
                        },
                        progressText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Shear = -OsuGame.SHEAR,
                            Margin = new MarginPadding
                            {
                                Left = 10
                            },
                            UseFullGlyphHeight = false,
                            Text = "00:27:123",
                            Font = OsuFont.TorusAlternate.With(size: 16, fixedWidth: true, weight: FontWeight.SemiBold)
                        }
                    ]
                },
                captionText = new OsuSpriteText
                {
                    Margin = new MarginPadding
                    {
                        Top = 80,
                        Left = 20
                    },
                    Colour = CaptionColour ?? colourScheme.Primary,
                    Text = Caption,
                    Font = OsuFont.TorusAlternate.With(size: 24, weight: FontWeight.SemiBold)
                }
            };
        }

        private LocalisableString heading;

        /// <summary>
        /// Heading text to be displayed indicating the purpose of the current stage.
        /// </summary>
        public LocalisableString Heading
        {
            get => heading;
            set
            {
                heading = value;
                if (headingText != null)
                    headingText.Text = value;
            }
        }

        private LocalisableString caption;

        /// <summary>
        /// Subtitle text to be displayed indicating the action a user should take in the current stage.
        /// </summary>
        public LocalisableString Caption
        {
            get => caption;
            set
            {
                caption = value;
                if (captionText != null)
                    captionText.Text = value;
            }
        }

        private Color4? captionColour;

        /// <summary>
        /// Overrides the default caption colour from the colour scheme with a custom one.
        /// </summary>
        public Color4? CaptionColour
        {
            get => captionColour;
            set
            {
                captionColour = value;
                if (captionText != null)
                    captionText.Colour = value ?? colourScheme.Primary;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.CountdownStarted += onCountdownStarted;
            client.CountdownStopped += onCountdownStopped;

            if (client.Room != null)
            {
                foreach (var countdown in client.Room.ActiveCountdowns)
                    onCountdownStarted(countdown);
            }
        }

        protected override void Update()
        {
            base.Update();

            headingTextBackground.Width = headingText!.DrawWidth + 80;

            TimeSpan duration = countdownEndTime - countdownStartTime;
            TimeSpan remaining = countdownEndTime - DateTimeOffset.Now;

            if (duration > TimeSpan.Zero)
                progressBar.Width = (float)Math.Clamp(remaining / duration, 0, 1);

            int minutes = (int)Math.Max(0, remaining.TotalMinutes);
            int seconds = Math.Max(0, remaining.Seconds);
            int ms = Math.Max(0, remaining.Milliseconds);

            progressText.Text = $"{minutes:00}:{seconds:00}.{ms:000}";
        }

        private void onCountdownStarted(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (countdown is not RankedPlayStageCountdown)
                return;

            countdownStartTime = DateTimeOffset.Now;
            countdownEndTime = DateTimeOffset.Now + countdown.TimeRemaining;
        });

        private void onCountdownStopped(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (countdown is not RankedPlayStageCountdown)
                return;

            countdownEndTime = DateTimeOffset.Now;
        });

        protected override void PopIn()
        {
            this.FadeIn();
        }

        protected override void PopOut()
        {
            this.FadeOut();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.CountdownStarted -= onCountdownStarted;
                client.CountdownStopped -= onCountdownStopped;
            }
        }
    }
}
