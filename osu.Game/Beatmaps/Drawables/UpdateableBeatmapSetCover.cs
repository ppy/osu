// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class UpdateableBeatmapSetCover : ModelBackedDrawable<BeatmapSetInfo>
    {
        private readonly BeatmapSetCoverType coverType;

        public BeatmapSetInfo BeatmapSet
        {
            get => Model;
            set => Model = value;
        }

        public new bool Masking
        {
            get => base.Masking;
            set => base.Masking = value;
        }

        public UpdateableBeatmapSetCover(BeatmapSetCoverType coverType = BeatmapSetCoverType.Cover)
        {
            this.coverType = coverType;

            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(0.2f),
            };
        }

        protected override double LoadDelay => 500;

        protected override double TransformDuration => 400;

        protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
            => new DelayedLoadUnloadWrapper(createContentFunc, timeBeforeLoad);

        protected override Drawable CreateDrawable(BeatmapSetInfo model)
        {
            if (model == null)
                return null;

            return new BeatmapSetCover(model, coverType)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
            };
        }
    }
}
