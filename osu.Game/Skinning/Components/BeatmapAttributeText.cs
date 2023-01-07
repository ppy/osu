// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class BeatmapAttributeText : Container, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Attribute", "The attribute to be displayed.")]
        public Bindable<BeatmapAttribute> Attribute { get; } = new Bindable<BeatmapAttribute>(BeatmapAttribute.StarRating);

        [SettingSource("Template", "Supports {Label} and {Value}, but also including arbitrary attributes like {StarRating} (see attribute list for supported values).")]
        public Bindable<string> Template { get; set; } = new Bindable<string>("{Label}: {Value}");

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private ModSettingChangeTracker? modSettingChangeTracker;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private CancellationTokenSource? starDifficultyCancellationSource;
        private StarDifficulty? starRating;

        private CancellationTokenSource? preStarDifficultyCancellationSource;
        private StarDifficulty? preStarRating;

        private BeatmapDifficulty difficulty = null!;

        private static readonly ImmutableDictionary<BeatmapAttribute, LocalisableString> label_dictionary = new Dictionary<BeatmapAttribute, LocalisableString>
        {
            [BeatmapAttribute.CircleSize] = BeatmapsetsStrings.ShowStatsCs,
            [BeatmapAttribute.PreModCircleSize] = "Pre Mod " + BeatmapsetsStrings.ShowStatsCs,
            [BeatmapAttribute.Accuracy] = BeatmapsetsStrings.ShowStatsAccuracy,
            [BeatmapAttribute.PreModAccuracy] = "Pre Mod " + BeatmapsetsStrings.ShowStatsAccuracy,
            [BeatmapAttribute.HPDrain] = BeatmapsetsStrings.ShowStatsDrain,
            [BeatmapAttribute.PreModHPDrain] = "Pre Mod " + BeatmapsetsStrings.ShowStatsDrain,
            [BeatmapAttribute.ApproachRate] = BeatmapsetsStrings.ShowStatsAr,
            [BeatmapAttribute.PreModApproachRate] = "Pre Mod " + BeatmapsetsStrings.ShowStatsAr,
            [BeatmapAttribute.StarRating] = BeatmapsetsStrings.ShowStatsStars,
            [BeatmapAttribute.PreModStarRating] = "Pre Mod " + BeatmapsetsStrings.ShowStatsStars,
            [BeatmapAttribute.Title] = EditorSetupStrings.Title,
            [BeatmapAttribute.Artist] = EditorSetupStrings.Artist,
            [BeatmapAttribute.DifficultyName] = EditorSetupStrings.DifficultyHeader,
            [BeatmapAttribute.Creator] = EditorSetupStrings.Creator,
            [BeatmapAttribute.Length] = ArtistStrings.TracklistLength.ToTitle(),
            [BeatmapAttribute.PreModLength] = "Pre Mod " + ArtistStrings.TracklistLength.ToTitle(),
            [BeatmapAttribute.RankedStatus] = BeatmapDiscussionsStrings.IndexFormBeatmapsetStatusDefault,
            [BeatmapAttribute.BPM] = BeatmapsetsStrings.ShowStatsBpm,
            [BeatmapAttribute.PreModBPM] = "Pre Mod " + BeatmapsetsStrings.ShowStatsBpm,
            [BeatmapAttribute.BPMMinimum] = ArtistStrings.TracksIndexFormBpmGte,
            [BeatmapAttribute.PreModBPMMinimum] = "Pre Mod " + ArtistStrings.TracksIndexFormBpmGte,
            [BeatmapAttribute.BPMMaximum] = ArtistStrings.TracksIndexFormBpmLte,
            [BeatmapAttribute.PreModBPMMaximum] = "Pre Mod " + ArtistStrings.TracksIndexFormBpmLte,
        }.ToImmutableDictionary();

        private readonly OsuSpriteText text;

        public BeatmapAttributeText()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = OsuFont.Default.With(size: 40)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Attribute.BindValueChanged(_ => updateLabel());
            Template.BindValueChanged(_ => updateLabel());
            ruleset.BindValueChanged(_ =>
            {
                //we only need to compute the starRating if we need it and there are mods, that could influence it.
                if (needsStarRating() && mods.Value.Count > 0) updateStarRating();
                if (needsPreStarRating() || needsStarRating() && mods.Value.Count == 0) updatePreStarRating();

                updateLabel();
            });
            mods.BindValueChanged(_ =>
            {
                modSettingChangeTracker?.Dispose();
                modSettingChangeTracker = new ModSettingChangeTracker(mods.Value);
                modSettingChangeTracker.SettingChanged += _ =>
                {
                    //we only need to compute the starRating if we need it and there are mods, that could influence it.
                    if (needsStarRating() && mods.Value.Count > 0) updateStarRating();
                    updateLabel();
                };

                //we only need to compute the starRating if we need it and there are mods, that could influence it.
                if (needsStarRating() && mods.Value.Count > 0) updateStarRating();
                if (needsPreStarRating() || needsStarRating() && mods.Value.Count == 0) updatePreStarRating();

                updateLabel();
            });
            workingBeatmap.BindValueChanged(_ =>
            {
                if (needsStarRating())
                {
                    if (mods.Value.Count > 0) updateStarRating();
                    else updatePreStarRating();
                }

                if (needsPreStarRating()) updatePreStarRating();
                updateLabel();
            }, true);
        }

        private void updateDifficulty()
        {
            difficulty = new BeatmapDifficulty(workingBeatmap.Value.BeatmapInfo.Difficulty);
            foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(difficulty);
        }

        private bool needsStarRating() => Template.Value.Contains($"{{{nameof(BeatmapAttribute.StarRating)}}}") || Template.Value.Contains("{Value}") && Attribute.Value == BeatmapAttribute.StarRating;
        private bool needsPreStarRating() => Template.Value.Contains($"{{{nameof(BeatmapAttribute.PreModStarRating)}}}") || Template.Value.Contains("{Value}") && Attribute.Value == BeatmapAttribute.PreModStarRating;

        private void updatePreStarRating()
        {
            preStarDifficultyCancellationSource?.Cancel();
            preStarRating = null;
            preStarDifficultyCancellationSource = new CancellationTokenSource();

            difficultyCache.GetDifficultyAsync(workingBeatmap.Value.BeatmapInfo, ruleset.Value, null, preStarDifficultyCancellationSource.Token).ContinueWith(starDifficultyTaskNoMods => Schedule(() =>
            {
                preStarRating = starDifficultyTaskNoMods.GetResultSafely();
                updateLabel();
            }), preStarDifficultyCancellationSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
        }

        private void updateStarRating()
        {
            starDifficultyCancellationSource?.Cancel();
            starDifficultyCancellationSource = new CancellationTokenSource();

            difficultyCache.GetDifficultyAsync(workingBeatmap.Value.BeatmapInfo, ruleset.Value, mods.Value, starDifficultyCancellationSource.Token).ContinueWith(starDifficultyTaskMods => Schedule(() =>
            {
                starRating = starDifficultyTaskMods.GetResultSafely();
                updateLabel();
            }), starDifficultyCancellationSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
        }

        private void updateLabel()
        {
            string numberedTemplate = Template.Value
                                              .Replace("{", "{{")
                                              .Replace("}", "}}")
                                              .Replace(@"{{Label}}", "{0}")
                                              .Replace(@"{{Value}}", "{{" + Attribute.Value + "}}")
                                              .Replace("{{" + nameof(BeatmapAttribute.PreModCircleSize) + "}}", workingBeatmap.Value.BeatmapInfo.Difficulty.CircleSize.ToString(@"F2"))
                                              .Replace("{{" + nameof(BeatmapAttribute.PreModHPDrain) + "}}", workingBeatmap.Value.BeatmapInfo.Difficulty.DrainRate.ToString(@"F2"))
                                              .Replace("{{" + nameof(BeatmapAttribute.PreModAccuracy) + "}}", workingBeatmap.Value.BeatmapInfo.Difficulty.OverallDifficulty.ToString(@"F2"))
                                              .Replace("{{" + nameof(BeatmapAttribute.PreModApproachRate) + "}}", workingBeatmap.Value.BeatmapInfo.Difficulty.ApproachRate.ToString(@"F2"))
                                              .Replace("{{" + nameof(BeatmapAttribute.Title) + "}}", workingBeatmap.Value.Metadata.Title)
                                              .Replace("{{" + nameof(BeatmapAttribute.Artist) + "}}", workingBeatmap.Value.Metadata.Artist)
                                              .Replace("{{" + nameof(BeatmapAttribute.DifficultyName) + "}}", workingBeatmap.Value.BeatmapInfo.DifficultyName)
                                              .Replace("{{" + nameof(BeatmapAttribute.Creator) + "}}", workingBeatmap.Value.Metadata.Author.Username);

            //conditionally insert CS, HP, OD, AR
            {
                string[] beatmapDiffStrings = { nameof(BeatmapAttribute.CircleSize), nameof(BeatmapAttribute.HPDrain), nameof(BeatmapAttribute.Accuracy), nameof(BeatmapAttribute.ApproachRate) };
                bool diffCompute = false;

                for (int i = 0; i < beatmapDiffStrings.Length; i++)
                {
                    diffCompute |= numberedTemplate.Contains("{{" + beatmapDiffStrings[i] + "}}");
                    if (diffCompute) break;
                }

                if (diffCompute)
                {
                    updateDifficulty();
                    numberedTemplate = numberedTemplate.Replace("{{" + nameof(BeatmapAttribute.CircleSize) + "}}", difficulty.CircleSize.ToString(@"F2"))
                                                       .Replace("{{" + nameof(BeatmapAttribute.HPDrain) + "}}", difficulty.DrainRate.ToString(@"F2"))
                                                       .Replace("{{" + nameof(BeatmapAttribute.Accuracy) + "}}", difficulty.OverallDifficulty.ToString(@"F2"))
                                                       .Replace("{{" + nameof(BeatmapAttribute.ApproachRate) + "}}", difficulty.ApproachRate.ToString(@"F2"));
                }
            }

            //Conditionally insert RankedStatus
            {
                string rankedStatus;

                if (numberedTemplate.Contains(rankedStatus = "{{" + nameof(BeatmapAttribute.RankedStatus) + "}}"))
                {
                    numberedTemplate = numberedTemplate.Replace(rankedStatus, workingBeatmap.Value.BeatmapInfo.Status.GetLocalisableDescription().ToString());
                }
            }

            if (needsStarRating())
            {
                numberedTemplate = numberedTemplate.Replace($"{{{{{nameof(BeatmapAttribute.StarRating)}}}}}",
                    (mods.Value.Count > 0 ? starRating : preStarRating)?.Stars.ToString(@"F2") ?? workingBeatmap.Value.BeatmapInfo.StarRating.ToString(@"F2"));
            }

            if (needsPreStarRating())
            {
                numberedTemplate = numberedTemplate.Replace($"{{{{{nameof(BeatmapAttribute.PreModStarRating)}}}}}",
                    preStarRating?.Stars.ToString(@"F2") ?? workingBeatmap.Value.BeatmapInfo.StarRating.ToString(@"F2"));
            }

            //conditionally insert BPM, BPMMinimum, BPMMaximum, Length
            {
                string[] beatmapDiffStrings = { nameof(BeatmapAttribute.BPMMinimum), nameof(BeatmapAttribute.BPM), nameof(BeatmapAttribute.BPMMaximum), nameof(BeatmapAttribute.Length) };
                bool bpmCompute = false;

                for (int i = 0; i < beatmapDiffStrings.Length; i++)
                {
                    bpmCompute |= numberedTemplate.Contains("{{" + beatmapDiffStrings[i] + "}}");
                    if (bpmCompute) break;
                }

                if (bpmCompute)
                {
                    var (bpmMin, bpm, bpmMax, length) = workingBeatmap.Value.Beatmap.GetBPMAndLength(mods.Value.OfType<ModRateAdjust>());
                    numberedTemplate = numberedTemplate.Replace("{{" + nameof(BeatmapAttribute.BPMMinimum) + "}}", bpmMin.ToString(@"N"))
                                                       .Replace("{{" + nameof(BeatmapAttribute.BPM) + "}}", bpm.ToString(@"N"))
                                                       .Replace("{{" + nameof(BeatmapAttribute.BPMMaximum) + "}}", bpmMax.ToString(@"N"))
                                                       .Replace("{{" + nameof(BeatmapAttribute.Length) + "}}", TimeSpan.FromMilliseconds(length).ToFormattedDuration().ToString());
                }
            }

            //conditionally insert PreModBPM, PreModBPMMinimum, PreModBPMMaximum, PreModLength
            {
                string[] beatmapDiffStrings = { nameof(BeatmapAttribute.PreModBPMMinimum), nameof(BeatmapAttribute.PreModBPM), nameof(BeatmapAttribute.PreModBPMMaximum), nameof(BeatmapAttribute.PreModLength) };
                bool bpmCompute = false;

                for (int i = 0; i < beatmapDiffStrings.Length; i++)
                {
                    bpmCompute |= numberedTemplate.Contains("{{" + beatmapDiffStrings[i] + "}}");
                    if (bpmCompute) break;
                }

                if (bpmCompute)
                {
                    var (bpmMin, bpm, bpmMax, length) = workingBeatmap.Value.Beatmap.GetBPMAndLength(Enumerable.Empty<IApplicableToRate>());
                    numberedTemplate = numberedTemplate.Replace("{{" + nameof(BeatmapAttribute.PreModBPMMinimum) + "}}", bpmMin.ToString(@"N"))
                                                       .Replace("{{" + nameof(BeatmapAttribute.PreModBPM) + "}}", bpm.ToString(@"N"))
                                                       .Replace("{{" + nameof(BeatmapAttribute.PreModBPMMaximum) + "}}", bpmMax.ToString(@"N"))
                                                       .Replace("{{" + nameof(BeatmapAttribute.PreModLength) + "}}", TimeSpan.FromMilliseconds(length).ToFormattedDuration().ToString());
                }
            }

            text.Text = LocalisableString.Format(numberedTemplate, label_dictionary[Attribute.Value]);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            starDifficultyCancellationSource?.Cancel();
            modSettingChangeTracker?.Dispose();
        }
    }

    public enum BeatmapAttribute
    {
        CircleSize,
        HPDrain,
        Accuracy,
        ApproachRate,
        StarRating,
        Title,
        Artist,
        DifficultyName,
        Creator,
        Length,
        RankedStatus,
        BPM,
        BPMMinimum,
        BPMMaximum,
        PreModCircleSize,
        PreModHPDrain,
        PreModAccuracy,
        PreModApproachRate,
        PreModStarRating,
        PreModLength,
        PreModBPM,
        PreModBPMMinimum,
        PreModBPMMaximum,
    }
}
