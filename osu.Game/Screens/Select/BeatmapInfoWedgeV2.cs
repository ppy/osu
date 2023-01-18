// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
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
    public partial class BeatmapInfoWedgeV2 : VisibilityContainer
    {
        private const float shear_width = 21;
        private const float wedge_height = 120;
        private const float transition_duration = 250;
        private const float corner_radius = 10;
        private const float colour_bar_width = 30;

        /// Todo: move this const out to song select when more new design elements are implemented for the beatmap details area, since it applies to text alignment of various elements
        private const float text_margin = 62;

        private static readonly Vector2 wedged_container_shear = new Vector2(shear_width / wedge_height, 0);

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected Container? DisplayedContent { get; private set; }

        protected WedgeInfoText? Info { get; private set; }

        private readonly Container difficultyColourBar;
        private readonly StarCounter starCounter;

        public BeatmapInfoWedgeV2()
        {
            Height = wedge_height;
            Shear = wedged_container_shear;
            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Colour4.Black.Opacity(.25f),
                Type = EdgeEffectType.Shadow,
                Radius = corner_radius,
                Roundness = corner_radius
            };
            CornerRadius = corner_radius;

            Children = new Drawable[]
            {
                // These elements can't be grouped with the rest of the content, due to being present either outside or under the backgrounds area
                difficultyColourBar = new Container
                {
                    Colour = Colour4.Transparent,
                    Depth = float.MaxValue,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Y,

                    // By limiting the width we avoid this box showing up as an outline around the drawables that are on top of it.
                    Width = colour_bar_width + corner_radius,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                starCounter = new StarCounter
                {
                    Colour = Colour4.Transparent,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.35f),
                    Shear = -wedged_container_shear,
                    X = -colour_bar_width / 2,
                    Direction = FillDirection.Vertical
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ruleset.BindValueChanged(_ => updateDisplay());

            float starAngle = (float)(Math.Atan(shear_width / wedge_height) * (180 / Math.PI));

            // Applying the rotation directly to the StarCounter distorts the stars, hence it is applied to the child container
            starCounter.Children.First().Rotation = starAngle;

            // Makes sure the stars center themselves properly in the colour bar
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

        private WorkingBeatmap? beatmap;

        public WorkingBeatmap? Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value) return;

                beatmap = value;

                updateDisplay();
            }
        }

        public override bool IsPresent => base.IsPresent || DisplayedContent == null; // Visibility is updated in the LoadComponentAsync callback

        private Container? loadingInfo;

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
                    // We offset this by the portion of the colour bar underneath we wish to show
                    X = -colour_bar_width,
                    CornerRadius = corner_radius,
                    RelativeSizeAxes = Axes.Both,
                    Depth = DisplayedContent?.Depth + 1 ?? 0,
                    Children = new Drawable[]
                    {
                        // TODO: New wedge design uses a coloured horizontal gradient for its background, however this lacks implementation information in the figma draft.
                        // pending https://www.figma.com/file/DXKwqZhD5yyb1igc3mKo1P?node-id=2980:3361#340801912 being answered.
                        new BeatmapInfoWedgeBackground(beatmap) { Shear = -Shear },
                        Info = new WedgeInfoText(beatmap) { Shear = -Shear }
                    }
                }, loaded =>
                {
                    // Ensure we are the most recent loaded wedge.
                    if (loaded != loadingInfo) return;

                    removeOldInfo();
                    Add(DisplayedContent = loaded);

                    Info.StarRatingDisplay.DisplayedStars.BindValueChanged(s =>
                    {
                        starCounter.Current = (float)s.NewValue;
                        starCounter.Colour = s.NewValue >= 6.5 ? colours.Orange1 : Colour4.Black.Opacity(0.75f);

                        difficultyColourBar.FadeColour(colours.ForStarDifficulty(s.NewValue));
                    }, true);
                });
            }
        }

        public partial class WedgeInfoText : Container
        {
            public OsuSpriteText TitleLabel { get; private set; } = null!;
            public OsuSpriteText ArtistLabel { get; private set; } = null!;

            public StarRatingDisplay StarRatingDisplay = null!;

            private ILocalisedBindableString titleBinding = null!;
            private ILocalisedBindableString artistBinding = null!;

            private readonly WorkingBeatmap working;

            [Resolved]
            private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

            [Resolved]
            private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

            private ModSettingChangeTracker? settingChangeTracker;

            private IBindable<StarDifficulty?>? starDifficulty;
            private CancellationTokenSource? cancellationSource;

            public WedgeInfoText(WorkingBeatmap working)
            {
                this.working = working;
            }

            [BackgroundDependencyLoader]
            private void load(LocalisationManager localisation)
            {
                var beatmapInfo = working.BeatmapInfo;
                var metadata = working.Metadata;

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
                        Padding = new MarginPadding { Top = 3, Right = 8 },
                        AutoSizeAxes = Axes.Both,
                        Shear = wedged_container_shear,
                        Spacing = new Vector2(0f, 5f),
                        Children = new Drawable[]
                        {
                            StarRatingDisplay = new StarRatingDisplay(default, animated: true)
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
                        Padding = new MarginPadding { Horizontal = text_margin + shear_width, Top = 12 },
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Children = new Drawable[]
                        {
                            TitleLabel = new OsuSpriteText
                            {
                                Shadow = true,
                                Current = { BindTarget = titleBinding },
                                Font = OsuFont.TorusAlternate.With(size: 40, weight: FontWeight.SemiBold),
                                RelativeSizeAxes = Axes.X,
                                Truncate = true
                            },
                            ArtistLabel = new OsuSpriteText
                            {
                                // TODO : figma design has a diffused shadow, instead of the solid one present here, not possible currently as far as i'm aware.
                                Shadow = true,
                                Current = { BindTarget = artistBinding },
                                // Not sure if this should be semi bold or medium
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

                starDifficulty = difficultyCache.GetBindableDifficulty(working.BeatmapInfo, (cancellationSource = new CancellationTokenSource()).Token);
                starDifficulty.BindValueChanged(s =>
                {
                    StarRatingDisplay.Current.Value = s.NewValue ?? default;

                    // Don't roll the counter on initial display (but still allow it to roll on applying mods etc.)
                    if (!StarRatingDisplay.IsPresent)
                        StarRatingDisplay.FinishTransforms(true);

                    StarRatingDisplay.FadeIn(transition_duration);
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

                cancellationSource?.Cancel();
                settingChangeTracker?.Dispose();
            }
        }
    }
}
