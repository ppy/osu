// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal class DifficultySection : SetupSection
    {
        private LabelledSliderBar<float> circleSizeSlider;
        private LabelledSliderBar<float> healthDrainSlider;
        private LabelledSliderBar<float> approachRateSlider;
        private LabelledSliderBar<float> overallDifficultySlider;

        public override LocalisableString Title => "Difficulty";

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                circleSizeSlider = new LabelledSliderBar<float>
                {
                    Label = "Object Size",
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = "The size of all hit objects",
                    Current = new BindableFloat(Beatmap.Difficulty.CircleSize)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
                healthDrainSlider = new LabelledSliderBar<float>
                {
                    Label = "Health Drain",
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = "The rate of passive health drain throughout playable time",
                    Current = new BindableFloat(Beatmap.Difficulty.DrainRate)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
                approachRateSlider = new LabelledSliderBar<float>
                {
                    Label = "Approach Rate",
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = "The speed at which objects are presented to the player",
                    Current = new BindableFloat(Beatmap.Difficulty.ApproachRate)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
                overallDifficultySlider = new LabelledSliderBar<float>
                {
                    Label = "Overall Difficulty",
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = "The harshness of hit windows and difficulty of special objects (ie. spinners)",
                    Current = new BindableFloat(Beatmap.Difficulty.OverallDifficulty)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
            };

            foreach (var item in Children.OfType<LabelledSliderBar<float>>())
                item.Current.ValueChanged += onValueChanged;
        }

        private void onValueChanged(ValueChangedEvent<float> args)
        {
            // for now, update these on commit rather than making BeatmapMetadata bindables.
            // after switching database engines we can reconsider if switching to bindables is a good direction.
            Beatmap.Difficulty.CircleSize = circleSizeSlider.Current.Value;
            Beatmap.Difficulty.DrainRate = healthDrainSlider.Current.Value;
            Beatmap.Difficulty.ApproachRate = approachRateSlider.Current.Value;
            Beatmap.Difficulty.OverallDifficulty = overallDifficultySlider.Current.Value;

            Beatmap.UpdateAllHitObjects();
        }
    }
}
