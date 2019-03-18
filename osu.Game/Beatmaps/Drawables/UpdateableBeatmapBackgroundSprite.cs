// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        private BeatmapInfo lastModel;

        protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Drawable content, double timeBeforeLoad)
        {
            return new DelayedLoadUnloadWrapper(() =>
            {
                // If DelayedLoadUnloadWrapper is attempting to RELOAD the same content (Beatmap), that means that it was
                // previously UNLOADED and thus its children have been disposed of, so we need to recreate them here.
                if (lastModel == Beatmap.Value && Beatmap.Value != null)
                    return CreateDrawable(Beatmap.Value);

                // If the model has changed since the previous unload (or if there was no load), then we can safely use the given content
                lastModel = Beatmap.Value;
                return content;
            }, timeBeforeLoad, 10000);
        }

        protected override Drawable CreateDrawable(BeatmapInfo model)
        {
            Drawable drawable = getDrawableForModel(model);

            drawable.RelativeSizeAxes = Axes.Both;
            drawable.Anchor = Anchor.Centre;
            drawable.Origin = Anchor.Centre;
            drawable.FillMode = FillMode.Fill;
            drawable.OnLoadComplete += d => d.FadeInFromZero(400);

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
