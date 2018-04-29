// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Screens;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class AudioSettings : EditorSettingsGroup
    {
        private readonly EditorSliderBar<float> volumeSliderBar;
        private readonly OsuSpriteText volumeLabel;
        private readonly OsuSpriteText volumeText;
        private readonly TriangleButton normalSampleSoundButton;
        private readonly TriangleButton whistleSampleSoundButton;
        private readonly TriangleButton finishSampleSoundButton;
        private readonly TriangleButton clapSampleSoundButton;
        private readonly EditorEnumDropdown<SampleSet> modeDropdown;

        protected override string Title => @"audio";

        public AudioSettings()
        {
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        CreateSettingLabelText("Default Sample Set"),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                modeDropdown = new EditorEnumDropdown<SampleSet>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Items = new KeyValuePair<string, SampleSet>[]
                                    {
                                        new KeyValuePair<string, SampleSet>("Normal", SampleSet.Normal),
                                        new KeyValuePair<string, SampleSet>("Soft", SampleSet.Soft),
                                        new KeyValuePair<string, SampleSet>("Drum", SampleSet.Drum),
                                    },
                                    Alpha = 1,
                                },
                            },
                        },
                    }
                },
                CreateSettingCheckBox("Enable Custom Overrides"),
                CreateSettingCheckBox("Samples Match Playback Rate"),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        volumeLabel = CreateSettingLabelText("Sample Volume"),
                        volumeText = CreateSettingLabelTextBold(),
                    },
                },
                volumeSliderBar = new EditorSliderBar<float>
                {
                    NormalPrecision = 1,
                    AlternatePrecision = 1,
                    Bindable = CreateBindable(100, 100, 0, 100, 1),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        normalSampleSoundButton = CreateSettingButton("Normal"),
                        whistleSampleSoundButton = CreateSettingButton("Whistle"),
                        finishSampleSoundButton = CreateSettingButton("Finish"),
                        clapSampleSoundButton = CreateSettingButton("Clap"),
                    },
                },
            };
            volumeSliderBar.Bindable.ValueChanged += showValue => volumeText.Text = $"{volumeSliderBar.Bar.TooltipText}%";
            volumeSliderBar.Bindable.TriggerChange();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        void PlaySound(osu.Game.Audio.SampleInfo s)
        {
            // TODO: Play the sounds
            switch (s.Name)
            {
                case "Normal":
                    break;
                case "Whistle":
                    break;
                case "Finish":
                    break;
                case "Clap":
                    break;
                default:
                    break;
            }
        }
        TriangleButton CreateSettingButton(string text) => new TriangleButton
        {
            RelativeSizeAxes = Axes.X,
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            BackgroundColour = Color4.LightBlue,
            Text = text,
            Height = 30,
            Alpha = 1,
            Action = () => { PlaySound(new Audio.SampleInfo()); },
        };
        EditorCheckbox CreateSettingCheckBox(string text) => new EditorCheckbox
        {
            LabelText = text,
        };
        EditorCheckbox CreateSettingCheckBox(string text, bool defaultValue) => new EditorCheckbox
        {
            LabelText = text,
            Bindable = new BindableBool(defaultValue)
        };
        OsuSpriteText CreateSettingLabelText(string text) => new OsuSpriteText
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            Text = text,
        };
        OsuSpriteText CreateSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };
        Bindable<float> CreateBindable(float value, float defaultValue, float min, float max, float precision) => new BindableFloat(value)
        {
            Default = defaultValue,
            MinValue = min,
            MaxValue = max,
            Precision = precision,
        };
    }
}
