// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Text;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Skinning.Components
{
    /// <summary>
    /// Intended to be a test bed for skinning. May be removed at some point in the future.
    /// </summary>
    [UsedImplicitly]
    public class BeatmapInfoDrawable : Container, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Tracked Beatmap Info", "Which part of the BeatmapInformation should be tracked")]
        public Bindable<BeatmapInfo> Type { get; } = new Bindable<BeatmapInfo>(BeatmapInfo.StarRating);

        [Resolved]
        private OsuGameBase mGameBase { get; set; }

        private readonly OsuSpriteText text;

        public BeatmapInfoDrawable()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Text = "BeatInfoDrawable",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Default.With(size: 40)
                }
            };
        }

        // [BackgroundDependencyLoader]
        // private void load(WorkingBeatmap beatmap)
        // {
        //     this.beatmap = beatmap;
        // }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Type.BindValueChanged(update, true);
        }

        private void update(ValueChangedEvent<BeatmapInfo> type)
        {
            switch (type.NewValue)
            {
                case BeatmapInfo.CircleSize:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        text.Current.Value = bm.NewValue.BeatmapInfo.Difficulty.CircleSize.ToString("F2");
                    }, true);
                    break;

                case BeatmapInfo.HPDrain:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        text.Current.Value = bm.NewValue.BeatmapInfo.Difficulty.DrainRate.ToString("F2");
                    }, true);
                    break;

                case BeatmapInfo.Accuracy:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        text.Current.Value = bm.NewValue.BeatmapInfo.Difficulty.OverallDifficulty.ToString("F2");
                    }, true);
                    break;

                case BeatmapInfo.ApproachRate:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        text.Current.Value = bm.NewValue.BeatmapInfo.Difficulty.ApproachRate.ToString("F2");
                    }, true);
                    break;

                case BeatmapInfo.StarRating:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        text.Current.Value = bm.NewValue.BeatmapInfo.StarRating.ToString("F2");
                    }, true);
                    break;

                case BeatmapInfo.Song:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        text.Current.Value = bm.NewValue.BeatmapInfo.Metadata.Title;
                    }, true);
                    break;

                case BeatmapInfo.Artist:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        text.Current.Value = bm.NewValue.BeatmapInfo.Metadata.Artist;
                    }, true);
                    break;

                case BeatmapInfo.Difficulty:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        text.Current.Value = bm.NewValue.BeatmapInfo.DifficultyName;
                    }, true);
                    break;

                case BeatmapInfo.Mapper:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        text.Current.Value = bm.NewValue.BeatmapInfo.Metadata.Author.Username;
                    }, true);
                    break;

                case BeatmapInfo.Length:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        const long ms_to_s = 1000;
                        double length = bm.NewValue.BeatmapInfo.Length;
                        double rawS = length / ms_to_s;
                        double rawM = rawS / 60;
                        double rawH = rawM / 60;
                        double rawD = rawH / 24;

                        long s = (long)rawS % 60;
                        long m = (long)rawM % 60;
                        long h = (long)rawH % 24;
                        long d = (long)rawD;
                        StringBuilder builder = new StringBuilder();

                        if (d != 0)
                        {
                            builder.Append(d.ToString("D2"));
                            builder.Append(":");
                        }

                        if (h != 0 || d != 0)
                        {
                            builder.Append(h.ToString("D2"));
                            builder.Append(":");
                        }

                        builder.Append(m.ToString("D2"));
                        builder.Append(":");
                        builder.Append(s.ToString("D2"));
                        text.Current.Value = builder.ToString();
                    }, true);
                    break;

                case BeatmapInfo.BPM:
                    mGameBase.Beatmap.BindValueChanged(bm =>
                    {
                        text.Current.Value = bm.NewValue.BeatmapInfo.BPM.ToString("F2");
                    }, true);
                    break;
            }
        }
    }

    public enum BeatmapInfo
    {
        CircleSize,
        HPDrain,
        Accuracy, //OD?
        ApproachRate,
        StarRating,
        Song,
        Artist,
        Difficulty,
        Mapper,
        Length,
        BPM,
    }
}
