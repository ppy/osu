// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
//using osu.Game.Audio;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Components;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class AudioSettings : EditorSettingsGroup
    {
        protected override string Title => @"audio";

        public AudioSettings()
        {
            EditorEnumDropdown<SampleSet> sampleSetDropdown;
            EditorSliderBar<float> volumeSliderBar;
            OsuSpriteText volumeText;
            //TriangleButton normalSampleSoundButton;
            //TriangleButton whistleSampleSoundButton;
            //TriangleButton finishSampleSoundButton;
            //TriangleButton clapSampleSoundButton;

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
                        createSettingLabelText("Default Sample Set"),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                sampleSetDropdown = new EditorEnumDropdown<SampleSet>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Items = new List<KeyValuePair<string, SampleSet>>
                                    {
                                        new KeyValuePair<string, SampleSet>("Normal", SampleSet.Normal),
                                        new KeyValuePair<string, SampleSet>("Soft", SampleSet.Soft),
                                        new KeyValuePair<string, SampleSet>("Drum", SampleSet.Drum),
                                    },
                                    //Alpha = 1
                                },
                            },
                        },
                    }
                },
                createSettingCheckBox("Enable Custom Overrides"),
                createSettingCheckBox("Samples Match Playback Rate"),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        createSettingLabelText("Sample Volume"),
                        volumeText = createSettingLabelTextBold(),
                    },
                },
                volumeSliderBar = new EditorSliderBar<float>
                {
                    NormalPrecision = 1,
                    AlternatePrecision = 1,
                    Bindable = createBindable(100, 100, 0, 100, 1),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        createSettingButton("Normal"),
                        createSettingButton("Whistle"),
                        createSettingButton("Finish"),
                        createSettingButton("Clap"),
                    },
                },
            };
            sampleSetDropdown.Current.ValueChanged += a => { };
            volumeSliderBar.Bindable.ValueChanged += showValue => volumeText.Text = $"{volumeSliderBar.Bar.TooltipText}%";
            volumeSliderBar.Bindable.TriggerChange();
        }

        //private void playSound(SampleInfo s)
        //{
        //    // TODO: Play the sounds
        //    switch (s.Name)
        //    {
        //        case "Normal":
        //            break;
        //        case "Whistle":
        //            break;
        //        case "Finish":
        //            break;
        //        case "Clap":
        //            break;
        //        default:
        //            break;
        //    }
        //}
        private TriangleButton createSettingButton(string text) => new TriangleButton
        {
            RelativeSizeAxes = Axes.X,
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            BackgroundColour = Color4.LightBlue,
            Text = text,
            Height = 30,
            Alpha = 1,
            Action = () => { /*playSound(new SampleInfo { Name = text, Bank = sampleSetDropdown.Current.Value.ToString() });*/ },
        };
        private EditorCheckbox createSettingCheckBox(string text) => new EditorCheckbox
        {
            LabelText = text,
        };
        private EditorCheckbox createSettingCheckBox(string text, bool defaultValue) => new EditorCheckbox
        {
            LabelText = text,
            Bindable = new BindableBool(defaultValue)
        };
        private OsuSpriteText createSettingLabelText(string text) => new OsuSpriteText
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            Text = text,
        };
        private OsuSpriteText createSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };
        private Bindable<float> createBindable(float value, float defaultValue, float min, float max, float precision) => new BindableFloat(value)
        {
            Default = defaultValue,
            MinValue = min,
            MaxValue = max,
            Precision = precision,
        };
    }
}
