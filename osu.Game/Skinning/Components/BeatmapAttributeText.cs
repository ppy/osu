// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

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
            beatmap.BindValueChanged(b =>
            {
                updateBeatmapContent(b.NewValue);
                updateLabel();
            }, true);
        }

        private void updateBeatmapContent(WorkingBeatmap workingBeatmap)
        {
            valueDictionary[BeatmapAttribute.Title] = workingBeatmap.BeatmapInfo.Metadata.Title;
            valueDictionary[BeatmapAttribute.Artist] = workingBeatmap.BeatmapInfo.Metadata.Artist;
            valueDictionary[BeatmapAttribute.DifficultyName] = workingBeatmap.BeatmapInfo.DifficultyName;
            valueDictionary[BeatmapAttribute.Creator] = workingBeatmap.BeatmapInfo.Metadata.Author.Username;
            valueDictionary[BeatmapAttribute.Length] = TimeSpan.FromMilliseconds(workingBeatmap.BeatmapInfo.Length).ToFormattedDuration();
            valueDictionary[BeatmapAttribute.RankedStatus] = workingBeatmap.BeatmapInfo.Status.GetLocalisableDescription();
            valueDictionary[BeatmapAttribute.BPM] = workingBeatmap.BeatmapInfo.BPM.ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.CircleSize] = ((double)workingBeatmap.BeatmapInfo.Difficulty.CircleSize).ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.HPDrain] = ((double)workingBeatmap.BeatmapInfo.Difficulty.DrainRate).ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.Accuracy] = ((double)workingBeatmap.BeatmapInfo.Difficulty.OverallDifficulty).ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.ApproachRate] = ((double)workingBeatmap.BeatmapInfo.Difficulty.ApproachRate).ToLocalisableString(@"F2");
            valueDictionary[BeatmapAttribute.StarRating] = workingBeatmap.BeatmapInfo.StarRating.ToLocalisableString(@"F2");
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
    }
}
