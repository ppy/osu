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

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        //idea: make this static, and also have a static instance of BeatmapAttributeText, that is allowed to modify the values.
        //That would be the same as a cache, but without a actual [Resolved] and [Cached] attributes.
        private static readonly Dictionary<BeatmapAttribute, Lazy<Task<LocalisableString>>> values = new Dictionary<BeatmapAttribute, Lazy<Task<LocalisableString>>>();

        private static event Action<List<BeatmapAttribute>> valuesChanged = _ => { };

        //If this this stays null, even after ruleset, mods or beatmap changed, then there is no more BeatMapAttributeText in the scene.
        //Once a new BeatmapAttributeText is added, the values will be updated because of the workingBeatmap.BindValueChanged running instantly.
        //If this is null, and no ruleset, mod or beatmap update happens, then the values might not be updated.
        private static BeatmapAttributeText? modifyingInstance;

        //if modifyingInstance is the last Instance in the scene
        private static bool lastInstance = true;
        private static ModSettingChangeTracker? modSettingChangeTracker;
        private static CancellationTokenSource? preStarDifficultyCancellationSource;

        //This lock is ONLY for reading/writing modfifyingInstance.
        //All of the other mutable static members can be written to by modifyingInstance.
        //modSettingChangeTracker and preStarDifficultyCancellationSource should only need to be read by modfiyingInstance.
        //values can be read by anyone. Changes are announced via valuesChanged.
        private static readonly object modify_lock = new object();

        private BeatmapAttributeText modifyingInstanceValue
        {
            get
            {
                //This should never block, because all updated are currently called sequentially afaik.
                lock (modify_lock)
                {
                    if (modifyingInstance is null)
                    {
                        modifyingInstance = this;
                        //truth is, we don't know this for certain.
                        //But it's better to be pessimistic here, because this is the first instance (of this class), that got updated (or otherwise modifyingInstance would already have been non-null).
                        //If no more instances exist, then this guess is right.
                        lastInstance = true;
                    }
                    else
                    {
                        //If more instances (of this class) exist they will get updated after this one, and set lastInstance to false again (the line below).
                        lastInstance = false;
                    }

                    return modifyingInstance;
                }
            }
        }

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
            valuesChanged += updateLabelIfNessesary;

            Attribute.BindValueChanged(_ => updateLabel());
            Template.BindValueChanged(_ => updateLabel());
            ruleset.BindValueChanged(_ =>
            {
                if (modifyingInstanceValue != this) return;

                //we only need to compute the starRating if we need it and there are mods, that could influence it.
                updatePreStarRating();
                //We do update the label here, so that if PreModStarRating is ever used, the task to calculate it get's started.
                valuesChanged(new List<BeatmapAttribute> { BeatmapAttribute.PreModStarRating });
            });
            Action modAction = () =>
            {
                //This is async, because who knows how long or not mod application can take.
                Lazy<Task<BeatmapDifficulty>> difficulty = new Lazy<Task<BeatmapDifficulty>>(() => Task.Run(updateDifficulty));
                values[BeatmapAttribute.CircleSize] = new Lazy<Task<LocalisableString>>(async () => (await difficulty.Value.ConfigureAwait(false)).CircleSize.ToLocalisableString(@"F2"));
                values[BeatmapAttribute.HPDrain] = new Lazy<Task<LocalisableString>>(async () => (await difficulty.Value.ConfigureAwait(false)).DrainRate.ToLocalisableString(@"F2"));
                values[BeatmapAttribute.Accuracy] = new Lazy<Task<LocalisableString>>(async () => (await difficulty.Value.ConfigureAwait(false)).OverallDifficulty.ToLocalisableString(@"F2"));
                values[BeatmapAttribute.ApproachRate] = new Lazy<Task<LocalisableString>>(async () => (await difficulty.Value.ConfigureAwait(false)).ApproachRate.ToLocalisableString(@"F2"));
                //This should also be simple. Overall we don't do much, since we are not applying any mods.
                Lazy<Task<(int, int, int, double)>> bpmAndLength = new Lazy<Task<(int, int, int, double)>>(() => Task.Run(() => workingBeatmap.Value.Beatmap.GetBPMAndLength(mods.Value.OfType<ModRateAdjust>())));
                values[BeatmapAttribute.BPMMinimum] = new Lazy<Task<LocalisableString>>(async () => (await bpmAndLength.Value.ConfigureAwait(false)).Item1.ToLocalisableString(@"N"));
                values[BeatmapAttribute.BPM] = new Lazy<Task<LocalisableString>>(async () => (await bpmAndLength.Value.ConfigureAwait(false)).Item2.ToLocalisableString(@"N"));
                values[BeatmapAttribute.BPMMaximum] = new Lazy<Task<LocalisableString>>(async () => (await bpmAndLength.Value.ConfigureAwait(false)).Item3.ToLocalisableString(@"N"));
                values[BeatmapAttribute.Length] = new Lazy<Task<LocalisableString>>(async () => TimeSpan.FromMilliseconds((await bpmAndLength.Value.ConfigureAwait(false)).Item4).ToFormattedDuration());
            };
            mods.BindValueChanged(_ =>
            {
                if (modifyingInstanceValue != this) return;

                modSettingChangeTracker?.Dispose();
                modSettingChangeTracker = new ModSettingChangeTracker(mods.Value);
                modSettingChangeTracker.SettingChanged += _ =>
                {
                    //we have already established here, that we are the modifying Instance, because the modifying Instance only get's changed, when the old value got disposed.
                    //If the old value got disposed, we also dispose the modSettingChangeTracker.
                    modAction();
                    //We do update the label here, so that if CS, HP, OD, or AR is ever used, the task to calculate it get's started.
                    valuesChanged(new List<BeatmapAttribute>
                    {
                        BeatmapAttribute.CircleSize,
                        BeatmapAttribute.HPDrain,
                        BeatmapAttribute.Accuracy,
                        BeatmapAttribute.ApproachRate,
                        BeatmapAttribute.BPMMinimum,
                        BeatmapAttribute.BPM,
                        BeatmapAttribute.BPMMaximum,
                        BeatmapAttribute.Length
                    });
                };

                modAction();
                //see above
                valuesChanged(new List<BeatmapAttribute>
                {
                    BeatmapAttribute.CircleSize,
                    BeatmapAttribute.HPDrain,
                    BeatmapAttribute.Accuracy,
                    BeatmapAttribute.ApproachRate,
                    BeatmapAttribute.BPMMinimum,
                    BeatmapAttribute.BPM,
                    BeatmapAttribute.BPMMaximum,
                    BeatmapAttribute.Length
                });
            });
            Lazy<Task<IBindable<StarDifficulty?>>>? starRating = null;
            workingBeatmap.BindValueChanged(_ =>
            {
                if (modifyingInstanceValue != this) return;

                if (starRating is not null && starRating.IsValueCreated && starRating.Value.IsCompleted)
                    starRating.Value.GetResultSafely().UnbindAll();
                starRating = new Lazy<Task<IBindable<StarDifficulty?>>>(() => Task.Run(() => difficultyCache.GetBindableDifficulty(workingBeatmap.Value.BeatmapInfo)));
                //populates CS, HP, OD, AR
                modAction();

                values[BeatmapAttribute.StarRating] = new Lazy<Task<LocalisableString>>(async () =>
                {
                    IBindable<StarDifficulty?> starDifficulty;

                    if (!starRating.Value.IsCompleted)
                    {
                        starDifficulty = await starRating.Value.ConfigureAwait(false);
                        starDifficulty.BindValueChanged(_ =>
                        {
                            valuesChanged(new List<BeatmapAttribute> { BeatmapAttribute.StarRating });
                            values[BeatmapAttribute.StarRating] = new Lazy<Task<LocalisableString>>(
                                () => Task.FromResult(starDifficulty.Value?.Stars.ToLocalisableString(@"F2") ?? workingBeatmap.Value.BeatmapInfo.StarRating.ToLocalisableString(@"F2"))
                            );
                        });
                    }
                    else starDifficulty = starRating.Value.GetResultSafely();

                    //Whilst the starRating is not calculated yet, we just use the default StarDifficulty from the beatmap (which does have a default).
                    return starDifficulty.Value?.Stars.ToLocalisableString(@"F2") ?? workingBeatmap.Value.BeatmapInfo.StarRating.ToLocalisableString(@"F2");
                });

                //These are all cheap. I will not asyncify them.
                values[BeatmapAttribute.Title] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(new LocalisableString(workingBeatmap.Value.Metadata.Title)));
                values[BeatmapAttribute.Artist] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(new LocalisableString(workingBeatmap.Value.Metadata.Artist)));
                values[BeatmapAttribute.DifficultyName] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(new LocalisableString(workingBeatmap.Value.BeatmapInfo.DifficultyName)));
                values[BeatmapAttribute.Creator] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(new LocalisableString(workingBeatmap.Value.Metadata.Author.Username)));
                values[BeatmapAttribute.RankedStatus] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(workingBeatmap.Value.BeatmapInfo.Status.GetLocalisableDescription()));
                values[BeatmapAttribute.PreModCircleSize] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(workingBeatmap.Value.BeatmapInfo.Difficulty.CircleSize.ToLocalisableString(@"F2")));
                values[BeatmapAttribute.PreModHPDrain] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(workingBeatmap.Value.BeatmapInfo.Difficulty.DrainRate.ToLocalisableString(@"F2")));
                values[BeatmapAttribute.PreModAccuracy] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(workingBeatmap.Value.BeatmapInfo.Difficulty.OverallDifficulty.ToLocalisableString(@"F2")));
                values[BeatmapAttribute.PreModApproachRate] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(workingBeatmap.Value.BeatmapInfo.Difficulty.ApproachRate.ToLocalisableString(@"F2")));

                updatePreStarRating();
                //This should also be simple. Overall we don't do much, since we are not applying any mods.
                Lazy<(int, int, int, double)> bpmAndLength = new Lazy<(int, int, int, double)>(() => workingBeatmap.Value.Beatmap.GetBPMAndLength(Enumerable.Empty<IApplicableToRate>()));
                values[BeatmapAttribute.PreModBPMMinimum] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(bpmAndLength.Value.Item1.ToLocalisableString(@"N")));
                values[BeatmapAttribute.PreModBPM] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(bpmAndLength.Value.Item2.ToLocalisableString(@"N")));
                values[BeatmapAttribute.PreModBPMMaximum] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(bpmAndLength.Value.Item3.ToLocalisableString(@"N")));
                values[BeatmapAttribute.PreModLength] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(TimeSpan.FromMilliseconds(bpmAndLength.Value.Item4).ToFormattedDuration()));

                valuesChanged(new List<BeatmapAttribute>
                {
                    BeatmapAttribute.CircleSize,
                    BeatmapAttribute.HPDrain,
                    BeatmapAttribute.Accuracy,
                    BeatmapAttribute.ApproachRate,
                    BeatmapAttribute.BPMMinimum,
                    BeatmapAttribute.BPM,
                    BeatmapAttribute.BPMMaximum,
                    BeatmapAttribute.Length,
                    BeatmapAttribute.StarRating,
                    BeatmapAttribute.Title,
                    BeatmapAttribute.Artist,
                    BeatmapAttribute.DifficultyName,
                    BeatmapAttribute.Creator,
                    BeatmapAttribute.RankedStatus,
                    BeatmapAttribute.PreModCircleSize,
                    BeatmapAttribute.PreModHPDrain,
                    BeatmapAttribute.PreModAccuracy,
                    BeatmapAttribute.PreModApproachRate,
                    BeatmapAttribute.PreModStarRating,
                    BeatmapAttribute.PreModBPMMinimum,
                    BeatmapAttribute.PreModBPM,
                    BeatmapAttribute.PreModBPMMaximum,
                    BeatmapAttribute.PreModLength,
                });
            }, true);
            Schedule(updateLabel);
        }

        private void updateLabelIfNessesary(List<BeatmapAttribute> list)
        {
            foreach (var entry in list)
            {
                if ((entry == Attribute.Value && Template.Value.Contains(@"{Value}")) || Template.Value.Contains($"{{{entry}}}"))
                {
                    Schedule(updateLabel);
                    return;
                }
            }
        }

        private BeatmapDifficulty updateDifficulty()
        {
            BeatmapDifficulty difficulty = new BeatmapDifficulty(workingBeatmap.Value.BeatmapInfo.Difficulty);
            foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(difficulty);
            return difficulty;
        }

        private void updatePreStarRating()
        {
            //We cancel immediately, because something changed that might make the previous calculation incorrect.
            //We just don't start the new calculation though.
            preStarDifficultyCancellationSource?.Cancel();
            preStarDifficultyCancellationSource = null;

            async Task<LocalisableString> updatePreStarRatingAsync()
            {
                preStarDifficultyCancellationSource = new CancellationTokenSource();
                StarDifficulty? preStarRating = await difficultyCache.GetDifficultyAsync(workingBeatmap.Value.BeatmapInfo, ruleset.Value, null, preStarDifficultyCancellationSource.Token).ConfigureAwait(false);
                //Whilst the starRating is not calculated yet, we just use the default StarDifficulty from the beatmap (which does have a default).
                LocalisableString preStarRatingString = preStarRating?.Stars.ToLocalisableString(@"F2") ?? workingBeatmap.Value.BeatmapInfo.StarRating.ToLocalisableString(@"F2");
                Schedule(updateLabel);
                return preStarRatingString;
            }

            values[BeatmapAttribute.PreModStarRating] = new Lazy<Task<LocalisableString>>(updatePreStarRatingAsync());
        }

        private void updateLabel()
        {
            string numberedTemplate = Template.Value
                                              .Replace("{", "{{")
                                              .Replace("}", "}}")
                                              .Replace(@"{{Label}}", "{0}")
                                              .Replace(@"{{Value}}", $"{{{1 + (int)Attribute.Value}}}"); // +1 because the first argument is the label

            var enumValues = Enum.GetValues(typeof(BeatmapAttribute));

            foreach (var type in enumValues.Cast<BeatmapAttribute>())
            {
                numberedTemplate = numberedTemplate.Replace($"{{{{{type}}}}}", $"{{{1 + (int)type}}}");
            }

            IEnumerable<Lazy<Task<LocalisableString>>> args = values.OrderBy(pair => pair.Key)
                                                                    .Select(pair => pair.Value);

            LocalisableString?[] argsArray = new LocalisableString?[enumValues.Length + 1];

            {
                //This would normally be wrong, but I specifically reserve argsArray[0] for the label.
                int i = 1;
                var test = args.GetEnumerator();

                while (test.MoveNext())
                {
                    if (numberedTemplate.Contains($"{{{i}}}"))
                    {
                        Task<LocalisableString> task = test.Current.Value;

                        //we don't want to wait on the update thread.
                        if (task.IsCompleted)
                            argsArray[i] = task.GetResultSafely();
                        else
                        {
                            task.ContinueWithSequential(() => Schedule(updateLabel));
                            argsArray[i] = "Computing...";
                        }
                    }
                    else argsArray[i] = null;

                    i++;
                }

                test.Dispose();
            }
            argsArray[0] = label_dictionary[Attribute.Value];

            text.Text = LocalisableString.Format(numberedTemplate, argsArray.Cast<object?>().ToArray());
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            valuesChanged -= updateLabelIfNessesary;

            if (modifyingInstanceValue == this)
            {
                //we do not dispose the modSettingChangeTracker here, so that mod changes are still tracked.
                //otherwise, if you deleted the modifingInstance, mod setting changes would not propagate to CS, AR, HP, OD, the different BPM values and Length.
                if (lastInstance)
                {
                    modSettingChangeTracker?.Dispose();
                    modSettingChangeTracker = null;
                    //we might want to also dispose 'values' here, if we are really concerned about memory usage.
                    //I will right now not do so, because I don't want to deal with a nullable dictionary type on values.
                    //e.g. values could be null, but would then instantly get a value again, when a new instance is created.
                    //That values pretty much has a value most of the time cannot be checked at compile time though.
                    //So either we wrongly declare values then as not nullable, but set it to null in this case anyways (idk if that is possible)
                    //or we ignore the nullability everytime, when we use values (which looks worse for code style imo).
                }

                //we do take care of the preStarDifficultyCancellationSource though, because if we didn't that might lead to unwanted & unneeded calculations.
                //especially if the modifyingInstance was the last BeatmapAttributeDisplay.
                preStarDifficultyCancellationSource?.Cancel();
                preStarDifficultyCancellationSource?.Dispose();
                preStarDifficultyCancellationSource = null;
                modifyingInstance = null;
            }
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
