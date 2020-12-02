// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal class DifficultySection : SetupSection
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; }

        private LabelledSliderBar<float> circleSizeSlider;
        private LabelledSliderBar<float> healthDrainSlider;
        private LabelledSliderBar<float> approachRateSlider;
        private LabelledSliderBar<float> overallDifficultySlider;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "Difficulty settings"
                },
                circleSizeSlider = new LabelledSliderBar<float>
                {
                    Label = "Object Size",
                    Description = "The size of all hit objects",
                    Current = new BindableFloat(Beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize)
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
                    Description = "The rate of passive health drain throughout playable time",
                    Current = new BindableFloat(Beatmap.Value.BeatmapInfo.BaseDifficulty.DrainRate)
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
                    Description = "The speed at which objects are presented to the player",
                    Current = new BindableFloat(Beatmap.Value.BeatmapInfo.BaseDifficulty.ApproachRate)
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
                    Description = "The harshness of hit windows and difficulty of special objects (ie. spinners)",
                    Current = new BindableFloat(Beatmap.Value.BeatmapInfo.BaseDifficulty.OverallDifficulty)
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
            Beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize = circleSizeSlider.Current.Value;
            Beatmap.Value.BeatmapInfo.BaseDifficulty.DrainRate = healthDrainSlider.Current.Value;
            Beatmap.Value.BeatmapInfo.BaseDifficulty.ApproachRate = approachRateSlider.Current.Value;
            Beatmap.Value.BeatmapInfo.BaseDifficulty.OverallDifficulty = overallDifficultySlider.Current.Value;

            editorBeatmap.UpdateAllHitObjects();
        }
    }
}
