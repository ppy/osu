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
using osu.Game.Skinning.Editor;

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

        private static Dictionary<BeatmapAttribute, Lazy<Task<LocalisableString>>>? values;

        //should only be accessed by modifyingInstance.
        private static Dictionary<BeatmapAttribute, Lazy<Task<LocalisableString>>> getValue => values ??= new Dictionary<BeatmapAttribute, Lazy<Task<LocalisableString>>>();

        //should only be invoked by modifyingInstance. Event handlers can be added/removed by anyone though.
        private static event Action<List<BeatmapAttribute>> valuesChanged = _ => { };

        //If this this stays null, even after ruleset, mods or beatmap changed, then there is no more BeatMapAttributeText in the scene.
        //Once a new BeatmapAttributeText is added, the values will be updated because of the workingBeatmap.BindValueChanged running instantly.
        //If this is null, and no ruleset, mod or beatmap update happens, then the values might not be updated.
        private static BeatmapAttributeText? modifyingInstance;

        //The number of instances, that listen to changes.
        //should only be accessed or modified atomically.
        //The programm counts on the correctness of the value in the Dispose method.
        private static long instances = 0;
        private bool enabled = true;

        //should only be accessed or modified by modifyingInstance.
        private static ModSettingChangeTracker? modSettingChangeTracker;

        //should only be accessed or modified by modifyingInstance.
        private static CancellationTokenSource? preStarDifficultyCancellationSource;

        private BeatmapAttributeText? modifyingInstanceValue
        {
            get
            {
                if (!enabled) return null;

                if (modifyingInstance is null)
                {
                    Interlocked.CompareExchange(ref modifyingInstance, this, null);
                }

                return modifyingInstance;
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
            Lazy<Task<IBindable<StarDifficulty?>>>? starRating = null;

            //our parents should be defined here, since dependencies are only loaded once we are added to the scene graph.
            //Also we need to explicitly exclude the preview instance, because otherwise we never have a "last" instance, and `values`,`modSettingChangeTracker` and `preStarDifficultyCancellationSource` will never be deassigned.
            if (this.FindClosestParent<SkinEditor>() is not null) enabled = false;
            else Interlocked.Increment(ref instances); //increment atomically, if we are actually a user skin element.

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
                getValue[BeatmapAttribute.CircleSize] = new Lazy<Task<LocalisableString>>(async () => (await difficulty.Value.ConfigureAwait(false)).CircleSize.ToLocalisableString(@"F2"));
                getValue[BeatmapAttribute.HPDrain] = new Lazy<Task<LocalisableString>>(async () => (await difficulty.Value.ConfigureAwait(false)).DrainRate.ToLocalisableString(@"F2"));
                getValue[BeatmapAttribute.Accuracy] = new Lazy<Task<LocalisableString>>(async () => (await difficulty.Value.ConfigureAwait(false)).OverallDifficulty.ToLocalisableString(@"F2"));
                getValue[BeatmapAttribute.ApproachRate] = new Lazy<Task<LocalisableString>>(async () => (await difficulty.Value.ConfigureAwait(false)).ApproachRate.ToLocalisableString(@"F2"));
                //This should also be simple. Overall we don't do much, since we are not applying any mods.
                Lazy<Task<(int, int, int, double)>> bpmAndLength = new Lazy<Task<(int, int, int, double)>>(() => Task.Run(() => workingBeatmap.Value.Beatmap.GetBPMAndLength(mods.Value.OfType<ModRateAdjust>())));
                getValue[BeatmapAttribute.BPMMinimum] = new Lazy<Task<LocalisableString>>(async () => (await bpmAndLength.Value.ConfigureAwait(false)).Item1.ToLocalisableString(@"N"));
                getValue[BeatmapAttribute.BPM] = new Lazy<Task<LocalisableString>>(async () => (await bpmAndLength.Value.ConfigureAwait(false)).Item2.ToLocalisableString(@"N"));
                getValue[BeatmapAttribute.BPMMaximum] = new Lazy<Task<LocalisableString>>(async () => (await bpmAndLength.Value.ConfigureAwait(false)).Item3.ToLocalisableString(@"N"));
                getValue[BeatmapAttribute.Length] = new Lazy<Task<LocalisableString>>(async () => TimeSpan.FromMilliseconds((await bpmAndLength.Value.ConfigureAwait(false)).Item4).ToFormattedDuration());
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
            workingBeatmap.BindValueChanged(_ =>
            {
                if (modifyingInstanceValue != this) return;

                if (starRating is not null && starRating.IsValueCreated && starRating.Value.IsCompleted)
                    starRating.Value.GetResultSafely().UnbindAll();
                starRating = new Lazy<Task<IBindable<StarDifficulty?>>>(() => Task.Run(() => difficultyCache.GetBindableDifficulty(workingBeatmap.Value.BeatmapInfo)));
                //populates CS, HP, OD, AR
                modAction();

                getValue[BeatmapAttribute.StarRating] = new Lazy<Task<LocalisableString>>(async () =>
                {
                    IBindable<StarDifficulty?> starDifficulty;

                    if (!starRating.Value.IsCompleted)
                    {
                        starDifficulty = await starRating.Value.ConfigureAwait(false);
                        starDifficulty.BindValueChanged(_ =>
                        {
                            valuesChanged(new List<BeatmapAttribute> { BeatmapAttribute.StarRating });
                            getValue[BeatmapAttribute.StarRating] = new Lazy<Task<LocalisableString>>(
                                () => Task.FromResult(starDifficulty.Value?.Stars.ToLocalisableString(@"F2") ?? workingBeatmap.Value.BeatmapInfo.StarRating.ToLocalisableString(@"F2"))
                            );
                        });
                    }
                    else starDifficulty = starRating.Value.GetResultSafely();

                    //Whilst the starRating is not calculated yet, we just use the default StarDifficulty from the beatmap (which does have a default).
                    return starDifficulty.Value?.Stars.ToLocalisableString(@"F2") ?? workingBeatmap.Value.BeatmapInfo.StarRating.ToLocalisableString(@"F2");
                });

                //These are all cheap. I will not asyncify them.
                getValue[BeatmapAttribute.Title] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(new LocalisableString(workingBeatmap.Value.Metadata.Title)));
                getValue[BeatmapAttribute.Artist] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(new LocalisableString(workingBeatmap.Value.Metadata.Artist)));
                getValue[BeatmapAttribute.DifficultyName] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(new LocalisableString(workingBeatmap.Value.BeatmapInfo.DifficultyName)));
                getValue[BeatmapAttribute.Creator] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(new LocalisableString(workingBeatmap.Value.Metadata.Author.Username)));
                getValue[BeatmapAttribute.RankedStatus] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(workingBeatmap.Value.BeatmapInfo.Status.GetLocalisableDescription()));
                getValue[BeatmapAttribute.PreModCircleSize] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(workingBeatmap.Value.BeatmapInfo.Difficulty.CircleSize.ToLocalisableString(@"F2")));
                getValue[BeatmapAttribute.PreModHPDrain] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(workingBeatmap.Value.BeatmapInfo.Difficulty.DrainRate.ToLocalisableString(@"F2")));
                getValue[BeatmapAttribute.PreModAccuracy] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(workingBeatmap.Value.BeatmapInfo.Difficulty.OverallDifficulty.ToLocalisableString(@"F2")));
                getValue[BeatmapAttribute.PreModApproachRate] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(workingBeatmap.Value.BeatmapInfo.Difficulty.ApproachRate.ToLocalisableString(@"F2")));

                updatePreStarRating();
                //This should also be simple. Overall we don't do much, since we are not applying any mods.
                Lazy<(int, int, int, double)> bpmAndLength = new Lazy<(int, int, int, double)>(() => workingBeatmap.Value.Beatmap.GetBPMAndLength(Enumerable.Empty<IApplicableToRate>()));
                getValue[BeatmapAttribute.PreModBPMMinimum] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(bpmAndLength.Value.Item1.ToLocalisableString(@"N")));
                getValue[BeatmapAttribute.PreModBPM] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(bpmAndLength.Value.Item2.ToLocalisableString(@"N")));
                getValue[BeatmapAttribute.PreModBPMMaximum] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(bpmAndLength.Value.Item3.ToLocalisableString(@"N")));
                getValue[BeatmapAttribute.PreModLength] = new Lazy<Task<LocalisableString>>(() => Task.FromResult(TimeSpan.FromMilliseconds(bpmAndLength.Value.Item4).ToFormattedDuration()));

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

            getValue[BeatmapAttribute.PreModStarRating] = new Lazy<Task<LocalisableString>>(updatePreStarRatingAsync());
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

            IEnumerable<Lazy<Task<LocalisableString>>> args = values?.OrderBy(pair => pair.Key)
                                                                    .Select(pair => pair.Value)
                                                              ?? Enumerable.Empty<Lazy<Task<LocalisableString>>>();

            LocalisableString?[] argsArray = new LocalisableString?[enumValues.Length + 1];

            {
                //This would normally be wrong, but I specifically reserve argsArray[0] for the label.
                var test = args.GetEnumerator();

                for (int i = 1; i < enumValues.Length + 1; i++)
                {
                    bool isContained = numberedTemplate.Contains($"{{{i}}}");

                    if (test is not null && test.MoveNext())
                    {
                        if (isContained)
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
                    }
                    else
                    {
                        if (test is not null)
                        {
                            test.Dispose();
                            test = null;
                        }

                        if (isContained) argsArray[i] = "null";
                        else argsArray[i] = null;
                    }
                }
            }
            argsArray[0] = label_dictionary[Attribute.Value];

            text.Text = LocalisableString.Format(numberedTemplate, argsArray.Cast<object?>().ToArray());
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            valuesChanged -= updateLabelIfNessesary;
            Interlocked.Decrement(ref instances);

            if (modifyingInstanceValue == this)
            {
                //we do cleanup, if we are the last instance.
                if (Interlocked.Read(ref instances) <= 0)
                {
                    //if instances < 0, then something went wrong.
                    //do we log that somewhere?
                    modSettingChangeTracker?.Dispose();
                    modSettingChangeTracker = null;
                    //we dispose 'values' here, to save memory usage.
                    //We don't need the `values` Dictionary right now, because there will not be a instance after this is disposed.
                    values = null;
                    //update the disabled Skinnable instances.
                    valuesChanged?.Invoke(Enum.GetValues<BeatmapAttribute>().ToList());
                    //only take care of the preStarDifficultyCancellationSource if there are no more instances, because only then are the calculations really unnessesary and unwanted.
                    //if there are instances left, and something changes another instance will take care of disposing the cancellation source (like nothing even happened).
                    preStarDifficultyCancellationSource?.Cancel();
                    preStarDifficultyCancellationSource?.Dispose();
                    preStarDifficultyCancellationSource = null;
                }

                //we do not need a lock here. we are the ModifyingInstance, and can relieve ourselves
                Interlocked.Exchange(ref modifyingInstance, null);
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
