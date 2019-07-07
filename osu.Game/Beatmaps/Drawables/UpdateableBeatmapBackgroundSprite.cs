// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Beatmaps.Drawables
{
    /// <summary>
    /// Display a beatmap background from a local source, but fallback to online source if not available.
    /// </summary>
    public class UpdateableBeatmapBackgroundSprite : ModelBackedDrawable<BeatmapInfo>
    {
        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private readonly BeatmapSetCoverType beatmapSetCoverType;

        public UpdateableBeatmapBackgroundSprite(BeatmapSetCoverType beatmapSetCoverType = BeatmapSetCoverType.Cover)
        {
            Beatmap.BindValueChanged(b => Model = b.NewValue);
            this.beatmapSetCoverType = beatmapSetCoverType;
        }

        /// <summary>
        /// Delay before the background is unloaded while off-screen.
        /// </summary>
        protected virtual double UnloadDelay => 10000;

        protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
            => new DelayedLoadUnloadWrapper(createContentFunc, timeBeforeLoad, UnloadDelay);

        protected override double TransformDuration => 400;

        protected override Drawable CreateDrawable(BeatmapInfo model)
        {
            var drawable = getDrawableForModel(model);
            drawable.RelativeSizeAxes = Axes.Both;
            drawable.Anchor = Anchor.Centre;
            drawable.Origin = Anchor.Centre;
            drawable.FillMode = FillMode.Fill;

            return drawable;
        }

        private Drawable getDrawableForModel(BeatmapInfo model)
        {
            // prefer online cover where available.
            if (model?.BeatmapSet?.OnlineInfo != null)
                return new BeatmapSetCover(model.BeatmapSet, beatmapSetCoverType);

            return model?.ID > 0
                ? new BeatmapBackgroundSprite(beatmaps.GetWorkingBeatmap(model))
                : new BeatmapBackgroundSprite(beatmaps.DefaultBeatmap);
        }
    }
}
