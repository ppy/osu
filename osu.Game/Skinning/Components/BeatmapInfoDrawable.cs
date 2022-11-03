// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
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
    public class BeatmapInfoDrawable : Container, ISkinnableDrawable, IHasTooltip
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Tracked Beatmap Info", "Which part of the BeatmapInformation should be tracked")]
        public Bindable<BeatmapInfo> Type { get; } = new Bindable<BeatmapInfo>(BeatmapInfo.StarRating);

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

        [SettingSource("Value Prefix", "Add something to be shown before the Value")]
        public Bindable<string> ValuePrefix { get; set; } = new Bindable<string>("");

        [SettingSource("Show Value Prefix", "Should the Value Prefix be included?")]
        public BindableBool ShowValuePrefix { get; } = new BindableBool();

        [SettingSource("Value Suffix", "Add something to be shown after the Value")]
        public Bindable<string> ValueSuffix { get; set; } = new Bindable<string>("");

        [SettingSource("Show Value Suffix", "Should the Value Suffix be included?")]
        public BindableBool ShowValueSuffix { get; } = new BindableBool();

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        private readonly OsuSpriteText text;

        public LocalisableString TooltipText { get; set; }
        private LocalisableString value;
        private LocalisableString labelText;

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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Type.BindValueChanged(_ => updateBeatmapContent());
            beatmap.BindValueChanged(_ => updateBeatmapContent(), true);
            ShowLabel.BindValueChanged(_ => updateLabel());
            ValueBeforeLabel.BindValueChanged(_ => updateLabel());
            LabelPrefix.BindValueChanged(_ => updateLabel());
            ShowLabelPrefix.BindValueChanged(_ => updateLabel());
            LabelSuffix.BindValueChanged(_ => updateLabel());
            ShowLabelSuffix.BindValueChanged(_ => updateLabel());
            ValuePrefix.BindValueChanged(_ => updateLabel());
            ShowValuePrefix.BindValueChanged(_ => updateLabel());
            ValueSuffix.BindValueChanged(_ => updateLabel());
            ShowValueSuffix.BindValueChanged(_ => updateLabel());
        }

        private LocalisableString getLabelText()
        {
            if (!ShowLabel.Value) return new LocalisableString("");

            return LocalisableString.Format("{0}{1}{2}",
                ShowLabelPrefix.Value ? LabelPrefix.Value : "",
                labelText,
                ShowLabelSuffix.Value ? LabelSuffix.Value : "");
        }

        private LocalisableString getValueText()
        {
            return LocalisableString.Format("{0}{1}{2}",
                ShowValuePrefix.Value ? ValuePrefix.Value : "",
                value,
                ShowValueSuffix.Value ? ValueSuffix.Value : "");
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

        private void updateBeatmapContent()
        {
            switch (Type.Value)
            {
                case BeatmapInfo.CircleSize:
                    double cs = beatmap.Value.BeatmapInfo.Difficulty.CircleSize;
                    labelText = TooltipText = BeatmapsetsStrings.ShowStatsCs;
                    value = cs.ToString("F2");
                    break;

                case BeatmapInfo.HPDrain:
                    double hp = beatmap.Value.BeatmapInfo.Difficulty.DrainRate;
                    labelText = TooltipText = BeatmapsetsStrings.ShowStatsDrain;
                    value = hp.ToString("F2");
                    break;

                case BeatmapInfo.Accuracy:
                    double od = beatmap.Value.BeatmapInfo.Difficulty.OverallDifficulty;
                    labelText = TooltipText = BeatmapsetsStrings.ShowStatsAccuracy;
                    value = od.ToString("F2");
                    break;

                case BeatmapInfo.ApproachRate:
                    double ar = beatmap.Value.BeatmapInfo.Difficulty.ApproachRate;
                    labelText = TooltipText = BeatmapsetsStrings.ShowStatsAr;
                    value = ar.ToString("F2");
                    break;

                case BeatmapInfo.StarRating:
                    double sr = beatmap.Value.BeatmapInfo.StarRating;
                    labelText = TooltipText = BeatmapsetsStrings.ShowStatsStars;
                    value = sr.ToString("F2");
                    break;

                case BeatmapInfo.Song:
                    string title = beatmap.Value.BeatmapInfo.Metadata.Title;
                    labelText = TooltipText = EditorSetupStrings.Title;
                    value = title;
                    break;

                case BeatmapInfo.Artist:
                    string artist = beatmap.Value.BeatmapInfo.Metadata.Artist;
                    labelText = EditorSetupStrings.Artist;
                    TooltipText = BeatmapsetsStrings.ShowDetailsByArtist(artist);
                    value = artist;
                    break;

                case BeatmapInfo.Difficulty:
                    string diff = beatmap.Value.BeatmapInfo.DifficultyName;
                    labelText = TooltipText = EditorSetupStrings.DifficultyHeader;
                    text.Current.Value = diff;
                    break;

                case BeatmapInfo.Mapper:
                    string mapper = beatmap.Value.BeatmapInfo.Metadata.Author.Username;
                    //todo: is there a good alternative, to NotificationsOptionsMapping?
                    labelText = AccountsStrings.NotificationsOptionsMapping;
                    TooltipText = BeatmapsetsStrings.ShowDetailsMappedBy(mapper);
                    value = mapper;
                    break;

                case BeatmapInfo.Length:
                    labelText = TooltipText = ArtistStrings.TracklistLength;
                    value = TimeSpan.FromMilliseconds(beatmap.Value.BeatmapInfo.Length).ToFormattedDuration();
                    break;

                case BeatmapInfo.Status:
                    BeatmapOnlineStatus status = beatmap.Value.BeatmapInfo.Status;
                    TooltipText = labelText = BeatmapDiscussionsStrings.IndexFormBeatmapsetStatusDefault;

                    switch (status)
                    {
                        case BeatmapOnlineStatus.Approved:
                            value = BeatmapsetsStrings.ShowStatusApproved;
                            break;

                        case BeatmapOnlineStatus.Graveyard:
                            value = BeatmapsetsStrings.ShowStatusGraveyard;
                            break;

                        case BeatmapOnlineStatus.Loved:
                            value = BeatmapsetsStrings.ShowStatusLoved;
                            break;

                        case BeatmapOnlineStatus.None:
                            value = "None";
                            break;

                        case BeatmapOnlineStatus.Pending:
                            value = BeatmapsetsStrings.ShowStatusPending;
                            break;

                        case BeatmapOnlineStatus.Qualified:
                            value = BeatmapsetsStrings.ShowStatusQualified;
                            break;

                        case BeatmapOnlineStatus.Ranked:
                            value = BeatmapsetsStrings.ShowStatusRanked;
                            break;

                        case BeatmapOnlineStatus.LocallyModified:
                            value = SongSelectStrings.LocallyModified;
                            break;

                        case BeatmapOnlineStatus.WIP:
                            value = BeatmapsetsStrings.ShowStatusWip;
                            break;
                    }

                    break;

                case BeatmapInfo.BPM:
                    labelText = TooltipText = BeatmapsetsStrings.ShowStatsBpm;
                    value = beatmap.Value.BeatmapInfo.BPM.ToString("F2");
                    break;
            }

            updateLabel();
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
    }
}
