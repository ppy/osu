// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Screens.Multi.Components
{
    public class MultiplayerBackgroundSprite : MultiplayerComposite
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            UpdateableBeatmapBackgroundSprite sprite;

            InternalChild = sprite = CreateBackgroundSprite();

            CurrentItem.BindValueChanged(i => sprite.Beatmap.Value = i?.Beatmap, true);
        }

        protected virtual UpdateableBeatmapBackgroundSprite CreateBackgroundSprite() => new UpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both };
    }
}
