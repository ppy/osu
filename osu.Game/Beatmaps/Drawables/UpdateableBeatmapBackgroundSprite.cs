// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        public readonly IBindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public UpdateableBeatmapBackgroundSprite()
        {
            Beatmap.BindValueChanged(b => Schedule(() => Model = b));
        }

        protected override Drawable CreateDrawable(BeatmapInfo model)
        {
            Drawable drawable;

            var localBeatmap = beatmaps.GetWorkingBeatmap(model);

            if (localBeatmap == beatmaps.DefaultBeatmap && model?.BeatmapSet?.OnlineInfo != null)
                drawable = new BeatmapSetCover(model.BeatmapSet);
            else
                drawable = new BeatmapBackgroundSprite(localBeatmap);

            drawable.RelativeSizeAxes = Axes.Both;
            drawable.Anchor = Anchor.Centre;
            drawable.Origin = Anchor.Centre;
            drawable.FillMode = FillMode.Fill;

            return drawable;
        }

        protected override double FadeDuration => 400;
    }
}
