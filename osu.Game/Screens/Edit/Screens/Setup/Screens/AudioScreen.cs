// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents;
using System;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class AudioScreen : EditorScreen
    {
        private readonly LabelledEnumDropdown<SampleBank> defaultSampleBank;
        private readonly LabelledSliderBar defaultSampleVolume;
        private readonly LabelledSwitchButton samplesMatchPlaybackRate;

        private readonly OsuSetupCircularButton normalSample;
        private readonly OsuSetupCircularButton whistleSample;
        private readonly OsuSetupCircularButton finishSample;
        private readonly OsuSetupCircularButton clapSample;

        private readonly Container resetDefaultSampleBankSettingsContainer;
        private readonly Container resetDefaultSampleVolumeSettingsContainer;
        private readonly OsuSetupCircularButton resetDefaultSampleBankSettingsButton;
        private readonly OsuSetupCircularButton resetDefaultSampleVolumeSettingsButton;

        private readonly Container sampleBankSettings;
        private readonly Container sampleVolumeSettings;
        private readonly FillFlowContainer sampleBankSettingsContainer;
        private readonly FillFlowContainer sampleVolumeSettingsContainer;

        public const float DEFAULT_LABEL_TEXT_SIZE = 12;

        public AudioScreen()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding { Left = Setup.SCREEN_LEFT_PADDING, Top = Setup.SCREEN_TOP_PADDING },
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Spacing = new Vector2(3),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Colour = Color4.White,
                                    Text = "Default Sample Settings",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                sampleBankSettings = new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    Height = 40,
                                    Children = new Drawable[]
                                    {
                                        sampleBankSettingsContainer = new FillFlowContainer
                                        {
                                            Direction = FillDirection.Vertical,
                                            RelativeSizeAxes = Axes.X,
                                            Spacing = new Vector2(3),
                                            Children = new Drawable[]
                                            {
                                                defaultSampleBank = new LabelledEnumDropdown<SampleBank>
                                                {
                                                    LabelText = "Sample Bank",
                                                },
                                            }
                                        },
                                        resetDefaultSampleBankSettingsContainer = new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 60,
                                            Alpha = 0,
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    CornerRadius = 15,
                                                    Masking = true,
                                                    Height = 50,
                                                    Children = new Drawable[]
                                                    {
                                                        new Box
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Colour = OsuColour.FromHex("1c2125"),
                                                        },
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Height = 50,
                                                            Children = new Drawable[]
                                                            {
                                                                new OsuSpriteText
                                                                {
                                                                    Anchor = Anchor.CentreLeft,
                                                                    Origin = Anchor.CentreLeft,
                                                                    Padding = new MarginPadding { Left = 15 },
                                                                    Colour = Color4.White,
                                                                    TextSize = DEFAULT_LABEL_TEXT_SIZE,
                                                                    Text = "This beatmap has timing-section-dependent sample bank settings, therefore you cannot set beatmap-wide settings here.",
                                                                    Font = @"Exo2.0-Bold",
                                                                },
                                                                resetDefaultSampleBankSettingsButton = new OsuSetupCircularButton
                                                                {
                                                                    Anchor = Anchor.CentreRight,
                                                                    Origin = Anchor.CentreRight,
                                                                    Margin = new MarginPadding { Right = 15 },
                                                                    LabelText = "Reset Settings",
                                                                    Width = 125,
                                                                },
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                    }
                                },
                                sampleVolumeSettings = new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = LabelledSliderBar.NORMAL_HEIGHT,
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    Children = new Drawable[]
                                    {
                                        sampleVolumeSettingsContainer = new FillFlowContainer
                                        {
                                            Direction = FillDirection.Vertical,
                                            RelativeSizeAxes = Axes.X,
                                            Spacing = new Vector2(3),
                                            Children = new Drawable[]
                                            {
                                                defaultSampleVolume = new LabelledSliderBar
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    LabelText = "Sample Volume",
                                                    TooltipTextSuffix = "%",
                                                    SliderMaxValue = 100,
                                                    SliderMinValue = 0,
                                                    SliderNormalPrecision = 10,
                                                    SliderAlternatePrecision = 1,
                                                },
                                            }
                                        },
                                        resetDefaultSampleVolumeSettingsContainer = new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 60,
                                            Alpha = 0,
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    CornerRadius = 15,
                                                    Masking = true,
                                                    Height = 50,
                                                    Children = new Drawable[]
                                                    {
                                                        new Box
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Colour = OsuColour.FromHex("1c2125"),
                                                        },
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Height = 50,
                                                            Children = new Drawable[]
                                                            {
                                                                new OsuSpriteText
                                                                {
                                                                    Anchor = Anchor.CentreLeft,
                                                                    Origin = Anchor.CentreLeft,
                                                                    Padding = new MarginPadding { Left = 15 },
                                                                    Colour = Color4.White,
                                                                    TextSize = DEFAULT_LABEL_TEXT_SIZE,
                                                                    Text = "This beatmap has timing-section-dependent sample volume settings, therefore you cannot set beatmap-wide settings here.",
                                                                    Font = @"Exo2.0-Bold",
                                                                },
                                                                resetDefaultSampleVolumeSettingsButton = new OsuSetupCircularButton
                                                                {
                                                                    Anchor = Anchor.CentreRight,
                                                                    Origin = Anchor.CentreRight,
                                                                    Margin = new MarginPadding { Right = 15 },
                                                                    LabelText = "Reset Settings",
                                                                    Width = 125,
                                                                },
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                    }
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    Height = 60,
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 50,
                                            Padding = new MarginPadding { Top = 10 },
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    CornerRadius = 15,
                                                    Masking = true,
                                                    Height = 50,
                                                    Children = new Drawable[]
                                                    {
                                                        new Box
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Colour = OsuColour.FromHex("1c2125"),
                                                        },
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Height = 50,
                                                            Children = new Drawable[]
                                                            {
                                                                new OsuSpriteText
                                                                {
                                                                    Anchor = Anchor.CentreLeft,
                                                                    Origin = Anchor.CentreLeft,
                                                                    Padding = new MarginPadding { Left = 15 },
                                                                    Colour = Color4.White,
                                                                    TextSize = 16,
                                                                    Text = "Sample Playtest",
                                                                    Font = @"Exo2.0-Bold",
                                                                },
                                                                new FillFlowContainer
                                                                {
                                                                    Anchor = Anchor.TopRight,
                                                                    Origin = Anchor.TopRight,
                                                                    Direction = FillDirection.Horizontal,
                                                                    Spacing = new Vector2(5),
                                                                    Margin = new MarginPadding { Top = 10, Right = 15 },
                                                                    Children = new[]
                                                                    {
                                                                        clapSample = new OsuSetupCircularButton
                                                                        {
                                                                            Anchor = Anchor.TopRight,
                                                                            Origin = Anchor.TopRight,
                                                                            LabelText = "Clap"
                                                                        },
                                                                        finishSample = new OsuSetupCircularButton
                                                                        {
                                                                            Anchor = Anchor.TopRight,
                                                                            Origin = Anchor.TopRight,
                                                                            LabelText = "Finish"
                                                                        },
                                                                        whistleSample = new OsuSetupCircularButton
                                                                        {
                                                                            Anchor = Anchor.TopRight,
                                                                            Origin = Anchor.TopRight,
                                                                            LabelText = "Whistle"
                                                                        },
                                                                        normalSample = new OsuSetupCircularButton
                                                                        {
                                                                            Anchor = Anchor.TopRight,
                                                                            Origin = Anchor.TopRight,
                                                                            LabelText = "Normal"
                                                                        },
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                    }
                                },
                                new OsuSpriteText
                                {
                                    Padding = new MarginPadding { Top = 20 },
                                    Colour = Color4.White,
                                    Text = "Misc. Toggles",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                samplesMatchPlaybackRate = new LabelledSwitchButton
                                {
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Samples Match Playback Rate",
                                    BottomLabelText = "This option is suitable for fully-hitsounded maps.",
                                },
                            }
                        },
                    },
                },
            };

            updateInfo();
            Beatmap.ValueChanged += a => updateInfo();

            samplesMatchPlaybackRate.SwitchButtonValueChanged += a => Beatmap.Value.BeatmapInfo.SamplesMatchPlaybackRate = a;
            defaultSampleVolume.SliderBarValueChanged += a =>
            {
                foreach (var s in Beatmap.Value.Beatmap.ControlPointInfo.SamplePoints)
                    s.SampleVolume = (int)a;
            };
            defaultSampleBank.DropdownSelectionChanged += a =>
            {
                foreach (var s in Beatmap.Value.Beatmap.ControlPointInfo.SamplePoints)
                    s.SampleBank = a.ToString().ToLower();
            };

            resetDefaultSampleBankSettingsButton.ButtonClicked += ResetDefaultSampleBanks;
            resetDefaultSampleVolumeSettingsButton.ButtonClicked += ResetDefaultSampleVolumes;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            normalSample.DefaultColour = osuColour.Purple;
            whistleSample.DefaultColour = osuColour.Purple;
            finishSample.DefaultColour = osuColour.Purple;
            clapSample.DefaultColour = osuColour.Purple;
            resetDefaultSampleBankSettingsButton.DefaultColour = osuColour.BlueDark;
            resetDefaultSampleVolumeSettingsButton.DefaultColour = osuColour.BlueDark;
        }

        // According to osu!stable's behaviour
        public void ResetDefaultSampleBanks()
        {
            string bank = Beatmap.Value.Beatmap.ControlPointInfo.SamplePoints.First().SampleBank;
            foreach (var s in Beatmap.Value.Beatmap.ControlPointInfo.SamplePoints)
                s.SampleBank = bank;
            updateInfo();
        }
        public void ResetDefaultSampleVolumes()
        {
            int volume = Beatmap.Value.Beatmap.ControlPointInfo.SamplePoints.First().SampleVolume;
            foreach (var s in Beatmap.Value.Beatmap.ControlPointInfo.SamplePoints)
                s.SampleVolume = volume;
            updateInfo();
        }
        public void ChangeDefaultSampleBank(SampleBank newValue) => defaultSampleBank.DropdownSelectedItem = newValue;
        public void ChangeDefaultSampleVolume(int newValue)
        {
            // For test purposes only
            var normalPrecision = defaultSampleVolume.SliderNormalPrecision;
            defaultSampleVolume.SliderNormalPrecision = defaultSampleVolume.SliderAlternatePrecision;
            defaultSampleVolume.CurrentValue = newValue;
            defaultSampleVolume.SliderNormalPrecision = normalPrecision;
        }
        public void ChangeSamplesMatchPlaybackRate(bool newValue) => samplesMatchPlaybackRate.CurrentValue = newValue;

        private void updateInfo()
        {
            samplesMatchPlaybackRate.CurrentValue = Beatmap.Value?.BeatmapInfo.SamplesMatchPlaybackRate ?? false;

            string commonBank = "";
            int? commonVolume = -1;

            if (Beatmap.Value != null)
            {
                foreach (var s in Beatmap.Value.Beatmap.ControlPointInfo.SamplePoints)
                {
                    if (commonBank == "")
                        commonBank = s.SampleBank;
                    else if (commonBank != s.SampleBank)
                    {
                        commonBank = null;
                        break;
                    }
                }
                foreach (var s in Beatmap.Value.Beatmap.ControlPointInfo.SamplePoints)
                {
                    if (commonVolume == -1)
                        commonVolume = s.SampleVolume;
                    else if (commonVolume != s.SampleVolume)
                    {
                        commonVolume = null;
                        break;
                    }
                }
            }

            if (commonBank != null)
            {
                if (commonBank == "")
                    defaultSampleBank.DropdownSelectedItem = SampleBank.Normal;
                else
                {
                    Enum.TryParse<SampleBank>(commonBank, out var i);
                    defaultSampleBank.DropdownSelectedItem = i;
                }
            }
            sampleBankSettingsContainer.Alpha = Convert.ToInt32(commonBank != null);
            resetDefaultSampleBankSettingsContainer.Alpha = Convert.ToInt32(commonBank == null);
            sampleBankSettings.Height = commonBank != null ? 40 : 60;

            if (commonVolume != null)
            {
                if (commonVolume == -1)
                    defaultSampleVolume.CurrentValue = 100;
                else
                    defaultSampleVolume.CurrentValue = (float)commonVolume;
            }
            sampleVolumeSettingsContainer.Alpha = Convert.ToInt32(commonVolume != null);
            resetDefaultSampleVolumeSettingsContainer.Alpha = Convert.ToInt32(commonVolume == null);
            sampleVolumeSettings.Height = commonVolume != null ? LabelledSliderBar.NORMAL_HEIGHT : 50;
        }
    }

    public enum SampleBank
    {
        [System.ComponentModel.Description("Normal")]
        Normal = 0,
        [System.ComponentModel.Description("Soft")]
        Soft = 1,
        [System.ComponentModel.Description("Drum")]
        Drum = 2,
    }
}
