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
            Type.BindValueChanged(update, true);
            ShowLabel.BindValueChanged(ignored => updateLabel());
            ValueBeforeLabel.BindValueChanged(ignored => updateLabel());
            LabelPrefix.BindValueChanged(ignored => updateLabel());
            ShowLabelPrefix.BindValueChanged(ignored => updateLabel());
            LabelSuffix.BindValueChanged(ignored => updateLabel());
            ShowLabelSuffix.BindValueChanged(ignored => updateLabel());
            ValuePrefix.BindValueChanged(ignored => updateLabel());
            ShowValuePrefix.BindValueChanged(ignored => updateLabel());
            ValueSuffix.BindValueChanged(ignored => updateLabel());
            ShowValueSuffix.BindValueChanged(ignored => updateLabel());
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

        private void update(ValueChangedEvent<BeatmapInfo> type)
        {
            switch (type.NewValue)
            {
                case BeatmapInfo.CircleSize:
                    beatmap.BindValueChanged(bm =>
                    {
                        double cs = bm.NewValue.BeatmapInfo.Difficulty.CircleSize;
                        labelText = TooltipText = BeatmapsetsStrings.ShowStatsCs;
                        value = cs.ToString("F2");
                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.HPDrain:
                    beatmap.BindValueChanged(bm =>
                    {
                        double hp = bm.NewValue.BeatmapInfo.Difficulty.DrainRate;
                        labelText = TooltipText = BeatmapsetsStrings.ShowStatsDrain;
                        value = hp.ToString("F2");
                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.Accuracy:
                    beatmap.BindValueChanged(bm =>
                    {
                        double od = bm.NewValue.BeatmapInfo.Difficulty.OverallDifficulty;
                        labelText = TooltipText = BeatmapsetsStrings.ShowStatsAccuracy;
                        value = od.ToString("F2");
                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.ApproachRate:
                    beatmap.BindValueChanged(bm =>
                    {
                        double ar = bm.NewValue.BeatmapInfo.Difficulty.ApproachRate;
                        labelText = TooltipText = BeatmapsetsStrings.ShowStatsAr;
                        value = ar.ToString("F2");
                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.StarRating:
                    beatmap.BindValueChanged(bm =>
                    {
                        double sr = bm.NewValue.BeatmapInfo.StarRating;
                        labelText = TooltipText = BeatmapsetsStrings.ShowStatsStars;
                        value = sr.ToString("F2");
                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.Song:
                    beatmap.BindValueChanged(bm =>
                    {
                        string title = bm.NewValue.BeatmapInfo.Metadata.Title;
                        //todo: no Song Title localisation?
                        labelText = TooltipText = "Song Title";
                        value = title;
                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.Artist:
                    beatmap.BindValueChanged(bm =>
                    {
                        string artist = bm.NewValue.BeatmapInfo.Metadata.Artist;
                        //todo: Localize Artist
                        labelText = "Artist";
                        TooltipText = BeatmapsetsStrings.ShowDetailsByArtist(artist);
                        value = artist;
                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.Difficulty:
                    beatmap.BindValueChanged(bm =>
                    {
                        string diff = bm.NewValue.BeatmapInfo.DifficultyName;
                        //todo: no Difficulty name localisation?
                        labelText = TooltipText = "Difficulty";
                        text.Current.Value = diff;
                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.Mapper:
                    beatmap.BindValueChanged(bm =>
                    {
                        string mapper = bm.NewValue.BeatmapInfo.Metadata.Author.Username;
                        //todo: is there a good alternative, to ShowDetailsMappedBy?
                        labelText = "Mapper";
                        TooltipText = BeatmapsetsStrings.ShowDetailsMappedBy(mapper);
                        value = mapper;
                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.Length:
                    beatmap.BindValueChanged(bm =>
                    {
                        labelText = TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(TimeSpan.FromMilliseconds(bm.NewValue.BeatmapInfo.Length).ToFormattedDuration());
                        value = TimeSpan.FromMilliseconds(bm.NewValue.BeatmapInfo.Length).ToFormattedDuration();
                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.Status:
                    beatmap.BindValueChanged(bm =>
                    {
                        BeatmapOnlineStatus status = bm.NewValue.BeatmapInfo.Status;
                        //todo: no Localizasion for None Beatmap Online Status
                        //todo: no Localization for Status?
                        labelText = "Status";

                        switch (status)
                        {
                            case BeatmapOnlineStatus.Approved:
                                value = BeatmapsetsStrings.ShowStatusApproved;
                                //todo: is this correct?
                                TooltipText = BeatmapsetsStrings.ShowDetailsDateApproved(bm.NewValue.BeatmapSetInfo.DateRanked.ToString());
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

                        updateLabel();
                    }, true);
                    break;

                case BeatmapInfo.BPM:
                    beatmap.BindValueChanged(bm =>
                    {
                        labelText = TooltipText = BeatmapsetsStrings.ShowStatsBpm;
                        value = bm.NewValue.BeatmapInfo.BPM.ToString("F2");
                        updateLabel();
                    }, true);
                    break;
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
    }
}
