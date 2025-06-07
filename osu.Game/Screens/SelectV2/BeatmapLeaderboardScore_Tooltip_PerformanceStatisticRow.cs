// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapLeaderboardScore
    {
        public partial class LeaderboardScoreTooltip
        {
            public partial class PerformanceStatisticRow : StatisticRow
            {
                private readonly ScoreInfo score;

                public PerformanceStatisticRow(LocalisableString label, Color4 labelColour, ScoreInfo score)
                    : base(label, labelColour, 0.ToLocalisableString("N0"))
                {
                    this.score = score;
                }

                [BackgroundDependencyLoader]
                private void load(BeatmapDifficultyCache difficultyCache, CancellationToken? cancellationToken)
                {
                    if (score.PP.HasValue)
                    {
                        setPerformanceValue(score, score.PP.Value);
                        return;
                    }

                    Task.Run(async () =>
                    {
                        var attributes = await difficultyCache.GetDifficultyAsync(score.BeatmapInfo!, score.Ruleset, score.Mods, cancellationToken ?? default).ConfigureAwait(false);
                        var performanceCalculator = score.Ruleset.CreateInstance().CreatePerformanceCalculator();

                        // Performance calculation requires the beatmap and ruleset to be locally available. If not, return a default value.
                        if (attributes?.DifficultyAttributes == null || performanceCalculator == null)
                            return;

                        var result = await performanceCalculator.CalculateAsync(score, attributes.Value.DifficultyAttributes, cancellationToken ?? default).ConfigureAwait(false);

                        Schedule(() => setPerformanceValue(score, result.Total));
                    }, cancellationToken ?? default);
                }

                private void setPerformanceValue(ScoreInfo scoreInfo, double? pp)
                {
                    if (pp.HasValue)
                    {
                        int ppValue = (int)Math.Round(pp.Value, MidpointRounding.AwayFromZero);
                        ValueLabel.Text = ppValue.ToLocalisableString("N0");

                        if (!scoreInfo.BeatmapInfo!.Status.GrantsPerformancePoints() || hasUnrankedMods(scoreInfo))
                            Alpha = 0.5f;
                        else
                            Alpha = 1f;
                    }
                }

                private static bool hasUnrankedMods(ScoreInfo scoreInfo)
                {
                    IEnumerable<Mod> modsToCheck = scoreInfo.Mods;

                    if (scoreInfo.IsLegacyScore)
                        modsToCheck = modsToCheck.Where(m => m is not ModClassic);

                    return modsToCheck.Any(m => !m.Ranked);
                }
            }
        }
    }
}
