// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class DesignScreen : EditorScreen
    {
        private readonly LabelledSwitchButton enableCountdown;
        private readonly OsuSetupRadioButton halfCountdownSpeed;
        private readonly OsuSetupRadioButton normalCountdownSpeed;
        private readonly OsuSetupRadioButton doubleCountdownSpeed;
        private readonly LabelledTextBox countdownOffset;
        private readonly LabelledSwitchButton displayEpilepsyWarning;
        private readonly LabelledSwitchButton displayStoryboard;
        private readonly LabelledSwitchButton letterbox;
        private readonly LabelledSwitchButton widescreenSupport;

        public DesignScreen()
        {
            LabelledRadioButtonCollection countdownSpeed;

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
                                    Text = "Countdown",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                enableCountdown = new LabelledSwitchButton
                                {
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Enable Countdown",
                                    BottomLabelText = "Adds a \"3, 2, 1, GO!\" countdown at the beginning of the map, assuming there is enough time to do so.",
                                },
                                countdownSpeed = new LabelledRadioButtonCollection
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                    LabelText = "Countdown Speed",
                                    Items = new[]
                                    {
                                        halfCountdownSpeed = new OsuSetupRadioButton
                                        {
                                            LabelText = "Half"
                                        },
                                        normalCountdownSpeed = new OsuSetupRadioButton
                                        {
                                            LabelText = "Normal"
                                        },
                                        doubleCountdownSpeed = new OsuSetupRadioButton
                                        {
                                            LabelText = "Double"
                                        },
                                    }
                                },
                                countdownOffset = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                    LabelText = "Countdown Offset",
                                    TextBoxText = "0",
                                },
                                new OsuSpriteText
                                {
                                    Padding = new MarginPadding { Top = 10 },
                                    Colour = Color4.White,
                                    Text = "Misc. Toggles",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                widescreenSupport = new LabelledSwitchButton
                                {
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Widescreen Support",
                                },
                                displayStoryboard = new LabelledSwitchButton
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Display storyboard in front of combo fire",
                                },
                                letterbox = new LabelledSwitchButton
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Letterbox During Breaks",
                                },
                                displayEpilepsyWarning = new LabelledSwitchButton
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Display Epilepsy Effect",
                                    BottomLabelText = "This should be enabled if the storyboard contains rapid colour adjustments potential to cause seizures.",
                                },
                            }
                        },
                    },
                },
            };

            updateInfo();
            Beatmap.ValueChanged += a => updateInfo();

            enableCountdown.SwitchButtonValueChanged += a =>
            {
                Beatmap.Value.BeatmapInfo.Countdown = a;
                countdownSpeed.FadeTo(a ? 1 : 0, 250, Easing.OutQuint);
                countdownOffset.FadeTo(a ? 1 : 0, 250, Easing.OutQuint);
            };
            halfCountdownSpeed.Current.ValueChanged += a =>
            {
                if (a)
                    Beatmap.Value.BeatmapInfo.CountdownSpeed = 1;
            };
            normalCountdownSpeed.Current.ValueChanged += a =>
            {
                if (a)
                    Beatmap.Value.BeatmapInfo.CountdownSpeed = 0;
            };
            doubleCountdownSpeed.Current.ValueChanged += a =>
            {
                if (a)
                    Beatmap.Value.BeatmapInfo.CountdownSpeed = 2;
            };
            countdownOffset.TextBoxTextChanged += a =>
            {
                try
                {
                    Beatmap.Value.BeatmapInfo.CountdownOffset = Convert.ToInt32(a);
                }
                catch
                {
                    countdownOffset.TextBoxText = Beatmap.Value?.BeatmapInfo.CountdownOffset.ToString() ?? "0";
                }
            };
            widescreenSupport.SwitchButtonValueChanged += a => Beatmap.Value.BeatmapInfo.WidescreenStoryboard = a;
            displayStoryboard.SwitchButtonValueChanged += a => Beatmap.Value.BeatmapInfo.StoryFireInFront = a;
            letterbox.SwitchButtonValueChanged += a => Beatmap.Value.BeatmapInfo.LetterboxInBreaks = a;
            displayEpilepsyWarning.SwitchButtonValueChanged += a => Beatmap.Value.BeatmapInfo.EpilepsyWarning = a;
        }

        public void ChangeEnableCountdown(bool newValue) => enableCountdown.CurrentValue = newValue;
        public void ChangeWidescreenStoryboard(bool newValue) => widescreenSupport.CurrentValue = newValue;
        public void ChangeStoryFireInFront(bool newValue) => displayStoryboard.CurrentValue = newValue;
        public void ChangeLetterboxInBreaks(bool newValue) => letterbox.CurrentValue = newValue;
        public void ChangeEpilepsyWarning(bool newValue) => displayEpilepsyWarning.CurrentValue = newValue;

        private void updateInfo()
        {
            enableCountdown.CurrentValue = Beatmap.Value?.BeatmapInfo.Countdown ?? false;
            halfCountdownSpeed.Current.Value = Beatmap.Value?.BeatmapInfo.CountdownSpeed == 1;
            normalCountdownSpeed.Current.Value = Beatmap.Value?.BeatmapInfo.CountdownSpeed == 0;
            doubleCountdownSpeed.Current.Value = Beatmap.Value?.BeatmapInfo.CountdownSpeed == 2;
            countdownOffset.TextBoxText = Beatmap.Value?.BeatmapInfo.CountdownOffset.ToString() ?? "0";
            widescreenSupport.CurrentValue = Beatmap.Value?.BeatmapInfo.WidescreenStoryboard ?? false;
            displayStoryboard.CurrentValue = Beatmap.Value?.BeatmapInfo.StoryFireInFront ?? false;
            letterbox.CurrentValue = Beatmap.Value?.BeatmapInfo.LetterboxInBreaks ?? false;
            displayEpilepsyWarning.CurrentValue = Beatmap.Value?.BeatmapInfo.EpilepsyWarning ?? false;
        }
    }
}
