// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;

namespace osu.Game.Rulesets.Mania.Edit.Setup
{
    public partial class ManiaDifficultySection : SetupSection
    {
        public override LocalisableString Title => EditorSetupStrings.DifficultyHeader;

        private FormSliderBar<float> keyCountSlider { get; set; } = null!;
        private FormCheckBox specialStyle { get; set; } = null!;
        private FormSliderBar<float> healthDrainSlider { get; set; } = null!;
        private FormSliderBar<float> overallDifficultySlider { get; set; } = null!;
        private FormSliderBar<double> baseVelocitySlider { get; set; } = null!;
        private FormSliderBar<double> tickRateSlider { get; set; } = null!;

        [Resolved]
        private Editor? editor { get; set; }

        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                keyCountSlider = new FormSliderBar<float>
                {
                    Caption = BeatmapsetsStrings.ShowStatsCsMania,
                    HintText = "The number of columns in the beatmap",
                    Current = new BindableFloat(Beatmap.Difficulty.CircleSize)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 1,
                    },
                    TransferValueOnCommit = true,
                    TabbableContentContainer = this,
                },
                specialStyle = new FormCheckBox
                {
                    Caption = "Use special (N+1) style",
                    HintText = "Changes one column to act as a classic \"scratch\" or \"special\" column, which can be moved around by the user's skin (to the left/right/centre). Generally used in 6K (5+1) or 8K (7+1) configurations.",
                    Current = { Value = Beatmap.SpecialStyle }
                },
                healthDrainSlider = new FormSliderBar<float>
                {
                    Caption = BeatmapsetsStrings.ShowStatsDrain,
                    HintText = EditorSetupStrings.DrainRateDescription,
                    Current = new BindableFloat(Beatmap.Difficulty.DrainRate)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    },
                    TransferValueOnCommit = true,
                    TabbableContentContainer = this,
                },
                overallDifficultySlider = new FormSliderBar<float>
                {
                    Caption = BeatmapsetsStrings.ShowStatsAccuracy,
                    HintText = EditorSetupStrings.OverallDifficultyDescription,
                    Current = new BindableFloat(Beatmap.Difficulty.OverallDifficulty)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    },
                    TransferValueOnCommit = true,
                    TabbableContentContainer = this,
                },
                baseVelocitySlider = new FormSliderBar<double>
                {
                    Caption = EditorSetupStrings.BaseVelocity,
                    HintText = EditorSetupStrings.BaseVelocityDescription,
                    Current = new BindableDouble(Beatmap.Difficulty.SliderMultiplier)
                    {
                        Default = 1.4,
                        MinValue = 0.4,
                        MaxValue = 3.6,
                        Precision = 0.01f,
                    },
                    TransferValueOnCommit = true,
                    TabbableContentContainer = this,
                },
                tickRateSlider = new FormSliderBar<double>
                {
                    Caption = EditorSetupStrings.TickRate,
                    HintText = EditorSetupStrings.TickRateDescription,
                    Current = new BindableDouble(Beatmap.Difficulty.SliderTickRate)
                    {
                        Default = 1,
                        MinValue = 1,
                        MaxValue = 4,
                        Precision = 1,
                    },
                    TransferValueOnCommit = true,
                    TabbableContentContainer = this,
                },
            };

            keyCountSlider.Current.BindValueChanged(updateKeyCount);
            healthDrainSlider.Current.BindValueChanged(_ => updateValues());
            overallDifficultySlider.Current.BindValueChanged(_ => updateValues());
            baseVelocitySlider.Current.BindValueChanged(_ => updateValues());
            tickRateSlider.Current.BindValueChanged(_ => updateValues());
        }

        private bool updatingKeyCount;

        private void updateKeyCount(ValueChangedEvent<float> keyCount)
        {
            if (updatingKeyCount) return;

            updateValues();

            if (editor == null) return;

            updatingKeyCount = true;

            editor.SaveAndReload().ContinueWith(t =>
            {
                if (!t.GetResultSafely())
                {
                    Schedule(() =>
                    {
                        changeHandler!.RestoreState(-1);
                        Beatmap.Difficulty.CircleSize = keyCountSlider.Current.Value = keyCount.OldValue;
                        updatingKeyCount = false;
                    });
                }
                else
                {
                    updatingKeyCount = false;
                }
            });
        }

        private void updateValues()
        {
            // for now, update these on commit rather than making BeatmapMetadata bindables.
            // after switching database engines we can reconsider if switching to bindables is a good direction.
            Beatmap.Difficulty.CircleSize = keyCountSlider.Current.Value;
            Beatmap.SpecialStyle = specialStyle.Current.Value;
            Beatmap.Difficulty.DrainRate = healthDrainSlider.Current.Value;
            Beatmap.Difficulty.OverallDifficulty = overallDifficultySlider.Current.Value;
            Beatmap.Difficulty.SliderMultiplier = baseVelocitySlider.Current.Value;
            Beatmap.Difficulty.SliderTickRate = tickRateSlider.Current.Value;

            Beatmap.UpdateAllHitObjects();
            Beatmap.SaveState();
        }
    }
}
