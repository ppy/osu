// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets;
using osu.Game.Utils;
using osu.Game.Scoring;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class BeatmapAttributeText : FontAdjustableSkinComponent
    {
        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Attribute), nameof(BeatmapAttributeTextStrings.AttributeDescription))]
        public Bindable<BeatmapAttribute> Attribute { get; } = new Bindable<BeatmapAttribute>(BeatmapAttribute.StarRating);

        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Template), nameof(BeatmapAttributeTextStrings.TemplateDescription))]
        public Bindable<string> Template { get; } = new Bindable<string>("{Label}: {Value}");

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
            [BeatmapAttribute.Source] = EditorSetupStrings.Source,
            [BeatmapAttribute.Length] = ArtistStrings.TracklistLength.ToTitle(),
            [BeatmapAttribute.RankedStatus] = BeatmapDiscussionsStrings.IndexFormBeatmapsetStatusDefault,
            [BeatmapAttribute.BPM] = BeatmapsetsStrings.ShowStatsBpm,
            [BeatmapAttribute.MaxPerformance] = "Max Performance"
        }.ToImmutableDictionary();

        private readonly OsuSpriteText text;

        public LocalisableString Text => text.Text;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private BeatmapInfo beatmapInfo => beatmap.Value.Beatmap.BeatmapInfo;

        private ModSettingChangeTracker? modSettingChangeTracker;

        public BeatmapAttributeText()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (var attribute in Enum.GetValues<BeatmapAttribute>())
                valueDictionary[attribute] = string.Empty;

            ruleset.BindValueChanged(_ => updateAll());

            Template.BindValueChanged(_ => updateLabel());
            Attribute.BindValueChanged(_ =>
            {
                updateBindableDifficulty();
                updateAll();
            });

            beatmap.BindValueChanged(_ =>
            {
                updateBindableDifficulty();
                updateAll();
            });

            mods.BindValueChanged(_ =>
            {
                modSettingChangeTracker?.Dispose();
                modSettingChangeTracker = new ModSettingChangeTracker(mods.Value);
                modSettingChangeTracker.SettingChanged += _ => updateAll();
                updateAll();
            }, true);
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

            foreach (var type in Enum.GetValues<BeatmapAttribute>())
            {
                numberedTemplate = numberedTemplate.Replace($"{{{{{type}}}}}", $"{{{1 + (int)type}}}");
            }

            text.Text = LocalisableString.Format(numberedTemplate, args);
        }

        protected override void SetFont(FontUsage font) => text.Font = font.With(size: 40);

        // This function will update only necessary info. DON'T update bindable difficulty to avoid excessive bindable creations.
        private void updateAll()
        {
            updateMetadata();
            updateBeatmapDifficultyAttributes();
            updateTimingData();
            updateLabel();
        }

        private void updateMetadata()
        {
            // Skip if not relevant attribute
            if (!((Attribute.Value >= BeatmapAttribute.Title && Attribute.Value <= BeatmapAttribute.Creator)
                || Attribute.Value == BeatmapAttribute.RankedStatus || Attribute.Value == BeatmapAttribute.Source))
                return;

            valueDictionary[BeatmapAttribute.Title] = new RomanisableString(beatmap.Value.BeatmapInfo.Metadata.TitleUnicode, beatmap.Value.BeatmapInfo.Metadata.Title);
            valueDictionary[BeatmapAttribute.Artist] = new RomanisableString(beatmap.Value.BeatmapInfo.Metadata.ArtistUnicode, beatmap.Value.BeatmapInfo.Metadata.Artist);
            valueDictionary[BeatmapAttribute.DifficultyName] = beatmap.Value.BeatmapInfo.DifficultyName;
            valueDictionary[BeatmapAttribute.Creator] = beatmap.Value.BeatmapInfo.Metadata.Author.Username;
            valueDictionary[BeatmapAttribute.Source] = beatmap.Value.BeatmapInfo.Metadata.Source;
            valueDictionary[BeatmapAttribute.RankedStatus] = beatmap.Value.BeatmapInfo.Status.GetLocalisableDescription();
        }

        private void updateBeatmapDifficultyAttributes()
        {
            // Skip if not relevant attribute
            if (!(Attribute.Value <= BeatmapAttribute.ApproachRate))
                return;

            double rate = ModUtils.CalculateRateWithMods(mods.Value);

            BeatmapDifficulty difficulty = new BeatmapDifficulty(beatmapInfo.Difficulty);

            foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(difficulty);

            difficulty = ruleset.Value.CreateInstance().GetRateAdjustedDisplayDifficulty(difficulty, rate);

            valueDictionary[BeatmapAttribute.CircleSize] = difficulty.CircleSize.ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.HPDrain] = difficulty.DrainRate.ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.Accuracy] = difficulty.OverallDifficulty.ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.ApproachRate] = difficulty.ApproachRate.ToLocalisableString(@"F2");
        }

        private void updateTimingData()
        {
            // Skip if not relevant attribute
            if (!(Attribute.Value == BeatmapAttribute.Length || Attribute.Value == BeatmapAttribute.BPM))
                return;

            double rate = ModUtils.CalculateRateWithMods(mods.Value);

            valueDictionary[BeatmapAttribute.Length] = TimeSpan.FromMilliseconds(beatmapInfo.Length / rate).ToFormattedDuration();
            valueDictionary[BeatmapAttribute.BPM] = FormatUtils.RoundBPM(beatmapInfo.BPM, rate).ToLocalisableString(@"F0");
        }

        #region diffcalc stuff

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private Bindable<StarDifficulty?>? bindableDifficulty;

        private CancellationTokenSource? cancellationTokenSource;

        private void updateBindableDifficulty()
        {
            cancellationTokenSource?.Cancel();
            bindableDifficulty?.UnbindEvents();

            // Skip if not relevant attribute
            if (!(Attribute.Value == BeatmapAttribute.StarRating || Attribute.Value == BeatmapAttribute.MaxPerformance))
                return;

            cancellationTokenSource = new CancellationTokenSource();

            bindableDifficulty = (Bindable<StarDifficulty?>)difficultyCache.GetBindableDifficulty(beatmapInfo, cancellationTokenSource.Token);
            bindableDifficulty.BindValueChanged(d =>
            {
                StarDifficulty difficulty = d.NewValue ?? new StarDifficulty();

                if (Attribute.Value == BeatmapAttribute.StarRating)
                {
                    valueDictionary[BeatmapAttribute.StarRating] = difficulty.Stars.ToLocalisableString(@"F2");
                    updateLabel();
                }
                else if (Attribute.Value == BeatmapAttribute.MaxPerformance)
                {
                    calculateMaxPerformance(difficulty.Attributes).ContinueWith(t =>
                    {
                        valueDictionary[BeatmapAttribute.MaxPerformance] = t.GetResultSafely().ToLocalisableString(@"0pp");
                        updateLabel();
                    }, cancellationTokenSource.Token);
                }
            });
        }

        private async Task<double> calculateMaxPerformance(DifficultyAttributes? difficultyAttributes)
        {
            if (difficultyAttributes == null || cancellationTokenSource == null)
                return 0;

            var performanceCalculator = ruleset.Value.CreateInstance().CreatePerformanceCalculator();

            if (performanceCalculator == null)
                return 0;

            IBeatmap playableBeatmap = beatmap.Value.GetPlayableBeatmap(ruleset.Value, mods.Value, cancellationTokenSource.Token);
            ScoreInfo perfectScore = ScoreUtils.GetPerfectPlay(playableBeatmap, ruleset.Value, mods.Value.ToArray());

            var performanceAttributes = await performanceCalculator.CalculateAsync(perfectScore, difficultyAttributes, cancellationTokenSource.Token).ConfigureAwait(false);

            return performanceAttributes.Total;
        }

        #endregion
    }

    // WARNING: DO NOT ADD ANY VALUES TO THIS ENUM ANYWHERE ELSE THAN AT THE END.
    // Doing so will break existing user skins.
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
        Source,
        MaxPerformance
    }
}
