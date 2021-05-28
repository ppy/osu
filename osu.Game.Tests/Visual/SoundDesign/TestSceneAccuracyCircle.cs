// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.SoundDesign
{
    public class TestSceneAccuracyCircle : OsuTestScene
    {
        [Resolved]
        private AudioManager audioManager { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private DrawableSample previewSampleChannel;
        private AccuracyCircleAudioSettings settings = new AccuracyCircleAudioSettings();
        private OsuTextBox saveFilename;

        private Storage presetStorage;
        private FileSelector presetFileSelector;

        private Bindable<SampleLoadTarget> sampleLoadTarget = new Bindable<SampleLoadTarget>();
        private Bindable<string> selectedSampleName = new Bindable<string>();

        private Container accuracyCircle;

        private enum SampleLoadTarget
        {
            ScoreTick,
            BadgeDink,
            BadgeDinkMax,
            Swoosh,
            ImpactD,
            ImpactC,
            ImpactB,
            ImpactA,
            ImpactS,
            ImpactSS,
            ApplauseD,
            ApplauseC,
            ApplauseB,
            ApplauseA,
            ApplauseS,
            ApplauseSS,
        };

        private enum SectionTabs
        {
            [System.ComponentModel.Description("Score Ticks")]
            ScoreTicks,

            [System.ComponentModel.Description("Badge Dinks")]
            BadgeDinks,

            [System.ComponentModel.Description("Swoosh")]
            Swoosh,

            [System.ComponentModel.Description("Impact")]
            Impact,

            [System.ComponentModel.Description("Applause")]
            Applause,

            [System.ComponentModel.Description("Preset")]
            Preset
        }

        private OsuTabControl<SectionTabs> tabSelector;

        private Dictionary<SectionTabs, FillFlowContainer> tabContainers = new Dictionary<SectionTabs, FillFlowContainer>();
        private FillFlowContainer sampleSelectContainer;

        private FileSelector sampleFileSelector;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            presetStorage = host.Storage.GetStorageForDirectory("presets");

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.5f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex("222")
                                },
                                new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ScrollbarVisible = false,
                                    Child = new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 10),
                                        Padding = new MarginPadding(10),
                                        Children = new Drawable[]
                                        {
                                            tabSelector = new OsuTabControl<SectionTabs>
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Width = 1f,
                                                Height = 24,
                                            },

                                            #region score ticks

                                            // ==================== SCORE TICKS ====================
                                            tabContainers[SectionTabs.ScoreTicks] = new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(0, 10),
                                                Width = 1f,
                                                Children = new Drawable[]
                                                {
                                                    new SettingsCheckbox
                                                    {
                                                        LabelText = "Play Ticks",
                                                        Current = { BindTarget = settings.PlayTicks }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "Tick Volume (Start)",
                                                        Current = { BindTarget = settings.TickVolumeStart }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "Tick Volume (End)",
                                                        Current = { BindTarget = settings.TickVolumeEnd }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "ScoreTick Start Debounce Rate",
                                                        Current = { BindTarget = settings.TickDebounceStart }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "ScoreTick End Debounce Rate",
                                                        Current = { BindTarget = settings.TickDebounceEnd }
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                        Text = "ScoreTick Rate Easing:"
                                                    },
                                                    new SettingsEnumDropdown<Easing>
                                                    {
                                                        Current = { BindTarget = settings.TickRateEasing }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "ScoreTick Pitch Factor",
                                                        Current = { BindTarget = settings.TickPitchFactor }
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                        Text = "Pitch Easing:"
                                                    },
                                                    new SettingsEnumDropdown<Easing>
                                                    {
                                                        Current = { BindTarget = settings.TickPitchEasing }
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                        Text = "Volume Easing:"
                                                    },
                                                    new SettingsEnumDropdown<Easing>
                                                    {
                                                        Current = { BindTarget = settings.TickVolumeEasing }
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                        Text = "Tick Sample:"
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2 },
                                                        Current = { BindTarget = settings.TickSampleName }
                                                    }
                                                }
                                            },

                                            #endregion

                                            #region badge dinks

                                            // ==================== BADGE DINKS ====================
                                            tabContainers[SectionTabs.BadgeDinks] = new FillFlowContainer
                                            {
                                                Alpha = 0,
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(0, 10),
                                                Width = 1f,
                                                Children = new Drawable[]
                                                {
                                                    new SettingsCheckbox
                                                    {
                                                        LabelText = "Play BadgeSounds",
                                                        Current = { BindTarget = settings.PlayBadgeSounds }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "Badge Dink Volume",
                                                        Current = { BindTarget = settings.BadgeDinkVolume }
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Badge Dink Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.BadgeSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Badge Max Dink Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.BadgeMaxSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    }
                                                }
                                            },

                                            #endregion

                                            #region swoosh

                                            // ==================== SWOOSHES ====================
                                            tabContainers[SectionTabs.Swoosh] = new FillFlowContainer
                                            {
                                                Alpha = 0,
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(0, 10),
                                                Width = 1f,
                                                Children = new Drawable[]
                                                {
                                                    new SettingsCheckbox
                                                    {
                                                        LabelText = "Play Swoosh",
                                                        Current = { BindTarget = settings.PlaySwooshSound }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "Swoosh Volume",
                                                        Current = { BindTarget = settings.SwooshVolume }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "Swoosh Pre-Delay (ms)",
                                                        Current = { BindTarget = settings.SwooshPreDelay }
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Swoosh Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.SwooshSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    }
                                                }
                                            },

                                            #endregion

                                            #region impact

                                            // ==================== IMPACT ====================
                                            tabContainers[SectionTabs.Impact] = new FillFlowContainer
                                            {
                                                Alpha = 0,
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(0, 10),
                                                Width = 1f,
                                                Children = new Drawable[]
                                                {
                                                    new SettingsCheckbox
                                                    {
                                                        LabelText = "Play Impact",
                                                        Current = { BindTarget = settings.PlayImpact }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "Impact Volume",
                                                        Current = { BindTarget = settings.ImpactVolume }
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade D Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ImpactGradeDSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade C Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ImpactGradeCSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade B Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ImpactGradeBSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade A Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ImpactGradeASampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade S Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ImpactGradeSSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade SS Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ImpactGradeSSSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    }
                                                }
                                            },

                                            #endregion

                                            #region applause

                                            // ==================== APPLAUSE ====================
                                            tabContainers[SectionTabs.Applause] = new FillFlowContainer
                                            {
                                                Alpha = 0,
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(0, 10),
                                                Width = 1f,
                                                Children = new Drawable[]
                                                {
                                                    new SettingsCheckbox
                                                    {
                                                        LabelText = "Play Applause",
                                                        Current = { BindTarget = settings.PlayApplause }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "Applause Volume",
                                                        Current = { BindTarget = settings.ApplauseVolume }
                                                    },
                                                    new SettingsSlider<double>
                                                    {
                                                        LabelText = "Applause Delay (ms)",
                                                        Current = { BindTarget = settings.ApplauseDelay }
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade D Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ApplauseGradeDSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade C Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ApplauseGradeCSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade B Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ApplauseGradeBSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade A Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ApplauseGradeASampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade S Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ApplauseGradeSSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(),
                                                        Text = "Grade SS Sample:",
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Current = { BindTarget = settings.ApplauseGradeSSSampleName },
                                                        Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS * 2f },
                                                    }
                                                }
                                            },

                                            #endregion

                                            #region preset

                                            // ==================== PRESET ====================
                                            tabContainers[SectionTabs.Preset] = new FillFlowContainer
                                            {
                                                Alpha = 0,
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(0, 10),
                                                Width = 1f,
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.Default.With(size: 24),
                                                        Text = "Load",
                                                        Colour = colours.Yellow
                                                    },
                                                    presetFileSelector = new FileSelector(presetStorage.GetFullPath(string.Empty))
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        Height = 300,
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.Default.With(size: 24),
                                                        Text = "Save",
                                                        Colour = colours.Yellow
                                                    },
                                                    saveFilename = new OsuTextBox
                                                    {
                                                        PlaceholderText = "New preset filename",
                                                        RelativeSizeAxes = Axes.X,
                                                    },
                                                    new TriangleButton
                                                    {
                                                        Text = "Save",
                                                        Action = savePreset,
                                                        RelativeSizeAxes = Axes.X,
                                                    },
                                                }
                                            },

                                            #endregion

                                            #region fileselector

                                            // ==================== SAMPLE SELECTOR ====================
                                            sampleSelectContainer = new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                Direction = FillDirection.Vertical,
                                                Padding = new MarginPadding(10)
                                                {
                                                    Top = 20,
                                                },
                                                Width = 1f,
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Font = OsuFont.Default.With(size: 20),
                                                        Text = "Load Sample",
                                                        Colour = colours.Yellow
                                                    },
                                                    sampleFileSelector = new FileSelector("/Users/jamie/Sandbox/derp/Samples/Results")
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        Height = 300,
                                                    },
                                                    new TriangleButton
                                                    {
                                                        Text = "Refresh",
                                                        Action = refreshSampleBrowser,
                                                        RelativeSizeAxes = Axes.X,
                                                    },
                                                    new SettingsEnumDropdown<SampleLoadTarget>
                                                    {
                                                        Current = { BindTarget = sampleLoadTarget }
                                                    },
                                                    new TriangleButton
                                                    {
                                                        Text = "Load Sample",
                                                        Action = loadSample,
                                                        RelativeSizeAxes = Axes.X,
                                                    }
                                                }
                                            }

                                            #endregion
                                        }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.5f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#333"))
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 0.5f,
                                    Children = new[]
                                    {
                                        new TriangleButton
                                        {
                                            Text = "Low D Rank",
                                            Action = CreateLowRankDCircle,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.25f,
                                        },
                                        new TriangleButton
                                        {
                                            Text = "D Rank",
                                            Action = CreateDRankCircle,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.25f,
                                        },
                                        new TriangleButton
                                        {
                                            Text = "C Rank",
                                            Action = CreateCRankCircle,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.25f,
                                        },
                                        new TriangleButton
                                        {
                                            Text = "B Rank",
                                            Action = CreateBRankCircle,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.25f,
                                        },
                                        new TriangleButton
                                        {
                                            Text = "A Rank",
                                            Action = CreateARankCircle,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.25f,
                                        },
                                        new TriangleButton
                                        {
                                            Text = "S Rank",
                                            Action = CreateSRankCircle,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.25f,
                                        },
                                        new TriangleButton
                                        {
                                            Text = "Almost SS Rank",
                                            Action = CreateAlmostSSRankCircle,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.25f,
                                        },
                                        new TriangleButton
                                        {
                                            Text = "SS Rank",
                                            Action = CreateSSRankCircle,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.25f,
                                        },
                                    }
                                },
                                accuracyCircle = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    // Child = CreateRankDCircle()
                                }
                            }
                        }
                    }
                },
            };

            presetFileSelector.CurrentFile.ValueChanged += value =>
            {
                string path = value.NewValue.FullName;

                loadPreset(path);
                saveFilename.Text = Path.GetFileNameWithoutExtension(path);
            };

            sampleFileSelector.CurrentFile.ValueChanged += value =>
            {
                var sample = Path.GetFileNameWithoutExtension(value.NewValue.Name);

                previewSampleChannel?.Dispose();
                previewSampleChannel = new DrawableSample(audioManager.Samples.Get($"Results/{sample}"));
                previewSampleChannel?.Play();

                selectedSampleName.Value = sample;
            };

            tabSelector.Current.ValueChanged += tab =>
            {
                tabContainers[tab.OldValue].Hide();
                tabContainers[tab.NewValue].Show();

                switch (tab.NewValue)
                {
                    case SectionTabs.Preset:
                        sampleSelectContainer.Hide();
                        break;

                    case SectionTabs.Impact:
                        sampleLoadTarget.Value = SampleLoadTarget.ImpactD;
                        sampleSelectContainer.Show();
                        break;

                    case SectionTabs.Swoosh:
                        sampleLoadTarget.Value = SampleLoadTarget.Swoosh;
                        sampleSelectContainer.Show();
                        break;

                    case SectionTabs.BadgeDinks:
                        sampleLoadTarget.Value = SampleLoadTarget.BadgeDink;
                        sampleSelectContainer.Show();
                        break;

                    case SectionTabs.ScoreTicks:
                        sampleLoadTarget.Value = SampleLoadTarget.ScoreTick;
                        sampleSelectContainer.Show();
                        break;

                    case SectionTabs.Applause:
                        sampleLoadTarget.Value = SampleLoadTarget.ApplauseD;
                        sampleSelectContainer.Show();
                        break;
                }
            };
        }

        #region rank scenarios

        [Test]
        public void TestDoNothing() => AddStep("show", () =>
        {
             /* do nothing */
        });

        [Test]
        public void TestLowDRank() => AddStep("show", CreateLowRankDCircle);

        [Test]
        public void TestDRank() => AddStep("show", CreateDRankCircle);

        [Test]
        public void TestCRank() => AddStep("show", CreateCRankCircle);

        [Test]
        public void TestBRank() => AddStep("show", CreateBRankCircle);

        [Test]
        public void TestARank() => AddStep("show", CreateARankCircle);

        [Test]
        public void TestSRank() => AddStep("show", CreateSRankCircle);

        [Test]
        public void TestAlmostSSRank() => AddStep("show", CreateAlmostSSRankCircle);

        [Test]
        public void TestSSRank() => AddStep("show", CreateSSRankCircle);

        #endregion

        public void CreateLowRankDCircle() =>
            accuracyCircle.Child = CreateAccuracyCircle(createScore(0.2, ScoreRank.D));

        public void CreateDRankCircle() =>
            accuracyCircle.Child = CreateAccuracyCircle(createScore(0.5, ScoreRank.D));

        public void CreateCRankCircle() =>
            accuracyCircle.Child = CreateAccuracyCircle(createScore(0.75, ScoreRank.C));

        public void CreateBRankCircle() =>
            accuracyCircle.Child = CreateAccuracyCircle(createScore(0.85, ScoreRank.B));

        public void CreateARankCircle() =>
            accuracyCircle.Child = CreateAccuracyCircle(createScore(0.925, ScoreRank.A));

        public void CreateSRankCircle() =>
            accuracyCircle.Child = CreateAccuracyCircle(createScore(0.975, ScoreRank.S));

        public void CreateAlmostSSRankCircle() =>
            accuracyCircle.Child = CreateAccuracyCircle(createScore(0.9999, ScoreRank.S));

        public void CreateSSRankCircle() =>
            accuracyCircle.Child = CreateAccuracyCircle(createScore(1, ScoreRank.X));

        public AccuracyCircle CreateAccuracyCircle(ScoreInfo score)
        {
            var newAccuracyCircle = new AccuracyCircle(score, true)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(230),
            };

            // newAccuracyCircle.BindAudioSettings(settings);

            return newAccuracyCircle;
        }

        private void savePreset()
        {
            string path = presetStorage.GetFullPath($"{saveFilename.Text}.json", true);
            File.WriteAllText(path, JsonConvert.SerializeObject(settings));
            presetFileSelector.CurrentFile.Value = new FileInfo(path);
        }

        private void loadPreset(string filename)
        {
            var saved = JsonConvert.DeserializeObject<AccuracyCircleAudioSettings>(File.ReadAllText(presetStorage.GetFullPath(filename)));

            foreach (var (_, prop) in saved.GetSettingsSourceProperties())
            {
                var targetBindable = (IBindable)prop.GetValue(settings);
                var sourceBindable = (IBindable)prop.GetValue(saved);

                ((IParseable)targetBindable)?.Parse(sourceBindable);
            }
        }

        private void refreshSampleBrowser() =>
            sampleFileSelector.CurrentPath.Value = new DirectoryInfo(sampleFileSelector.CurrentPath.Value.FullName);

        private void loadSample()
        {
            switch (sampleLoadTarget.Value)
            {
                case SampleLoadTarget.Swoosh:
                    settings.SwooshSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ScoreTick:
                    settings.TickSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.BadgeDink:
                    settings.BadgeSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.BadgeDinkMax:
                    settings.BadgeMaxSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ImpactD:
                    settings.ImpactGradeDSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ImpactC:
                    settings.ImpactGradeCSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ImpactB:
                    settings.ImpactGradeBSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ImpactA:
                    settings.ImpactGradeASampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ImpactS:
                    settings.ImpactGradeSSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ImpactSS:
                    settings.ImpactGradeSSSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ApplauseD:
                    settings.ApplauseGradeDSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ApplauseC:
                    settings.ApplauseGradeCSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ApplauseB:
                    settings.ApplauseGradeBSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ApplauseA:
                    settings.ApplauseGradeASampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ApplauseS:
                    settings.ApplauseGradeSSampleName.Value = selectedSampleName.Value;
                    break;

                case SampleLoadTarget.ApplauseSS:
                    settings.ApplauseGradeSSSampleName.Value = selectedSampleName.Value;
                    break;
            }
        }

        private ScoreInfo createScore(double accuracy = 0.95, ScoreRank rank = ScoreRank.S) => new ScoreInfo
        {
            User = new User
            {
                Id = 2,
                Username = "peppy",
            },
            Beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo,
            Mods = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
            TotalScore = 2845370,
            Accuracy = accuracy,
            MaxCombo = 999,
            Rank = rank,
            Date = DateTimeOffset.Now,
            Statistics =
            {
                { HitResult.Miss, 1 },
                { HitResult.Meh, 50 },
                { HitResult.Good, 100 },
                { HitResult.Great, 300 },
            }
        };
    }

    [Serializable]
    public class AccuracyCircleAudioSettings
    {
        [SettingSource("setting")]
        public Bindable<bool> PlayTicks { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<string> TickSampleName { get; } = new Bindable<string>("score-tick");

        [SettingSource("setting")]
        public Bindable<bool> PlayBadgeSounds { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<string> BadgeSampleName { get; } = new Bindable<string>("badge-dink");

        [SettingSource("setting")]
        public Bindable<string> BadgeMaxSampleName { get; } = new Bindable<string>("badge-dink-max");

        [SettingSource("setting")]
        public Bindable<bool> PlaySwooshSound { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<string> SwooshSampleName { get; } = new Bindable<string>("swoosh-up");

        [SettingSource("setting")]
        public Bindable<bool> PlayImpact { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeDSampleName { get; } = new Bindable<string>("rank-impact-fail-d");

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeCSampleName { get; } = new Bindable<string>("rank-impact-fail");

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeBSampleName { get; } = new Bindable<string>("rank-impact-fail");

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeASampleName { get; } = new Bindable<string>("rank-impact-pass");

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeSSampleName { get; } = new Bindable<string>("rank-impact-pass");

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeSSSampleName { get; } = new Bindable<string>("rank-impact-pass-ss");

        [SettingSource("setting")]
        public Bindable<bool> PlayApplause { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public BindableDouble ApplauseVolume { get; } = new BindableDouble(0.8)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble ApplauseDelay { get; } = new BindableDouble(545)
        {
            MinValue = 0,
            MaxValue = 10000,
            Precision = 1
        };

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeDSampleName { get; } = new Bindable<string>("applause-d");

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeCSampleName { get; } = new Bindable<string>("applause-c");

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeBSampleName { get; } = new Bindable<string>("applause-b");

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeASampleName { get; } = new Bindable<string>("applause-a");

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeSSampleName { get; } = new Bindable<string>("applause-s");

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeSSSampleName { get; } = new Bindable<string>("applause-s");

        [SettingSource("setting")]
        public BindableDouble TickPitchFactor { get; } = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 3,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble TickDebounceStart { get; } = new BindableDouble(18)
        {
            MinValue = 1,
            MaxValue = 100
        };

        [SettingSource("setting")]
        public BindableDouble TickDebounceEnd { get; } = new BindableDouble(300)
        {
            MinValue = 100,
            MaxValue = 1000
        };

        [SettingSource("setting")]
        public BindableDouble SwooshPreDelay { get; } = new BindableDouble(443)
        {
            MinValue = -1000,
            MaxValue = 1000
        };

        [SettingSource("setting")]
        public Bindable<Easing> TickRateEasing { get; } = new Bindable<Easing>(Easing.OutSine);

        [SettingSource("setting")]
        public Bindable<Easing> TickPitchEasing { get; } = new Bindable<Easing>(Easing.OutSine);

        [SettingSource("setting")]
        public Bindable<Easing> TickVolumeEasing { get; } = new Bindable<Easing>(Easing.OutSine);

        [SettingSource("setting")]
        public BindableDouble TickVolumeStart { get; } = new BindableDouble(0.6)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble TickVolumeEnd { get; } = new BindableDouble(1.0)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble ImpactVolume { get; } = new BindableDouble(1.0)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble BadgeDinkVolume { get; } = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble SwooshVolume { get; } = new BindableDouble(0.4)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };
    }
}
