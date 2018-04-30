// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Globalization;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Components;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class DifficultySettings : EditorSettingsGroup
    {
        private readonly ColouredEditorSliderBar<float> approachRateSliderBar;
        private readonly ColouredEditorSliderBar<float> circleSizeSliderBar;
        private readonly OsuSpriteText approachRateText;
        private readonly OsuSpriteText circleSizeText;
        //private readonly OsuSpriteText overallDifficultyLabel;
        //private readonly OsuSpriteText hpDrainLabel;
        private readonly OsuSpriteText approachRateLabel;
        private readonly OsuSpriteText circleSizeLabel;
        private readonly FillFlowContainer ffc;

        protected override string Title => @"difficulty";

        public DifficultySettings()
        {
            ColouredEditorSliderBar<float> overallDifficultySliderBar;
            ColouredEditorSliderBar<float> hpDrainRateSliderBar;
            OsuSpriteText overallDifficultyText;
            OsuSpriteText hpDrainText;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        createSettingLabelText("Overall Difficulty"),
                        overallDifficultyText = createSettingLabelTextBold(),
                    },
                },
                overallDifficultySliderBar = createSliderBar(5, 5, 0, 10),
                ffc = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 15),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        createSettingLabelText("HP Drain Rate"),
                                        hpDrainText = createSettingLabelTextBold(),
                                    },
                                },
                                hpDrainRateSliderBar = createSliderBar(5, 5, 0, 10),
                            }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        approachRateLabel = createSettingLabelText("Approach Rate"),
                                        approachRateText = createSettingLabelTextBold(),
                                    },
                                },
                                approachRateSliderBar = createSliderBar(5, 5, 0, 10),
                            }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        circleSizeLabel = createSettingLabelText("Circle Size"),
                                        circleSizeText = createSettingLabelTextBold(),
                                    },
                                },
                                circleSizeSliderBar = createSliderBar(5, 5, 2, 7),
                            }
                        }
                    }
                }
            };

            // I am really sorry for your eyes
            hpDrainRateSliderBar.Bindable.ValueChanged += showValue => hpDrainText.Text = $"{trimUnnecessaryDecimalPart(hpDrainRateSliderBar.Bar.Current.Value.ToString("N1", CultureInfo.InvariantCulture))}";
            hpDrainRateSliderBar.Bindable.TriggerChange();
            circleSizeSliderBar.Bindable.ValueChanged += showValue => circleSizeText.Text = $"{trimUnnecessaryDecimalPart(circleSizeSliderBar.Bar.Current.Value.ToString("N1", CultureInfo.InvariantCulture))}";
            circleSizeSliderBar.Bindable.TriggerChange();
            approachRateSliderBar.Bindable.ValueChanged += showValue => approachRateText.Text = $"{trimUnnecessaryDecimalPart(approachRateSliderBar.Bar.Current.Value.ToString("N1", CultureInfo.InvariantCulture))}";
            approachRateSliderBar.Bindable.TriggerChange();
            overallDifficultySliderBar.Bindable.ValueChanged += showValue => overallDifficultyText.Text = $"{trimUnnecessaryDecimalPart(overallDifficultySliderBar.Bar.Current.Value.ToString("N1", CultureInfo.InvariantCulture))}";
            overallDifficultySliderBar.Bindable.TriggerChange();
        }

        private string trimUnnecessaryDecimalPart(string s)
        {
            int index = -1;
            for (int i = 0; i < s.Length && index < 0; i++)
                if (s[i] == '.')
                    index = i;
            if (index > -1) // Number contains decimal part
            {
                int finalLength = s.Length;
                while (s[finalLength - 1] == '0')
                    finalLength--;
                if (s[finalLength - 1] == '.')
                    finalLength--;
                return s.Substring(0, finalLength);
            }
            else return s;
        }

        public void HideApproachRateAndCircleSize()
        {
            ffc.Spacing = new Vector2(0, 0);
            ChangeApproachRateAlpha(0);
            ChangeCircleSizeAlpha(0);
        }
        public void ShowApproachRateAndCircleSize()
        {
            ffc.Spacing = new Vector2(0, 15);
            ChangeApproachRateAlpha(1);
            ChangeCircleSizeAlpha(1);
        }
        public void ChangeApproachRateAlpha(float alpha) => approachRateSliderBar.Alpha = approachRateText.Alpha = approachRateLabel.Alpha = alpha;
        public void ChangeCircleSizeAlpha(float alpha) => circleSizeSliderBar.Alpha = circleSizeText.Alpha = circleSizeLabel.Alpha = alpha;

        private OsuSpriteText createSettingLabelText(string text) => new OsuSpriteText
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Text = text,
        };
        private OsuSpriteText createSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };
        private ColouredEditorSliderBar<float> createSliderBar(float value, float defaultValue, float min, float max) => new ColouredEditorSliderBar<float>
        {
            NormalPrecision = 1,
            AlternatePrecision = 0.1f,
            Bindable = createBindable(value, defaultValue, min, max, 1),
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
