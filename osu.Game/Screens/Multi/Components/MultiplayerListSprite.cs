// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Screens.Multi.Components
{
    public class MultiplayerListSprite : MultiplayerBackgroundSprite
    {
        protected override UpdateableBeatmapBackgroundSprite CreateBackgroundSprite() => new UpdateableBeatmapBackgroundSprite(BeatmapSetCoverType.List) { RelativeSizeAxes = Axes.Both };
    }
}
