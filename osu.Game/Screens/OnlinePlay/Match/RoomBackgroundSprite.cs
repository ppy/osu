// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Screens.OnlinePlay.Match
{
    public class RoomBackgroundSprite : RoomSubScreenComposite
    {
        protected readonly BeatmapSetCoverType BeatmapSetCoverType;
        private UpdateableBeatmapBackgroundSprite sprite;

        public RoomBackgroundSprite(BeatmapSetCoverType beatmapSetCoverType = BeatmapSetCoverType.Cover)
        {
            BeatmapSetCoverType = beatmapSetCoverType;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = sprite = new UpdateableBeatmapBackgroundSprite(BeatmapSetCoverType) { RelativeSizeAxes = Axes.Both };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(item => sprite.Beatmap.Value = item.NewValue?.Beatmap.Value, true);
        }
    }
}
