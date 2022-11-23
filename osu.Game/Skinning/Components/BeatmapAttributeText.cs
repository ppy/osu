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
    public class BeatmapAttributeText : Container, ISkinnableDrawable
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
                    Text = "BeatInfoDrawable",
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = OsuFont.Default.With(size: 40)
                }
            };

            foreach (var type in Enum.GetValues(typeof(BeatmapAttribute)).Cast<BeatmapAttribute>())
            {
                valueDictionary[type] = type.ToString();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Attribute.BindValueChanged(_ => updateLabel());
            Template.BindValueChanged(f => updateLabel(), true);
            beatmap.BindValueChanged(b =>
            {
                UpdateBeatmapContent(b.NewValue);
                updateLabel();
            }, true);
        }

        private void updateLabel()
        {
            string newText = Template.Value.Replace("{Label}", label_dictionary[Attribute.Value].ToString())
                                     .Replace("{Value}", valueDictionary[Attribute.Value].ToString());

            foreach (var type in Enum.GetValues(typeof(BeatmapAttribute)).Cast<BeatmapAttribute>())
            {
                newText = newText.Replace("{" + type + "}", valueDictionary[type].ToString());
            }

            text.Text = newText;
        }

        public void UpdateBeatmapContent(WorkingBeatmap workingBeatmap)
        {
            //update cs
            double cs = workingBeatmap.BeatmapInfo.Difficulty.CircleSize;
            valueDictionary[BeatmapAttribute.CircleSize] = cs.ToString("F2");
            //update HP
            double hp = workingBeatmap.BeatmapInfo.Difficulty.DrainRate;
            valueDictionary[BeatmapAttribute.HPDrain] = hp.ToString("F2");
            //update od
            double od = workingBeatmap.BeatmapInfo.Difficulty.OverallDifficulty;
            valueDictionary[BeatmapAttribute.Accuracy] = od.ToString("F2");
            //update ar
            double ar = workingBeatmap.BeatmapInfo.Difficulty.ApproachRate;
            valueDictionary[BeatmapAttribute.ApproachRate] = ar.ToString("F2");
            //update sr
            double sr = workingBeatmap.BeatmapInfo.StarRating;
            valueDictionary[BeatmapAttribute.StarRating] = sr.ToString("F2");
            //update song title
            valueDictionary[BeatmapAttribute.Title] = workingBeatmap.BeatmapInfo.Metadata.Title;
            //update artist
            valueDictionary[BeatmapAttribute.Artist] = workingBeatmap.BeatmapInfo.Metadata.Artist;
            //update difficulty name
            valueDictionary[BeatmapAttribute.DifficultyName] = workingBeatmap.BeatmapInfo.DifficultyName;
            //update mapper
            valueDictionary[BeatmapAttribute.Creator] = workingBeatmap.BeatmapInfo.Metadata.Author.Username;
            //update Length
            valueDictionary[BeatmapAttribute.Length] = TimeSpan.FromMilliseconds(workingBeatmap.BeatmapInfo.Length).ToFormattedDuration();
            //update Status
            valueDictionary[BeatmapAttribute.RankedStatus] = GetBetmapStatus(workingBeatmap.BeatmapInfo.Status);
            //update BPM
            valueDictionary[BeatmapAttribute.BPM] = workingBeatmap.BeatmapInfo.BPM.ToString("F2");
        }

        public static LocalisableString GetBetmapStatus(BeatmapOnlineStatus status)
        {
            switch (status)
            {
                case BeatmapOnlineStatus.Approved:
                    return BeatmapsetsStrings.ShowStatusApproved;

                case BeatmapOnlineStatus.Graveyard:
                    return BeatmapsetsStrings.ShowStatusGraveyard;

                case BeatmapOnlineStatus.Loved:
                    return BeatmapsetsStrings.ShowStatusLoved;

                case BeatmapOnlineStatus.None:
                    return "None";

                case BeatmapOnlineStatus.Pending:
                    return BeatmapsetsStrings.ShowStatusPending;

                case BeatmapOnlineStatus.Qualified:
                    return BeatmapsetsStrings.ShowStatusQualified;

                case BeatmapOnlineStatus.Ranked:
                    return BeatmapsetsStrings.ShowStatusRanked;

                case BeatmapOnlineStatus.LocallyModified:
                    return SongSelectStrings.LocallyModified;

                case BeatmapOnlineStatus.WIP:
                    return BeatmapsetsStrings.ShowStatusWip;

                default:
                    return @"null";
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
    }
}
