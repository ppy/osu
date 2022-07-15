// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Collections
{
    public class CollectionToggleMenuItem : ToggleMenuItem
    {
        public CollectionToggleMenuItem(BeatmapCollection collection, IBeatmapInfo beatmap)
            : base(collection.Name.Value, MenuItemType.Standard, state =>
            {
                if (state)
                    collection.BeatmapHashes.Add(beatmap.MD5Hash);
                else
                    collection.BeatmapHashes.Remove(beatmap.MD5Hash);
            })
        {
            State.Value = collection.BeatmapHashes.Contains(beatmap.MD5Hash);
        }
    }
}
