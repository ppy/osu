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

        public UpdateableBeatmapBackgroundSprite(BeatmapSetCoverType beatmapSetCoverType = BeatmapSetCoverType.Cover)
        {
            Beatmap.BindValueChanged(b => Model = b.NewValue);
            this.beatmapSetCoverType = beatmapSetCoverType;
        }

        protected override Drawable CreateDrawable(BeatmapInfo model)
        {
            return new DelayedLoadUnloadWrapper(() =>
            {
                if (model?.BeatmapSet?.OnlineInfo == null)
                    return null;

                Drawable drawable = new BeatmapSetCover(model.BeatmapSet, beatmapSetCoverType);

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
