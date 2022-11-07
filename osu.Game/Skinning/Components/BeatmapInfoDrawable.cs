// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
    public class BeatmapInfoDrawable : Container, ISkinnableDrawable
    {
        private const BeatmapInfo default_beatmap_info = BeatmapInfo.StarRating;
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Tracked Beatmap Info/Label", "Which part of the BeatmapInformation should be displayed.")]
        public Bindable<BeatmapInfo> Type { get; } = new Bindable<BeatmapInfo>(default_beatmap_info);

        [SettingSource("Template", "Bypass the restriction of 1 Info per element. Format is '{'+Type+'}' to substitue values. e.g. '{Song}' ")]
        public Bindable<string> Template { get; set; } = new Bindable<string>("{Label}: {Value}");

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        private readonly Dictionary<BeatmapInfo, LocalisableString> valueDictionary = new Dictionary<BeatmapInfo, LocalisableString>();
        private static readonly ImmutableDictionary<BeatmapInfo, LocalisableString> label_dictionary;

        private readonly OsuSpriteText text;

        static BeatmapInfoDrawable()
        {
            label_dictionary = new Dictionary<BeatmapInfo, LocalisableString>
            {
                [BeatmapInfo.CircleSize] = BeatmapsetsStrings.ShowStatsCs,
                [BeatmapInfo.Accuracy] = BeatmapsetsStrings.ShowStatsAccuracy,
                [BeatmapInfo.HPDrain] = BeatmapsetsStrings.ShowStatsDrain,
                [BeatmapInfo.ApproachRate] = BeatmapsetsStrings.ShowStatsAr,
                [BeatmapInfo.StarRating] = BeatmapsetsStrings.ShowStatsStars,
                [BeatmapInfo.Song] = EditorSetupStrings.Title,
                [BeatmapInfo.Artist] = EditorSetupStrings.Artist,
                [BeatmapInfo.Difficulty] = EditorSetupStrings.DifficultyHeader,
                //todo: is there a good alternative, to NotificationsOptionsMapping?
                [BeatmapInfo.Mapper] = AccountsStrings.NotificationsOptionsMapping,
                [BeatmapInfo.Length] = ArtistStrings.TracklistLength,
                [BeatmapInfo.Status] = BeatmapDiscussionsStrings.IndexFormBeatmapsetStatusDefault,
                [BeatmapInfo.BPM] = BeatmapsetsStrings.ShowStatsBpm,
                [BeatmapInfo.None] = BeatmapInfo.None.ToString()
            }.ToImmutableDictionary();
        }

        public BeatmapInfoDrawable()
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

            foreach (var type in Enum.GetValues(typeof(BeatmapInfo)).Cast<BeatmapInfo>())
            {
                valueDictionary[type] = type.ToString();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Type.BindValueChanged(_ => updateLabel());
            Template.BindValueChanged(f => updateLabel(), true);
            beatmap.BindValueChanged(b =>
            {
                UpdateBeatmapContent(b.NewValue);
                updateLabel();
            }, true);
        }

        private void updateLabel()
        {
            string newText = Template.Value.Replace("{Label}", label_dictionary[Type.Value].ToString())
                                     .Replace("{Value}", valueDictionary[Type.Value].ToString());

            foreach (var type in Enum.GetValues(typeof(BeatmapInfo)).Cast<BeatmapInfo>())
            {
                newText = newText.Replace("{" + type + "}", valueDictionary[type].ToString());
            }

            text.Text = newText;
        }

        public void UpdateBeatmapContent(WorkingBeatmap workingBeatmap)
        {
            //update cs
            double cs = workingBeatmap.BeatmapInfo.Difficulty.CircleSize;
            valueDictionary[BeatmapInfo.CircleSize] = cs.ToString("F2");
            //update HP
            double hp = workingBeatmap.BeatmapInfo.Difficulty.DrainRate;
            valueDictionary[BeatmapInfo.HPDrain] = hp.ToString("F2");
            //update od
            double od = workingBeatmap.BeatmapInfo.Difficulty.OverallDifficulty;
            valueDictionary[BeatmapInfo.Accuracy] = od.ToString("F2");
            //update ar
            double ar = workingBeatmap.BeatmapInfo.Difficulty.ApproachRate;
            valueDictionary[BeatmapInfo.ApproachRate] = ar.ToString("F2");
            //update sr
            double sr = workingBeatmap.BeatmapInfo.StarRating;
            valueDictionary[BeatmapInfo.StarRating] = sr.ToString("F2");
            //update song title
            valueDictionary[BeatmapInfo.Song] = workingBeatmap.BeatmapInfo.Metadata.Title;
            //update artist
            valueDictionary[BeatmapInfo.Artist] = workingBeatmap.BeatmapInfo.Metadata.Artist;
            //update difficulty name
            valueDictionary[BeatmapInfo.Difficulty] = workingBeatmap.BeatmapInfo.DifficultyName;
            //update mapper
            valueDictionary[BeatmapInfo.Mapper] = workingBeatmap.BeatmapInfo.Metadata.Author.Username;
            //update Length
            valueDictionary[BeatmapInfo.Length] = TimeSpan.FromMilliseconds(workingBeatmap.BeatmapInfo.Length).ToFormattedDuration();
            //update Status
            valueDictionary[BeatmapInfo.Status] = GetBetmapStatus(workingBeatmap.BeatmapInfo.Status);
            //update BPM
            valueDictionary[BeatmapInfo.BPM] = workingBeatmap.BeatmapInfo.BPM.ToString("F2");
            valueDictionary[BeatmapInfo.None] = string.Empty;
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

    public enum BeatmapInfo
    {
        CircleSize,
        HPDrain,
        Accuracy,
        ApproachRate,
        StarRating,
        Song,
        Artist,
        Difficulty,
        Mapper,
        Length,
        Status,
        BPM,
        None,
    }
}
