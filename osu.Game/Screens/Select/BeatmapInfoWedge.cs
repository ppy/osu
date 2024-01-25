// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Graphics.Containers;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.Select
{
    public partial class BeatmapInfoWedge : VisibilityContainer
    {
        public const float BORDER_THICKNESS = 2.5f;
        private const float shear_width = 36.75f;

        private const float transition_duration = 250;

        private static readonly Vector2 wedged_container_shear = new Vector2(shear_width / SongSelect.WEDGE_HEIGHT, 0);

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        protected Container DisplayedContent { get; private set; }

        protected WedgeInfoText Info { get; private set; }

        public BeatmapInfoWedge()
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
                Radius = 15,
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
            this.FadeIn(transition_duration);
        }

        protected override void PopOut()
        {
            this.MoveToX(-100, animation_duration, Easing.In);
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
                        Info = new WedgeInfoText(beatmap, ruleset.Value),
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
            private FillFlowContainer infoLabelContainer;
            private Container bpmLabelContainer;
            private Container lengthLabelContainer;

            private readonly WorkingBeatmap working;
            private readonly RulesetInfo ruleset;

            [Resolved]
            private IBindable<IReadOnlyList<Mod>> mods { get; set; }

            [Resolved]
            private BeatmapDifficultyCache difficultyCache { get; set; }

            [Resolved]
            private OsuColour colours { get; set; }

            private ModSettingChangeTracker settingChangeTracker;

            public WedgeInfoText(WorkingBeatmap working, RulesetInfo userRuleset)
            {
                this.working = working;
                ruleset = userRuleset ?? working.BeatmapInfo.Ruleset;
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
                            VersionLabel = new TruncatingSpriteText
                            {
                                Text = beatmapInfo.DifficultyName,
                                Font = OsuFont.GetFont(size: 24, italics: true),
                                RelativeSizeAxes = Axes.X,
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
                            TitleLabel = new TruncatingSpriteText
                            {
                                Current = { BindTarget = titleBinding },
                                Font = OsuFont.GetFont(size: 28, italics: true),
                                RelativeSizeAxes = Axes.X,
                            },
                            ArtistLabel = new TruncatingSpriteText
                            {
                                Current = { BindTarget = artistBinding },
                                Font = OsuFont.GetFont(size: 17, italics: true),
                                RelativeSizeAxes = Axes.X,
                            },
                            MapperContainer = new FillFlowContainer
                            {
                                Margin = new MarginPadding { Top = 10 },
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Child = getMapper(metadata),
                            },
                            infoLabelContainer = new FillFlowContainer
                            {
                                Margin = new MarginPadding { Top = 8 },
                                Spacing = new Vector2(20, 0),
                                AutoSizeAxes = Axes.Both,
                            }
                        }
                    }
                };

                addInfoLabels();
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

                    refreshBPMAndLengthLabel();

                    settingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                    settingChangeTracker.SettingChanged += _ => refreshBPMAndLengthLabel();
                }, true);
            }

            private void addInfoLabels()
            {
                if (working.Beatmap?.HitObjects.Any() != true)
                    return;

                try
                {
                    IBeatmap playableBeatmap;

                    try
                    {
                        // Try to get the beatmap with the user's ruleset
                        playableBeatmap = working.GetPlayableBeatmap(ruleset, Array.Empty<Mod>());
                    }
                    catch (BeatmapInvalidForRulesetException)
                    {
                        // Can't be converted to the user's ruleset, so use the beatmap's own ruleset
                        playableBeatmap = working.GetPlayableBeatmap(working.BeatmapInfo.Ruleset, Array.Empty<Mod>());
                    }

                    infoLabelContainer.Children = new Drawable[]
                    {
                        lengthLabelContainer = new Container
                        {
                            AutoSizeAxes = Axes.Both,
                        },
                        bpmLabelContainer = new Container
                        {
                            AutoSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(20, 0),
                            Children = playableBeatmap.GetStatistics().Select(s => new InfoLabel(s)).ToArray()
                        }
                    };
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Could not load beatmap successfully!");
                }
            }

            private void refreshBPMAndLengthLabel()
            {
                var beatmap = working.Beatmap;

                if (beatmap == null || bpmLabelContainer == null)
                    return;

                // this doesn't consider mods which apply variable rates, yet.
                double rate = 1;
                foreach (var mod in mods.Value.OfType<IApplicableToRate>())
                    rate = mod.ApplyToRate(0, rate);

                int bpmMax = (int)Math.Round(Math.Round(beatmap.ControlPointInfo.BPMMaximum) * rate);
                int bpmMin = (int)Math.Round(Math.Round(beatmap.ControlPointInfo.BPMMinimum) * rate);
                int mostCommonBPM = (int)Math.Round(Math.Round(60000 / beatmap.GetMostCommonBeatLength()) * rate);

                string labelText = bpmMin == bpmMax
                    ? $"{bpmMin}"
                    : $"{bpmMin}-{bpmMax} (mostly {mostCommonBPM})";

                bpmLabelContainer.Child = new InfoLabel(new BeatmapStatistic
                {
                    Name = BeatmapsetsStrings.ShowStatsBpm,
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Bpm),
                    Content = labelText
                });

                double drainLength = Math.Round(beatmap.CalculateDrainLength() / rate);
                double hitLength = Math.Round(beatmap.BeatmapInfo.Length / rate);

                lengthLabelContainer.Child = new InfoLabel(new BeatmapStatistic
                {
                    Name = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration()),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Length),
                    Content = hitLength.ToFormattedDuration().ToString(),
                });
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

            public partial class InfoLabel : Container, IHasTooltip
            {
                public LocalisableString TooltipText { get; }

                internal BeatmapStatistic Statistic { get; }

                public InfoLabel(BeatmapStatistic statistic)
                {
                    Statistic = statistic;
                    TooltipText = statistic.Name;
                    AutoSizeAxes = Axes.Both;

                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(20),
                            Children = new[]
                            {
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex(@"441288"),
                                    Icon = FontAwesome.Solid.Square,
                                    Rotation = 45,
                                },
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex(@"f7dd55"),
                                    Icon = FontAwesome.Regular.Circle,
                                    Size = new Vector2(0.8f)
                                },
                                statistic.CreateIcon().With(i =>
                                {
                                    i.Anchor = Anchor.Centre;
                                    i.Origin = Anchor.Centre;
                                    i.RelativeSizeAxes = Axes.Both;
                                    i.Colour = Color4Extensions.FromHex(@"f7dd55");
                                    i.Size = new Vector2(0.64f);
                                }),
                            }
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Colour = new Color4(255, 221, 85, 255),
                            Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 17),
                            Margin = new MarginPadding { Left = 30 },
                            Text = statistic.Content,
                        }
                    };
                }
            }
        }
    }
}
