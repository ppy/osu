// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Beatmaps.Drawables
{
    public class UpdateableBeatmapBackgroundSprite : ModelBackedDrawable<WorkingBeatmap>
    {
        public readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        [Resolved]
        private OsuGameBase game { get; set; }

        public UpdateableBeatmapBackgroundSprite()
        {
            Beatmap.BindValueChanged(b => Schedule(() => Model = b));
        }

        protected override Drawable CreateDrawable(WorkingBeatmap model)
        {
            Drawable drawable = model == null ? (Drawable)new DefaultSprite() : new BeatmapBackgroundSprite(model);

            drawable.RelativeSizeAxes = Axes.Both;
            drawable.Anchor = Anchor.Centre;
            drawable.Origin = Anchor.Centre;
            drawable.FillMode = FillMode.Fill;

            return drawable;
        }

        protected override double FadeDuration => 400;

        private class DefaultSprite : Sprite
        {
            [Resolved]
            private IBindableBeatmap gameBeatmap { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                Texture = gameBeatmap.Default.Background;
            }
        }
    }
}
