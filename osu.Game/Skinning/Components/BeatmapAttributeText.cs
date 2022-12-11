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

        [SettingSource("Adjust Difficulty", "Should Mods that change Beatmap Statistics be applied to the shown Stats?")]
        public BindableBool DifficultyAdjust { get; set; } = new BindableBool(true);

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

        private BeatmapDifficulty difficulty = null!;

        private readonly Dictionary<BeatmapAttribute, LocalisableString> valueDictionary = new Dictionary<BeatmapAttribute, LocalisableString>();

        private static readonly ImmutableDictionary<BeatmapAttribute, LocalisableString> label_dictionary = new Dictionary<BeatmapAttribute, LocalisableString>
        {
            [BeatmapAttribute.CircleSize] = BeatmapsetsStrings.ShowStatsCs,
            [BeatmapAttribute.Accuracy] = BeatmapsetsStrings.ShowStatsAccuracy,
            [BeatmapAttribute.HPDrain] = BeatmapsetsStrings.ShowStatsDrain,
            [BeatmapAttribute.ApproachRate] = BeatmapsetsStrings.ShowStatsAr,
            [BeatmapAttribute.StarRating] = BeatmapsetsStrings.ShowStatsStars,
            [BeatmapAttribute.Title] = EditorSetupStrings.Title,
            [BeatmapAttribute.Artist] = EditorSetupStrings.Artist,
            [BeatmapAttribute.DifficultyName] = EditorSetupStrings.DifficultyHeader,
            [BeatmapAttribute.Creator] = EditorSetupStrings.Creator,
            [BeatmapAttribute.Length] = ArtistStrings.TracklistLength.ToTitle(),
            [BeatmapAttribute.RankedStatus] = BeatmapDiscussionsStrings.IndexFormBeatmapsetStatusDefault,
            [BeatmapAttribute.BPM] = BeatmapsetsStrings.ShowStatsBpm,
            [BeatmapAttribute.BPMMinimum] = ArtistStrings.TracksIndexFormBpmGte,
            [BeatmapAttribute.BPMMaximum] = ArtistStrings.TracksIndexFormBpmLte,
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
                updateAllInfo();
                //custom rulesets might not provide all of the same mods, and in that case we do need to update all info
            });
            mods.BindValueChanged(_ =>
            {
                modSettingChangeTracker?.Dispose();
                Action modsSettingsChangedAction = () =>
                {
                    //if we only dispaly base stats, mods are ignored anyways.
                    //so then we can skip checking for a StarRating Update
                    if (DifficultyAdjust.Value) updateStarRating();
                    updateBpmAndLength();
                    updateBeatmapContent();
                    updateLabel();
                };

                modSettingChangeTracker = new ModSettingChangeTracker(mods.Value);
                modSettingChangeTracker.SettingChanged += _ => modsSettingsChangedAction();
                modsSettingsChangedAction();
            });
            DifficultyAdjust.BindValueChanged(_ =>
            {
                updateAllInfo();
            });
            workingBeatmap.BindValueChanged(_ =>
            {
                updateAllInfo();
            }, true);
        }

        private void updateAllInfo()
        {
            updateStarRating();
            updateBpmAndLength();
            updateBeatmapContent();
            updateLabel();
        }

        private void updateDifficulty()
        {
            difficulty = new BeatmapDifficulty(workingBeatmap.Value.BeatmapInfo.Difficulty);

            if (!DifficultyAdjust.Value) return;

            foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(difficulty);
        }

        private void updateStarRating()
        {
            starDifficultyCancellationSource?.Cancel();
            starDifficultyCancellationSource = new CancellationTokenSource();

            var starDifficultyTask = DifficultyAdjust.Value
                ? difficultyCache.GetDifficultyAsync(workingBeatmap.Value.BeatmapInfo, ruleset.Value, mods.Value, starDifficultyCancellationSource.Token)
                : difficultyCache.GetDifficultyAsync(workingBeatmap.Value.BeatmapInfo, ruleset.Value, null, starDifficultyCancellationSource.Token);
            starDifficultyTask.ContinueWith(_ => Schedule(() =>
            {
                starRating = starDifficultyTask.GetResultSafely();
                valueDictionary[BeatmapAttribute.StarRating] = (starRating?.Stars ?? -1).ToLocalisableString(@"F2");
                updateLabel();
            }), starDifficultyCancellationSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
        }

        private void updateBpmAndLength()
        {
            var (bpmMax, mostCommonBPM, bpmMin, length) = workingBeatmap.Value.Beatmap.GetBPMAndLength(mods.Value.OfType<ModRateAdjust>());

            valueDictionary[BeatmapAttribute.BPMMaximum] = bpmMax.ToLocalisableString(@"F0");
            valueDictionary[BeatmapAttribute.BPMMinimum] = mostCommonBPM.ToLocalisableString(@"F0");
            valueDictionary[BeatmapAttribute.BPM] = bpmMin.ToLocalisableString(@"F0");
            valueDictionary[BeatmapAttribute.Length] = TimeSpan.FromMilliseconds(length).ToFormattedDuration();
        }

        private void updateBeatmapContent()
        {
            updateDifficulty();
            var beatmapInfo = workingBeatmap.Value.BeatmapInfo;
            valueDictionary[BeatmapAttribute.Title] = beatmapInfo.Metadata.Title;
            valueDictionary[BeatmapAttribute.Artist] = beatmapInfo.Metadata.Artist;
            valueDictionary[BeatmapAttribute.DifficultyName] = beatmapInfo.DifficultyName;
            valueDictionary[BeatmapAttribute.Creator] = beatmapInfo.Metadata.Author.Username;
            valueDictionary[BeatmapAttribute.RankedStatus] = beatmapInfo.Status.GetLocalisableDescription();
            valueDictionary[BeatmapAttribute.CircleSize] = ((double)difficulty.CircleSize).ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.HPDrain] = ((double)difficulty.DrainRate).ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.Accuracy] = ((double)difficulty.OverallDifficulty).ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.ApproachRate] = ((double)difficulty.ApproachRate).ToLocalisableString(@"F2");
        }

        private void updateLabel()
        {
            string numberedTemplate = Template.Value
                                              .Replace("{", "{{")
                                              .Replace("}", "}}")
                                              .Replace(@"{{Label}}", "{0}")
                                              .Replace(@"{{Value}}", $"{{{1 + (int)Attribute.Value}}}");

            object?[] args = valueDictionary.OrderBy(pair => pair.Key)
                                            .Select(pair => pair.Value)
                                            .Prepend(label_dictionary[Attribute.Value])
                                            .Cast<object?>()
                                            .ToArray();

            foreach (var type in Enum.GetValues(typeof(BeatmapAttribute)).Cast<BeatmapAttribute>())
            {
                numberedTemplate = numberedTemplate.Replace($"{{{{{type}}}}}", $"{{{1 + (int)type}}}");
            }

            text.Text = LocalisableString.Format(numberedTemplate, args);
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
    }
}
