// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Select
{
    public class BeatmapInfoWedge : VisibilityContainer
    {
        private const float shear_width = 36.75f;

        private static readonly Vector2 wedged_container_shear = new Vector2(shear_width / SongSelect.WEDGED_CONTAINER_SIZE.Y, 0);

        private readonly IBindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        protected BufferedWedgeInfo Info;

        public BeatmapInfoWedge()
        {
            Shear = wedged_container_shear;
            Masking = true;
            BorderColour = new Color4(221, 255, 255, 255);
            BorderThickness = 2.5f;
            Alpha = 0;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(130, 204, 255, 150),
                Radius = 20,
                Roundness = 15,
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] Bindable<RulesetInfo> parentRuleset)
        {
            ruleset.BindTo(parentRuleset);
            ruleset.ValueChanged += _ => updateDisplay();
        }

        protected override void PopIn()
        {
            this.MoveToX(0, 800, Easing.OutQuint);
            this.RotateTo(0, 800, Easing.OutQuint);
            this.FadeIn(250);
        }

        protected override void PopOut()
        {
            this.MoveToX(-100, 800, Easing.In);
            this.RotateTo(10, 800, Easing.In);
            this.FadeOut(500, Easing.In);
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

        public override bool IsPresent => base.IsPresent || Info == null; // Visibility is updated in the LoadComponentAsync callback

        private BufferedWedgeInfo loadingInfo;

        private void updateDisplay()
        {
            void removeOldInfo()
            {
                State.Value = beatmap == null ? Visibility.Hidden : Visibility.Visible;

                Info?.FadeOut(250);
                Info?.Expire();
                Info = null;
            }

            if (beatmap == null)
            {
                removeOldInfo();
                return;
            }

            LoadComponentAsync(loadingInfo = new BufferedWedgeInfo(beatmap, ruleset.Value)
            {
                Shear = -Shear,
                Depth = Info?.Depth + 1 ?? 0
            }, loaded =>
            {
                // ensure we are the most recent loaded wedge.
                if (loaded != loadingInfo) return;

                removeOldInfo();
                Add(Info = loaded);
            });
        }

        public class BufferedWedgeInfo : BufferedContainer
        {
            public OsuSpriteText VersionLabel { get; private set; }
            public OsuSpriteText TitleLabel { get; private set; }
            public OsuSpriteText ArtistLabel { get; private set; }
            public BeatmapSetOnlineStatusPill StatusPill { get; private set; }
            public FillFlowContainer MapperContainer { get; private set; }
            public FillFlowContainer InfoLabelContainer { get; private set; }

            private ILocalisedBindableString titleBinding;
            private ILocalisedBindableString artistBinding;

            private readonly WorkingBeatmap beatmap;
            private readonly RulesetInfo ruleset;

            public BufferedWedgeInfo(WorkingBeatmap beatmap, RulesetInfo userRuleset)
                : base(pixelSnapping: true)
            {
                this.beatmap = beatmap;
                ruleset = userRuleset ?? beatmap.BeatmapInfo.Ruleset;
            }

            [BackgroundDependencyLoader]
            private void load(LocalisationManager localisation)
            {
                var beatmapInfo = beatmap.BeatmapInfo;
                var metadata = beatmapInfo.Metadata ?? beatmap.BeatmapSetInfo?.Metadata ?? new BeatmapMetadata();

                CacheDrawnFrameBuffer = true;
                RedrawOnScale = false;

                RelativeSizeAxes = Axes.Both;

                titleBinding = localisation.GetLocalisedString(new LocalisedString((metadata.TitleUnicode, metadata.Title)));
                artistBinding = localisation.GetLocalisedString(new LocalisedString((metadata.ArtistUnicode, metadata.Artist)));

                Children = new Drawable[]
                {
                    // We will create the white-to-black gradient by modulating transparency and having
                    // a black backdrop. This results in an sRGB-space gradient and not linear space,
                    // transitioning from white to black more perceptually uniformly.
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    // We use a container, such that we can set the colour gradient to go across the
                    // vertices of the masked container instead of the vertices of the (larger) sprite.
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(Color4.White, Color4.White.Opacity(0.3f)),
                        Children = new[]
                        {
                            // Zoomed-in and cropped beatmap background
                            new BeatmapBackgroundSprite(beatmap)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                FillMode = FillMode.Fill,
                            },
                        },
                    },
                    new DifficultyColourBar(beatmapInfo)
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 20,
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
                                Text = beatmapInfo.Version,
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
                        Children = new Drawable[]
                        {
                            StatusPill = new BeatmapSetOnlineStatusPill
                            {
                                TextSize = 11,
                                TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                                Status = beatmapInfo.Status,
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
                                Font = OsuFont.GetFont(size: 28, italics: true),
                                RelativeSizeAxes = Axes.X,
                                Truncate = true,
                            },
                            ArtistLabel = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 17, italics: true),
                                RelativeSizeAxes = Axes.X,
                                Truncate = true,
                            },
                            MapperContainer = new FillFlowContainer
                            {
                                Margin = new MarginPadding { Top = 10 },
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Children = getMapper(metadata)
                            },
                            InfoLabelContainer = new FillFlowContainer
                            {
                                Margin = new MarginPadding { Top = 20 },
                                Spacing = new Vector2(20, 0),
                                AutoSizeAxes = Axes.Both,
                                Children = getInfoLabels()
                            }
                        }
                    }
                };

                titleBinding.BindValueChanged(_ => setMetadata(metadata.Source));
                artistBinding.BindValueChanged(_ => setMetadata(metadata.Source), true);

                // no difficulty means it can't have a status to show
                if (beatmapInfo.Version == null)
                    StatusPill.Hide();
            }

            private void setMetadata(string source)
            {
                ArtistLabel.Text = artistBinding.Value;
                TitleLabel.Text = string.IsNullOrEmpty(source) ? titleBinding.Value : source + " — " + titleBinding.Value;
                ForceRedraw();
            }

            private InfoLabel[] getInfoLabels()
            {
                var b = beatmap.Beatmap;

                List<InfoLabel> labels = new List<InfoLabel>();

                if (b?.HitObjects?.Any() == true)
                {
                    labels.Add(new InfoLabel(new BeatmapStatistic
                    {
                        Name = "Length",
                        Icon = FontAwesome.Regular.Clock,
                        Content = TimeSpan.FromMilliseconds(b.BeatmapInfo.Length).ToString(@"m\:ss"),
                    }));

                    labels.Add(new InfoLabel(new BeatmapStatistic
                    {
                        Name = "BPM",
                        Icon = FontAwesome.Regular.Circle,
                        Content = getBPMRange(b),
                    }));

                    IBeatmap playableBeatmap;

                    try
                    {
                        // Try to get the beatmap with the user's ruleset
                        playableBeatmap = beatmap.GetPlayableBeatmap(ruleset, Array.Empty<Mod>());
                    }
                    catch (BeatmapInvalidForRulesetException)
                    {
                        // Can't be converted to the user's ruleset, so use the beatmap's own ruleset
                        playableBeatmap = beatmap.GetPlayableBeatmap(beatmap.BeatmapInfo.Ruleset, Array.Empty<Mod>());
                    }

                    labels.AddRange(playableBeatmap.GetStatistics().Select(s => new InfoLabel(s)));
                }

                return labels.ToArray();
            }

            private string getBPMRange(IBeatmap beatmap)
            {
                double bpmMax = beatmap.ControlPointInfo.BPMMaximum;
                double bpmMin = beatmap.ControlPointInfo.BPMMinimum;

                if (Precision.AlmostEquals(bpmMin, bpmMax))
                    return $"{bpmMin:0}";

                return $"{bpmMin:0}-{bpmMax:0} (mostly {beatmap.ControlPointInfo.BPMMode:0})";
            }

            private OsuSpriteText[] getMapper(BeatmapMetadata metadata)
            {
                if (string.IsNullOrEmpty(metadata.Author?.Username))
                    return Array.Empty<OsuSpriteText>();

                return new[]
                {
                    new OsuSpriteText
                    {
                        Text = "mapped by ",
                        Font = OsuFont.GetFont(size: 15),
                    },
                    new OsuSpriteText
                    {
                        Text = metadata.Author.Username,
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 15),
                    }
                };
            }

            public class InfoLabel : Container, IHasTooltip
            {
                public string TooltipText { get; private set; }

                public InfoLabel(BeatmapStatistic statistic)
                {
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
                                    Colour = OsuColour.FromHex(@"441288"),
                                    Icon = FontAwesome.Solid.Square,
                                    Rotation = 45,
                                },
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Scale = new Vector2(0.8f),
                                    Colour = OsuColour.FromHex(@"f7dd55"),
                                    Icon = statistic.Icon,
                                },
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

            private class DifficultyColourBar : Container
            {
                private readonly BeatmapInfo beatmap;

                public DifficultyColourBar(BeatmapInfo beatmap)
                {
                    this.beatmap = beatmap;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    const float full_opacity_ratio = 0.7f;

                    var difficultyColour = colours.ForDifficultyRating(beatmap.DifficultyRating);

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = difficultyColour,
                            Width = full_opacity_ratio,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.Both,
                            Colour = difficultyColour,
                            Alpha = 0.5f,
                            X = full_opacity_ratio,
                            Width = 1 - full_opacity_ratio,
                        }
                    };
                }
            }
        }
    }
}
