// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
using System.Threading;
using osu.Framework.Localisation;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Skinning.Components
{
    public partial class ModdedStarRatingDisplay : ModdedAttributeDisplay
    {
        protected override LocalisableString AttributeLabel => BeatmapsetsStrings.ShowStatsStars;

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
                StarDifficulty starDifficulty = d.NewValue ?? new StarDifficulty();
                Current.Value = starDifficulty.Stars.ToLocalisableString(@"F2");
            });
        }

        // Explicitly do nothing as we're updating pp via BindableDifficulty
        protected override void UpdateValue()
        {
        }
    }
}
