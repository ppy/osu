// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Intro
{
    /// <summary>
    /// A brief intro animation that introduces matchmaking to the user.
    /// </summary>
    public partial class IntroScreen : OsuScreen
    {
        public override bool DisallowExternalBeatmapRulesetChanges => false;

        public override bool? ApplyModTrackAdjustments => true;

        public override bool ShowFooter => true;

        private Container introContent = null!;

        private Container titleContainer = null!;

        private bool animationBegan;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        private Sample? dateWindupSample;
        private Sample? dateImpactSample;
        private Sample? beatmapWindupSample;
        private Sample? beatmapImpactSample;

        private SampleChannel? dateWindupChannel;
        private SampleChannel? dateImpactChannel;
        private SampleChannel? beatmapWindupChannel;
        private SampleChannel? beatmapImpactChannel;

        private IDisposable? duckOperation;

        protected override BackgroundScreen CreateBackground() => new MatchmakingIntroBackgroundScreen(colourProvider);

        public IntroScreen()
        {
            ValidForResume = false;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            InternalChildren = new Drawable[]
            {
                introContent = new Container
                {
                    Alpha = 0f,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Shear = OsuGame.SHEAR,
                    Children = new Drawable[]
                    {
                        titleContainer = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    X = 10,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = "Quick Play",
                                            Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                            Shear = -OsuGame.SHEAR,
                                            Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                                        },
                                    }
                                },
                            }
                        },
                    }
                }
            };

            dateWindupSample = audio.Samples.Get(@"DailyChallenge/date-windup");
            dateImpactSample = audio.Samples.Get(@"DailyChallenge/date-impact");
            beatmapWindupSample = audio.Samples.Get(@"DailyChallenge/beatmap-windup");
            beatmapImpactSample = audio.Samples.Get(@"DailyChallenge/beatmap-impact");
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            this.FadeInFromZero(400, Easing.OutQuint);

            updateAnimationState();
            playDateWindupSample();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            duckOperation?.Dispose();

            this.FadeOut(800, Easing.OutQuint);
            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (base.OnExiting(e))
                return true;

            duckOperation?.Dispose();
            return false;
        }

        private void updateAnimationState()
        {
            if (animationBegan)
                return;

            beginAnimation();
            animationBegan = true;
        }

        private void beginAnimation()
        {
            using (BeginDelayedSequence(200))
            {
                introContent.Show();

                titleContainer
                    .ScaleTo(2)
                    .Then()
                    .ScaleTo(1, 400, Easing.In);

                using (BeginDelayedSequence(150))
                {
                    Schedule(() =>
                    {
                        playDateImpactSample();
                        playBeatmapWindupSample();

                        duckOperation?.Dispose();
                        duckOperation = musicController.Duck(new DuckParameters
                        {
                            RestoreDuration = 1500f,
                        });
                    });
                }

                using (BeginDelayedSequence(1000))
                {
                    using (BeginDelayedSequence(100))
                    {
                        titleContainer
                            .ScaleTo(0.4f, 400, Easing.In)
                            .FadeOut(500, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(240))
                    {
                        Schedule(() =>
                        {
                            if (this.IsCurrentScreen())
                                this.Push(new ScreenQueue());
                        });
                    }
                }
            }
        }

        private void playDateWindupSample()
        {
            dateWindupChannel = dateWindupSample?.GetChannel();
            dateWindupChannel?.Play();
        }

        private void playDateImpactSample()
        {
            dateImpactChannel = dateImpactSample?.GetChannel();
            dateImpactChannel?.Play();
        }

        private void playBeatmapWindupSample()
        {
            beatmapWindupChannel = beatmapWindupSample?.GetChannel();
            beatmapWindupChannel?.Play();
        }

        private void playBeatmapImpactSample()
        {
            beatmapImpactChannel = beatmapImpactSample?.GetChannel();
            beatmapImpactChannel?.Play();
        }

        protected override void Dispose(bool isDisposing)
        {
            resetAudio();
            base.Dispose(isDisposing);
        }

        private void resetAudio()
        {
            dateWindupChannel?.Stop();
            dateImpactChannel?.Stop();
            beatmapWindupChannel?.Stop();
            beatmapImpactChannel?.Stop();
            duckOperation?.Dispose();
        }

        private partial class MatchmakingIntroBackgroundScreen : RoomBackgroundScreen
        {
            private readonly OverlayColourProvider colourProvider;

            public MatchmakingIntroBackgroundScreen(OverlayColourProvider colourProvider)
                : base(null)
            {
                this.colourProvider = colourProvider;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddInternal(new Box
                {
                    Depth = float.MinValue,
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5.Opacity(0.6f),
                });
            }
        }
    }
}
