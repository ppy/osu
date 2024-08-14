// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeIntro : OsuScreen
    {
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool? ApplyModTrackAdjustments => true;

        private readonly Room room;
        private readonly PlaylistItem item;

        private Container introContent = null!;
        private Container topTitleDisplay = null!;
        private Container bottomDateDisplay = null!;
        private Container beatmapBackground = null!;
        private Box flash = null!;

        private FillFlowContainer beatmapContent = null!;

        private Container titleContainer = null!;

        private bool beatmapBackgroundLoaded;

        private bool animationBegan;

        private IBindable<StarDifficulty?> starDifficulty = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [Cached]
        private readonly OnlinePlayBeatmapAvailabilityTracker beatmapAvailabilityTracker = new OnlinePlayBeatmapAvailabilityTracker();

        private bool shouldBePlayingMusic;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

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

        public DailyChallengeIntro(Room room)
        {
            this.room = room;
            item = room.Playlist.Single();

            ValidForResume = false;
        }

        protected override BackgroundScreen CreateBackground() => new DailyChallengeIntroBackgroundScreen(colourProvider);

        [BackgroundDependencyLoader]
        private void load(BeatmapDifficultyCache difficultyCache, BeatmapModelDownloader beatmapDownloader, OsuConfigManager config, AudioManager audio)
        {
            const float horizontal_info_size = 500f;

            Ruleset ruleset = Ruleset.Value.CreateInstance();

            StarRatingDisplay starRatingDisplay;

            InternalChildren = new Drawable[]
            {
                beatmapAvailabilityTracker,
                introContent = new Container
                {
                    Alpha = 0f,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Shear = new Vector2(OsuGame.SHEAR, 0f),
                    Children = new Drawable[]
                    {
                        titleContainer = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                topTitleDisplay = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    X = -10,
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
                                            Text = "Today's Challenge",
                                            Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                            Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                                        },
                                    }
                                },
                                bottomDateDisplay = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreLeft,
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
                                            Text = room.Name.Value.Split(':', StringSplitOptions.TrimEntries).Last(),
                                            Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                            Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                                        },
                                    }
                                },
                            }
                        },
                        beatmapContent = new FillFlowContainer
                        {
                            AlwaysPresent = true, // so we can get the size ahead of time
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Scale = new Vector2(0.001f),
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                beatmapBackground = new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Size = new Vector2(horizontal_info_size, 150f),
                                    CornerRadius = 20f,
                                    BorderColour = colourProvider.Content2,
                                    BorderThickness = 3f,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        flash = new Box
                                        {
                                            Colour = Color4.White,
                                            Blending = BlendingParameters.Additive,
                                            RelativeSizeAxes = Axes.Both,
                                            Depth = float.MinValue,
                                        }
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Width = horizontal_info_size,
                                    AutoSizeAxes = Axes.Y,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Direction = FillDirection.Vertical,
                                            Padding = new MarginPadding(5f),
                                            Children = new Drawable[]
                                            {
                                                new TruncatingSpriteText
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    MaxWidth = horizontal_info_size,
                                                    Text = item.Beatmap.BeatmapSet!.Metadata.GetDisplayTitleRomanisable(false),
                                                    Padding = new MarginPadding { Horizontal = 5f },
                                                    Font = OsuFont.GetFont(size: 26),
                                                },
                                                new TruncatingSpriteText
                                                {
                                                    Text = $"Difficulty: {item.Beatmap.DifficultyName}",
                                                    Font = OsuFont.GetFont(size: 20, italics: true),
                                                    MaxWidth = horizontal_info_size,
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                },
                                                new TruncatingSpriteText
                                                {
                                                    Text = $"by {item.Beatmap.Metadata.Author.Username}",
                                                    Font = OsuFont.GetFont(size: 16, italics: true),
                                                    MaxWidth = horizontal_info_size,
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                },
                                                starRatingDisplay = new StarRatingDisplay(default)
                                                {
                                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                                    Margin = new MarginPadding(5),
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                }
                                            }
                                        },
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Width = horizontal_info_size,
                                    AutoSizeAxes = Axes.Y,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new ModFlowDisplay
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            AutoSizeAxes = Axes.Both,
                                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                            Current =
                                            {
                                                Value = item.RequiredMods.Select(m => m.ToMod(ruleset)).ToArray()
                                            },
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            };

            starDifficulty = difficultyCache.GetBindableDifficulty(item.Beatmap);
            starDifficulty.BindValueChanged(star =>
            {
                if (star.NewValue != null)
                    starRatingDisplay.Current.Value = star.NewValue.Value;
            }, true);

            LoadComponentAsync(new OnlineBeatmapSetCover(item.Beatmap.BeatmapSet as IBeatmapSetOnlineInfo)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fit,
                Scale = new Vector2(1.2f),
                Shear = new Vector2(-OsuGame.SHEAR, 0f),
            }, c =>
            {
                beatmapBackground.Add(c);

                beatmapBackgroundLoaded = true;
                updateAnimationState();
            });

            if (config.Get<bool>(OsuSetting.AutomaticallyDownloadMissingBeatmaps))
            {
                if (!beatmapManager.IsAvailableLocally(new BeatmapSetInfo { OnlineID = item.Beatmap.BeatmapSet!.OnlineID }))
                    beatmapDownloader.Download(item.Beatmap.BeatmapSet!, config.Get<bool>(OsuSetting.PreferNoVideo));
            }

            dateWindupSample = audio.Samples.Get(@"DailyChallenge/date-windup");
            dateImpactSample = audio.Samples.Get(@"DailyChallenge/date-impact");
            beatmapWindupSample = audio.Samples.Get(@"DailyChallenge/beatmap-windup");
            beatmapImpactSample = audio.Samples.Get(@"DailyChallenge/beatmap-impact");
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            beatmapAvailabilityTracker.SelectedItem.Value = item;
            beatmapAvailabilityTracker.Availability.BindValueChanged(availability =>
            {
                if (shouldBePlayingMusic && availability.NewValue.State == DownloadState.LocallyAvailable)
                    DailyChallenge.TrySetDailyChallengeBeatmap(this, beatmapManager, rulesets, musicController, item);
            }, true);

            this.FadeInFromZero(400, Easing.OutQuint);
            updateAnimationState();

            playDateWindupSample();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.FadeOut(800, Easing.OutQuint);
            base.OnSuspending(e);
        }

        private void updateAnimationState()
        {
            if (!beatmapBackgroundLoaded || !this.IsCurrentScreen())
                return;

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

                const float y_offset_start = 260;
                const float y_offset_end = 20;

                topTitleDisplay
                    .FadeInFromZero(400, Easing.OutQuint);

                topTitleDisplay.MoveToY(-y_offset_start)
                               .MoveToY(-y_offset_end, 300, Easing.OutQuint)
                               .Then()
                               .MoveToY(0, 4000);

                bottomDateDisplay.MoveToY(y_offset_start)
                                 .MoveToY(y_offset_end, 300, Easing.OutQuint)
                                 .Then()
                                 .MoveToY(0, 4000);

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

                    using (BeginDelayedSequence(2750))
                    {
                        Schedule(() =>
                        {
                            duckOperation?.Dispose();
                        });
                    }
                }

                using (BeginDelayedSequence(1000))
                {
                    beatmapContent
                        .ScaleTo(3)
                        .ScaleTo(1f, 500, Easing.In)
                        .Then()
                        .ScaleTo(1.1f, 4000);

                    using (BeginDelayedSequence(100))
                    {
                        titleContainer
                            .ScaleTo(0.4f, 400, Easing.In)
                            .FadeOut(500, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(240))
                    {
                        beatmapContent.FadeInFromZero(280, Easing.InQuad);

                        using (BeginDelayedSequence(300))
                        {
                            Schedule(() =>
                            {
                                shouldBePlayingMusic = true;
                                DailyChallenge.TrySetDailyChallengeBeatmap(this, beatmapManager, rulesets, musicController, item);
                                ApplyToBackground(bs => ((RoomBackgroundScreen)bs).SelectedItem.Value = item);
                                playBeatmapImpactSample();
                            });
                        }

                        using (BeginDelayedSequence(400))
                            flash.FadeOutFromOne(5000, Easing.OutQuint);

                        using (BeginDelayedSequence(2600))
                        {
                            Schedule(() =>
                            {
                                if (this.IsCurrentScreen())
                                    this.Push(new DailyChallenge(room));
                            });
                        }
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

        private partial class DailyChallengeIntroBackgroundScreen : RoomBackgroundScreen
        {
            private readonly OverlayColourProvider colourProvider;

            public DailyChallengeIntroBackgroundScreen(OverlayColourProvider colourProvider)
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
