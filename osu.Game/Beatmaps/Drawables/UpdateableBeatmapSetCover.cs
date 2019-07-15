// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class UpdateableBeatmapSetCover : ModelBackedDrawable<BeatmapSetInfo>
    {
        protected readonly BeatmapSetCoverType CoverType;

        public BeatmapSetInfo BeatmapSet
        {
            get => Model;
            set => Model = value;
        }

        protected override double TransformDuration => 400;

        public UpdateableBeatmapSetCover(BeatmapSetCoverType type = BeatmapSetCoverType.Cover)
        {
            CoverType = type;

            Masking = true;

            AddInternal(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ColourInfo.GradientVertical(OsuColour.Gray(0.2f), OsuColour.Gray(0.1f)),
            });
        }

        protected override Drawable CreateDrawable(BeatmapSetInfo setInfo) => setInfo != null
            ? new BeatmapSetCover(setInfo, CoverType)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fill,
            }
            : null;
    }
}
