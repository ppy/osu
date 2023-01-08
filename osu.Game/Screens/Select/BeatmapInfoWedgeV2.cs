// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.Select
{
    [Cached]
    public partial class BeatmapInfoWedgeV2 : VisibilityContainer
    {
        private const float shear_width = 21;
        private const int wedge_height = 120;

        private const float transition_duration = 250;

        private static readonly Vector2 wedged_container_shear = new Vector2(shear_width / wedge_height, 0);

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; }

        protected Container DisplayedContent { get; private set; }

        protected WedgeInfoText Info { get; private set; }

        private IBindable<StarDifficulty?> starDifficulty = new Bindable<StarDifficulty?>();
        private CancellationTokenSource cancellationSource;

        private readonly Container difficultyColourBar;
        private readonly StarCounter starCounter;

        public BeatmapInfoWedgeV2()
        {
            CornerRadius = 10;
            Shear = wedged_container_shear;
            Masking = true;
            Alpha = 0;

            Children = new Drawable[]
            {
                difficultyColourBar = new Container
                {
                    Depth = float.MaxValue,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = 40,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                starCounter = new StarCounter
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.4f),
                    Shear = -wedged_container_shear,
                    X = -15,
                    Direction = FillDirection.Vertical
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ruleset.BindValueChanged(_ => updateDisplay());

            float starAngle = (float)(Math.Atan(shear_width / wedge_height) * (180 / Math.PI));

            //Applying the rotation directly to the StarCounter distorts the stars, hence it is applied to the child container
            starCounter.Children.First().Rotation = starAngle;

            //Makes sure the stars center themselves properly in the colour bar
            starCounter.Children.First().Anchor = Anchor.Centre;
            starCounter.Children.First().Origin = Anchor.Centre;
        }

        private const double animation_duration = 800;

        protected override void PopIn()
        {
            this.MoveToX(0, animation_duration, Easing.OutQuint);
            this.RotateTo(0, animation_duration, Easing.OutQuint);
            this.FadeIn(transition_duration);
        }

        protected override void PopOut()
        {
            this.MoveToX(-100, animation_duration, Easing.In);
            this.RotateTo(10, animation_duration, Easing.In);
            this.FadeOut(transition_duration * 2, Easing.In);
        }

        private WorkingBeatmap beatmap;

        public WorkingBeatmap Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value) return;

                beatmap = value;
                starDifficulty = difficultyCache.GetBindableDifficulty(value.BeatmapInfo, (cancellationSource = new CancellationTokenSource()).Token);

                updateDisplay();
            }
        }

        public override bool IsPresent => base.IsPresent || DisplayedContent == null; // Visibility is updated in the LoadComponentAsync callback

        private Container loadingInfo;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            cancellationSource?.Cancel();
        }

        private void updateDisplay()
        {
            Scheduler.AddOnce(perform);

            void perform()
            {
                void removeOldInfo()
                {
                    State.Value = beatmap == null ? Visibility.Hidden : Visibility.Visible;

                    DisplayedContent?.FadeOut(transition_duration);
                    DisplayedContent?.Expire();
                    DisplayedContent = null;
                }

                if (beatmap == null)
                {
                    removeOldInfo();
                    return;
                }

                LoadComponentAsync(loadingInfo = new Container
                {
                    Masking = true,
                    X = -30,
                    CornerRadius = 10,
                    RelativeSizeAxes = Axes.Both,
                    Depth = DisplayedContent?.Depth + 1 ?? 0,
                    Children = new Drawable[]
                    {
                        new BeatmapInfoWedgeBackground(beatmap) { Shear = -Shear },
                        Info = new WedgeInfoText(beatmap, starDifficulty) { Shear = -Shear }
                    }
                }, loaded =>
                {
                    // ensure we are the most recent loaded wedge.
                    if (loaded != loadingInfo) return;

                    removeOldInfo();
                    Add(DisplayedContent = loaded);
                });
            }
        }

        public partial class WedgeInfoText : Container
        {
            public OsuSpriteText TitleLabel { get; private set; }
            public OsuSpriteText ArtistLabel { get; private set; }

            private StarRatingDisplay starRatingDisplay;

            private ILocalisedBindableString titleBinding;
            private ILocalisedBindableString artistBinding;

            private readonly WorkingBeatmap working;
            private readonly IBindable<StarDifficulty?> starDifficulty;

            [Resolved]
            private IBindable<IReadOnlyList<Mod>> mods { get; set; }

            [Resolved]
            private BeatmapInfoWedgeV2 wedge { get; set; }

            [Resolved]
            private OsuColour colours { get; set; }

            private ModSettingChangeTracker settingChangeTracker;

            public WedgeInfoText(WorkingBeatmap working, IBindable<StarDifficulty?> starDifficulty)
            {
                this.working = working;
                this.starDifficulty = starDifficulty;
            }

            [BackgroundDependencyLoader]
            private void load(LocalisationManager localisation)
            {
                var beatmapInfo = working.BeatmapInfo;
                var metadata = beatmapInfo.Metadata;

                RelativeSizeAxes = Axes.Both;

                titleBinding = localisation.GetLocalisedBindableString(new RomanisableString(metadata.TitleUnicode, metadata.Title));
                artistBinding = localisation.GetLocalisedBindableString(new RomanisableString(metadata.ArtistUnicode, metadata.Artist));

                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Name = "Topright-aligned metadata",
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding { Top = 14, Right = shear_width / 2 },
                        AutoSizeAxes = Axes.Both,
                        Shear = wedged_container_shear,
                        Spacing = new Vector2(0f, 5f),
                        Children = new Drawable[]
                        {
                            starRatingDisplay = new StarRatingDisplay(default, animated: true)
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Shear = -wedged_container_shear,
                                Alpha = 0f,
                            },
                            new BeatmapSetOnlineStatusPill
                            {
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Shear = -wedged_container_shear,
                                TextSize = 11,
                                TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                                Status = beatmapInfo.Status,
                                Alpha = string.IsNullOrEmpty(beatmapInfo.DifficultyName) ? 0 : 1
                            }
                        }
                    },
                    new FillFlowContainer
                    {
                        Name = "Top-left aligned metadata",
                        Direction = FillDirection.Vertical,
                        Position = new Vector2(50, 12),
                        Width = .8f,
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Children = new Drawable[]
                        {
                            TitleLabel = new OsuSpriteText
                            {
                                Current = { BindTarget = titleBinding },
                                Font = OsuFont.TorusAlternate.With(size: 40, weight: FontWeight.SemiBold),
                                RelativeSizeAxes = Axes.X,
                                Truncate = true
                            },
                            ArtistLabel = new OsuSpriteText
                            {
                                Current = { BindTarget = artistBinding },
                                //Not sure if this should be semi bold or medium
                                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                                RelativeSizeAxes = Axes.X,
                                Truncate = true
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                starRatingDisplay.DisplayedStars.BindValueChanged(s =>
                {
                    wedge.starCounter.Colour = s.NewValue >= 6.5 ? colours.Orange1 : Colour4.Black.Opacity(0.75f);
                    wedge.starCounter.Current = (float)s.NewValue;

                    wedge.difficultyColourBar.FadeColour(colours.ForStarDifficulty(s.NewValue), 750, Easing.OutQuint);
                }, true);

                starDifficulty.BindValueChanged(s =>
                {
                    starRatingDisplay.Current.Value = s.NewValue ?? default;

                    // Don't roll the counter on initial display (but still allow it to roll on applying mods etc.)
                    if (!starRatingDisplay.IsPresent)
                        starRatingDisplay.FinishTransforms(true);

                    starRatingDisplay.FadeIn(transition_duration);
                });

                mods.BindValueChanged(m =>
                {
                    settingChangeTracker?.Dispose();

                    settingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                }, true);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                settingChangeTracker?.Dispose();
            }
        }
    }
}
