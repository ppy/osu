// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Beatmaps.Drawables
{
    /// <summary>
    /// Display a beatmap background from an online source
    /// </summary>
    public class UpdateableBeatmapBackgroundSprite : ModelBackedDrawable<BeatmapInfo>
    {
        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private readonly BeatmapSetCoverType beatmapSetCoverType;

        private readonly bool fallback;

        public UpdateableBeatmapBackgroundSprite(bool fallback = false, BeatmapSetCoverType beatmapSetCoverType = BeatmapSetCoverType.Cover)
        {
            Beatmap.BindValueChanged(b => Model = b.NewValue);
            this.beatmapSetCoverType = beatmapSetCoverType;
            this.fallback = fallback;
        }

        protected override Drawable CreateDrawable(BeatmapInfo model)
        {
            return new DelayedLoadUnloadWrapper(() =>
            {
                Drawable drawable;

                var localBeatmap = beatmaps.GetWorkingBeatmap(model);

                if (model?.BeatmapSet?.OnlineInfo != null)
                    drawable = new BeatmapSetCover(model.BeatmapSet, beatmapSetCoverType);
                else if (fallback && localBeatmap.BeatmapInfo.ID != 0)
                {
                    // Fall back to local background if one exists
                    drawable = new BeatmapBackgroundSprite(localBeatmap);
                }
                else
                {
                    // Use the default background if somehow an online set does not exist and we don't have a local copy.
                    drawable = new BeatmapBackgroundSprite(beatmaps.GetWorkingBeatmap(null));
                }

                drawable.RelativeSizeAxes = Axes.Both;
                drawable.Anchor = Anchor.Centre;
                drawable.Origin = Anchor.Centre;
                drawable.FillMode = FillMode.Fill;
                drawable.OnLoadComplete = d => d.FadeInFromZero(400);

                return drawable;
            }, 500, 10000);
        }

        protected override double FadeDuration => 0;
    }
}
