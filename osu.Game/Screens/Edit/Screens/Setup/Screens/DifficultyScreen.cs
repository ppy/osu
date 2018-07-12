// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Screens.Setup.BottomHeaders;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class DifficultyScreen : EditorScreen
    {
        private readonly LabelledSliderBar hpDrainRate;
        private readonly LabelledSliderBar overallDifficulty;
        private readonly LabelledSliderBar circleSize;
        private readonly LabelledSliderBar approachRate;

        public DifficultyScreen()
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
                                    Text = "Difficulty",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                hpDrainRate = new LabelledSliderBar
                                {
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    SliderMinValue = 0,
                                    SliderMaxValue = 10,
                                    SliderNormalPrecision = 1,
                                    SliderAlternatePrecision = 0.1f,
                                    LeftTickCaption = "Easy",
                                    RightTickCaption = "Insane",
                                    LabelText = "HP Drain Rate",
                                    BottomLabelText = "The constant rate of health-bar drain throughout the song.",
                                },
                                overallDifficulty = new LabelledSliderBar
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    SliderMinValue = 0,
                                    SliderMaxValue = 10,
                                    SliderNormalPrecision = 1,
                                    SliderAlternatePrecision = 0.1f,
                                    LeftTickCaption = "Easy",
                                    RightTickCaption = "Insane",
                                    LabelText = "Overall Difficulty",
                                    BottomLabelText = "The harshness of the hit window.",
                                },
                                circleSize = new LabelledSliderBar
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    SliderMinValue = 2,
                                    SliderMaxValue = 7,
                                    SliderNormalPrecision = 1,
                                    SliderAlternatePrecision = 0.1f,
                                    Alpha = 0,
                                    LeftTickCaption = "Large",
                                    MiddleTickCaption = "Normal",
                                    RightTickCaption = "Small",
                                    LabelText = "Circle Size",
                                    BottomLabelText = "The radial size of the hit circles and sliders in osu! and the fruit in osu!catch.",
                                },
                                approachRate = new LabelledSliderBar
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    SliderMinValue = 0,
                                    SliderMaxValue = 10,
                                    SliderNormalPrecision = 1,
                                    SliderAlternatePrecision = 0.1f,
                                    Alpha = 0,
                                    LeftTickCaption = "Slow",
                                    RightTickCaption = "Fast",
                                    LabelText = "Approach Rate",
                                    BottomLabelText = "The speed at which objects appear.",
                                }
                            }
                        },
                        new DifficultyScreenBottomHeader
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Padding = new MarginPadding { Left = Setup.SCREEN_LEFT_PADDING, Top = Setup.SCREEN_BOTTOM_HEADER_TOP_PADDING },
                        }
                    },
                },
            };

            updateInfo();
            Beatmap.ValueChanged += a => updateInfo();

            hpDrainRate.SliderBarValueChanged += a => Beatmap.Value.BeatmapInfo.BaseDifficulty.DrainRate = a;
            overallDifficulty.SliderBarValueChanged += a => Beatmap.Value.BeatmapInfo.BaseDifficulty.OverallDifficulty = a;
            circleSize.SliderBarValueChanged += a => Beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize = a;
            approachRate.SliderBarValueChanged += a => Beatmap.Value.BeatmapInfo.BaseDifficulty.ApproachRate = a;
        }

        public void ChangeHpDrainRate(float newValue) => hpDrainRate.CurrentValue = newValue;
        public void ChangeOverallDifficulty(float newValue) => overallDifficulty.CurrentValue = newValue;
        public void ChangeCircleSize(float newValue) => circleSize.CurrentValue = newValue;
        public void ChangeApproachRate(float newValue) => approachRate.CurrentValue = newValue;

        private void updateInfo()
        {
            hpDrainRate.CurrentValue = Beatmap.Value?.BeatmapInfo.BaseDifficulty.DrainRate ?? 5;
            overallDifficulty.CurrentValue = Beatmap.Value?.BeatmapInfo.BaseDifficulty.OverallDifficulty ?? 5;
            circleSize.CurrentValue = Beatmap.Value?.BeatmapInfo.BaseDifficulty.CircleSize ?? 5;
            approachRate.CurrentValue = Beatmap.Value?.BeatmapInfo.BaseDifficulty.ApproachRate ?? 5;

            var ruleset = Beatmap.Value?.BeatmapInfo.RulesetID;
            approachRate.Alpha = circleSize.Alpha = ruleset == 1 || ruleset == 3 ? 0 : 1;
        }
    }
}
