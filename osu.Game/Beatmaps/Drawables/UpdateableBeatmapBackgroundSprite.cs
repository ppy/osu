// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Beatmaps.Drawables
{
    /// <summary>
    /// Display a baetmap background from a local source, but fallback to online source if not available.
    /// </summary>
    public class UpdateableBeatmapBackgroundSprite : ModelBackedDrawable<BeatmapInfo>
    {
        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public UpdateableBeatmapBackgroundSprite()
        {
            Beatmap.BindValueChanged(b => Model = b);
        }

        protected override Drawable CreateDrawable(BeatmapInfo model)
        {
            return new DelayedLoadUnloadWrapper(() => {
                Drawable drawable;

                var localBeatmap = beatmaps.GetWorkingBeatmap(model);

                if (localBeatmap.BeatmapInfo.ID == 0 && model?.BeatmapSet?.OnlineInfo != null)
                    drawable = new BeatmapSetCover(model.BeatmapSet);
                else
                    drawable = new BeatmapBackgroundSprite(localBeatmap);

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
