// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class DifficultySection : SetupSection
    {
        protected LabelledSliderBar<float> CircleSizeSlider { get; private set; } = null!;
        protected LabelledSliderBar<float> HealthDrainSlider { get; private set; } = null!;
        protected LabelledSliderBar<float> ApproachRateSlider { get; private set; } = null!;
        protected LabelledSliderBar<float> OverallDifficultySlider { get; private set; } = null!;
        protected LabelledSliderBar<double> BaseVelocitySlider { get; private set; } = null!;
        protected LabelledSliderBar<double> TickRateSlider { get; private set; } = null!;

        public override LocalisableString Title => EditorSetupStrings.DifficultyHeader;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                CircleSizeSlider = new LabelledSliderBar<float>
                {
                    Label = BeatmapsetsStrings.ShowStatsCs,
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = EditorSetupStrings.CircleSizeDescription,
                    Current = new BindableFloat(Beatmap.Difficulty.CircleSize)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
                HealthDrainSlider = new LabelledSliderBar<float>
                {
                    Label = BeatmapsetsStrings.ShowStatsDrain,
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = EditorSetupStrings.DrainRateDescription,
                    Current = new BindableFloat(Beatmap.Difficulty.DrainRate)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
                ApproachRateSlider = new LabelledSliderBar<float>
                {
                    Label = BeatmapsetsStrings.ShowStatsAr,
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = EditorSetupStrings.ApproachRateDescription,
                    Current = new BindableFloat(Beatmap.Difficulty.ApproachRate)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
                OverallDifficultySlider = new LabelledSliderBar<float>
                {
                    Label = BeatmapsetsStrings.ShowStatsAccuracy,
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = EditorSetupStrings.OverallDifficultyDescription,
                    Current = new BindableFloat(Beatmap.Difficulty.OverallDifficulty)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
                BaseVelocitySlider = new LabelledSliderBar<double>
                {
                    Label = EditorSetupStrings.BaseVelocity,
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = EditorSetupStrings.BaseVelocityDescription,
                    Current = new BindableDouble(Beatmap.Difficulty.SliderMultiplier)
                    {
                        Default = 1.4,
                        MinValue = 0.4,
                        MaxValue = 3.6,
                        Precision = 0.01f,
                    }
                },
                TickRateSlider = new LabelledSliderBar<double>
                {
                    Label = EditorSetupStrings.TickRate,
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = EditorSetupStrings.TickRateDescription,
                    Current = new BindableDouble(Beatmap.Difficulty.SliderTickRate)
                    {
                        Default = 1,
                        MinValue = 1,
                        MaxValue = 4,
                        Precision = 1,
                    }
                },
            };

            foreach (var item in Children.OfType<LabelledSliderBar<float>>())
                item.Current.ValueChanged += _ => updateValues();

            foreach (var item in Children.OfType<LabelledSliderBar<double>>())
                item.Current.ValueChanged += _ => updateValues();
        }

        private void updateValues()
        {
            // for now, update these on commit rather than making BeatmapMetadata bindables.
            // after switching database engines we can reconsider if switching to bindables is a good direction.
            Beatmap.Difficulty.CircleSize = CircleSizeSlider.Current.Value;
            Beatmap.Difficulty.DrainRate = HealthDrainSlider.Current.Value;
            Beatmap.Difficulty.ApproachRate = ApproachRateSlider.Current.Value;
            Beatmap.Difficulty.OverallDifficulty = OverallDifficultySlider.Current.Value;
            Beatmap.Difficulty.SliderMultiplier = BaseVelocitySlider.Current.Value;
            Beatmap.Difficulty.SliderTickRate = TickRateSlider.Current.Value;

            Beatmap.UpdateAllHitObjects();
            Beatmap.SaveState();
        }
    }
}
