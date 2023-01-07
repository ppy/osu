// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Threading;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Effects;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Select
{
    public partial class BeatmapInfoWedgeV2 : VisibilityContainer
    {
        public const float BORDER_THICKNESS = 2.5f;
        private const float shear_width = 36.75f;

        private const float transition_duration = 250;

        private static readonly Vector2 wedged_container_shear = new Vector2(shear_width / SongSelect.WEDGE_HEIGHT, 0);

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        protected Container DisplayedContent { get; private set; }

        protected WedgeInfoText Info { get; private set; }

        public BeatmapInfoWedgeV2()
        {
            Shear = wedged_container_shear;
            Masking = true;
            BorderColour = new Color4(221, 255, 255, 255);
            BorderThickness = BORDER_THICKNESS;
            Alpha = 0;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(130, 204, 255, 150),
                Radius = 20,
                Roundness = 15,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ruleset.BindValueChanged(_ => updateDisplay());
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

                updateDisplay();
            }
        }

        public override bool IsPresent => base.IsPresent || DisplayedContent == null; // Visibility is updated in the LoadComponentAsync callback

        private Container loadingInfo;

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
                    RelativeSizeAxes = Axes.Both,
                    Shear = -Shear,
                    Depth = DisplayedContent?.Depth + 1 ?? 0,
                    Children = new Drawable[]
                    {
                        new BeatmapInfoWedgeBackground(beatmap),
                        Info = new WedgeInfoText(beatmap),
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
            public OsuSpriteText VersionLabel { get; private set; }
            public OsuSpriteText TitleLabel { get; private set; }
            public OsuSpriteText ArtistLabel { get; private set; }
            public FillFlowContainer MapperContainer { get; private set; }

            private Container difficultyColourBar;
            private StarRatingDisplay starRatingDisplay;

            private ILocalisedBindableString titleBinding;
            private ILocalisedBindableString artistBinding;

            private readonly WorkingBeatmap working;

            [Resolved]
            private IBindable<IReadOnlyList<Mod>> mods { get; set; }

            [Resolved]
            private BeatmapDifficultyCache difficultyCache { get; set; }

            [Resolved]
            private OsuColour colours { get; set; }

            private ModSettingChangeTracker settingChangeTracker;

            public WedgeInfoText(WorkingBeatmap working)
            {
                this.working = working;
            }

            private CancellationTokenSource cancellationSource;
            private IBindable<StarDifficulty?> starDifficulty;

            [BackgroundDependencyLoader]
            private void load(LocalisationManager localisation)
            {
                var beatmapInfo = working.BeatmapInfo;
                var metadata = beatmapInfo.Metadata;

                RelativeSizeAxes = Axes.Both;

                titleBinding = localisation.GetLocalisedBindableString(new RomanisableString(metadata.TitleUnicode, metadata.Title));
                artistBinding = localisation.GetLocalisedBindableString(new RomanisableString(metadata.ArtistUnicode, metadata.Artist));

                const float top_height = 0.7f;

                Children = new Drawable[]
                {
                    difficultyColourBar = new Container
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 20f,
                        Children = new[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = top_height,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.Both,
                                Alpha = 0.5f,
                                X = top_height,
                                Width = 1 - top_height,
                            }
                        }
                    },
                    new FillFlowContainer
                    {
                        Name = "Topleft-aligned metadata",
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding { Top = 10, Left = 25, Right = shear_width * 2.5f },
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Children = new Drawable[]
                        {
                            VersionLabel = new OsuSpriteText
                            {
                                Text = beatmapInfo.DifficultyName,
                                Font = OsuFont.GetFont(size: 24, italics: true),
                                RelativeSizeAxes = Axes.X,
                                Truncate = true,
                            },
                        }
                    },
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
                        Name = "Centre-aligned metadata",
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.TopLeft,
                        Y = -7,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding { Left = 25, Right = shear_width },
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Children = new Drawable[]
                        {
                            TitleLabel = new OsuSpriteText
                            {
                                Current = { BindTarget = titleBinding },
                                Font = OsuFont.GetFont(size: 28, italics: true),
                                RelativeSizeAxes = Axes.X,
                                Truncate = true,
                            },
                            ArtistLabel = new OsuSpriteText
                            {
                                Current = { BindTarget = artistBinding },
                                Font = OsuFont.GetFont(size: 17, italics: true),
                                RelativeSizeAxes = Axes.X,
                                Truncate = true,
                            },
                            MapperContainer = new FillFlowContainer
                            {
                                Margin = new MarginPadding { Top = 10 },
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Child = getMapper(metadata),
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
                    difficultyColourBar.Colour = colours.ForStarDifficulty(s.NewValue);
                }, true);

                starDifficulty = difficultyCache.GetBindableDifficulty(working.BeatmapInfo, (cancellationSource = new CancellationTokenSource()).Token);
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

            private Drawable getMapper(BeatmapMetadata metadata)
            {
                if (string.IsNullOrEmpty(metadata.Author.Username))
                    return Empty();

                return new LinkFlowContainer(s =>
                {
                    s.Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 15);
                }).With(d =>
                {
                    d.AutoSizeAxes = Axes.Both;
                    d.AddText("mapped by ");
                    d.AddUserLink(metadata.Author);
                });
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                settingChangeTracker?.Dispose();
                cancellationSource?.Cancel();
            }
        }
    }
}
