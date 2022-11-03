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

        [SettingSource("Tracked Beatmap Info/Label", "Which part of the BeatmapInformation should be displayed. Gets overridden by complex changes to ValueFormat")]
        public Bindable<BeatmapInfo> Type { get; } = new Bindable<BeatmapInfo>(default_beatmap_info);

        [SettingSource("Show Label", "Should a Label be shown, as to which status is currently Displayed?")]
        public BindableBool ShowLabel { get; } = new BindableBool(true);

        [SettingSource("Show Value first?", "Should the Value be shown first?")]
        public BindableBool ValueBeforeLabel { get; } = new BindableBool();

        [SettingSource("Label Prefix", "Add something to be shown before the label")]
        public Bindable<string> LabelPrefix { get; set; } = new Bindable<string>("");

        [SettingSource("Show Label Prefix", "Should the Label Prefix be included?")]
        public BindableBool ShowLabelPrefix { get; } = new BindableBool();

        [SettingSource("Label Suffix", "Add something to be shown after the label")]
        public Bindable<string> LabelSuffix { get; set; } = new Bindable<string>(": ");

        [SettingSource("Show Label Suffix", "Should the Label Suffix be included?")]
        public BindableBool ShowLabelSuffix { get; } = new BindableBool(true);

        [SettingSource("Value Formatting", "Bypass the restriction of 1 Info per element. Format is '{'+Type+'}' to substitue values. e.g. '{Song}' ")]
        public Bindable<string> ValueFormat { get; set; } = new Bindable<string>("{" + default_beatmap_info + "}");

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
                [BeatmapInfo.Custom] = BeatmapInfo.Custom.ToString()
            }.ToImmutableDictionary();
        }

        public BeatmapInfoDrawable()
        {
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

        /// <summary>
        /// This will return the if the format-String contains of a singular replacement of type info, or not.
        /// If there is only one one replacement of type info, it will also return the prefix/suffix (or null if no prefix/suffix exists).
        /// </summary>
        /// <param name="format">The format-String to work on</param>
        /// <param name="info">The replacement Type to look for</param>
        /// <returns>(true, prefix, suffix), if there is only one replacement of type info. Else (false, null, null)</returns>
        private static (bool, string?, string?) isOnlyPrefixedOrSuffixed(string format, BeatmapInfo info)
        {
            string[] s = format.Split("{" + info + "}");

            foreach (string si in s)
            {
                foreach (var type in Enum.GetValues(typeof(BeatmapInfo)).Cast<BeatmapInfo>())
                {
                    if (si.Contains("{" + type + "}")) return (false, null, null);
                }
            }

            //Debug.WriteLine($"format:'{format}', type:{info} is only prefixed/suffixed");

            return (true,
                    s.Length >= 1 ? s[0] : null, //prefix
                    s.Length >= 2 ? s[1] : null //suffix
                );
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Type.BindValueChanged(v =>
            {
                string newDefault = "{" + v.NewValue + "}";
                bool custom = v.NewValue == BeatmapInfo.Custom;

                //If the ValueFormat is Default and the user did not change anything we should be able to just swap the strings.
                //If it was Default before, it should be default after the Type is changed.
                if (ValueFormat.IsDefault && !custom)
                    ValueFormat.Value = newDefault;
                else
                {
                    //In this if statement we decide if the ValueFormat has been trivially changed (so only been prefixed or suffixed)
                    (bool preOrSuffixed, string? prefix, string? suffix) = isOnlyPrefixedOrSuffixed(ValueFormat.Value, v.OldValue);
                    if (preOrSuffixed)
                        //If it has, we can keep the prefix and suffix and just change the thing that would be substituted.
                        ValueFormat.Value = (prefix ?? "") + newDefault + (suffix ?? "");
                    //else we just keep the ValueFormat. I determine here, that the user probably knows what they are doing, and how the ValueFormat works.
                }

                //Only if we could preserve the ValueFormat (so nothing was changed except a static prefix/suffix) I want to set the new Default.
                ValueFormat.Default = newDefault;
                updateLabel();
            });
            ValueFormat.BindValueChanged(f => updateLabel(), true);
            beatmap.BindValueChanged(b =>
            {
                UpdateBeatmapContent(b.NewValue);
                updateLabel();
            }, true);
            ShowLabel.BindValueChanged(_ => updateLabel());
            ValueBeforeLabel.BindValueChanged(_ => updateLabel());
            LabelPrefix.BindValueChanged(_ => updateLabel());
            ShowLabelPrefix.BindValueChanged(_ => updateLabel());
            LabelSuffix.BindValueChanged(_ => updateLabel());
            ShowLabelSuffix.BindValueChanged(_ => updateLabel());
        }

        private LocalisableString getLabelText()
        {
            if (!ShowLabel.Value) return new LocalisableString("");

            return LocalisableString.Format("{0}{1}{2}",
                ShowLabelPrefix.Value ? LabelPrefix.Value : "",
                label_dictionary[Type.Value],
                ShowLabelSuffix.Value ? LabelSuffix.Value : "");
        }

        private LocalisableString getValueText()
        {
            string value = ValueFormat.Value;

            foreach (var type in Enum.GetValues(typeof(BeatmapInfo)).Cast<BeatmapInfo>())
            {
                value = value.Replace("{" + type + "}", valueDictionary[type].ToString());
            }

            return value;
        }

        private void updateLabel()
        {
            text.Text = LocalisableString.Format(
                ValueBeforeLabel.Value ? "{1}{0}" : "{0}{1}",
                getLabelText(),
                getValueText()
            );
            Width = text.Width;
            Height = text.Height;
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
            valueDictionary[BeatmapInfo.Custom] = BeatmapInfo.Custom.ToString();
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
        Custom,
    }
}
