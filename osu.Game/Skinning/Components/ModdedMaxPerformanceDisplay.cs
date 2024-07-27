// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Localisation;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Scoring;
using osu.Game.Utils;


namespace osu.Game.Skinning.Components
{
    public partial class ModdedMaxPerformanceDisplay : ModdedAttributeDisplay
    {
        protected override LocalisableString AttributeLabel => "Max Performance";

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private Bindable<StarDifficulty?> bindableDifficulty = null!;

        private CancellationTokenSource? cancellationTokenSource;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.ValueChanged += _ => updateBindableDifficulty();
            updateBindableDifficulty();
        }

        private void updateBindableDifficulty()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new();

            bindableDifficulty = (Bindable<StarDifficulty?>)difficultyCache.GetBindableDifficulty(BeatmapInfo, cancellationTokenSource.Token);

            bindableDifficulty.BindValueChanged(d =>
            {
                DifficultyAttributes? difficultyAttributes = d.NewValue?.Attributes;
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                calculateMaxPerformance(difficultyAttributes).ContinueWith(t =>
                {
                    Current.Value = t.GetResultSafely().ToLocalisableString(@"0pp");
                }, cancellationTokenSource.Token);
            });
        }

        // Explicitly do nothing as we're updating pp via BindableDifficulty
        protected override void UpdateValue()
        {
        }

        private async Task<double> calculateMaxPerformance(DifficultyAttributes? difficultyAttributes)
        {
            if (difficultyAttributes == null || cancellationTokenSource == null)
                return 0;

            var performanceCalculator = Ruleset.Value.CreateInstance().CreatePerformanceCalculator();

            if (performanceCalculator == null)
                return 0;

            IBeatmap beatmap = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value, Mods.Value, cancellationTokenSource.Token);
            ScoreInfo perfectScore = ScoreUtils.GetPerfectPlay(beatmap, Ruleset.Value, Mods.Value.ToArray());

            var performanceAttributes = await performanceCalculator.CalculateAsync(perfectScore, difficultyAttributes, cancellationTokenSource.Token).ConfigureAwait(false);

            if (performanceAttributes == null)
                return 0;

            return performanceAttributes.Total;
        }
    }
}
