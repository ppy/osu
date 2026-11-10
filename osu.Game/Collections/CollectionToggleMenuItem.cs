// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Collections
{
    public class CollectionToggleMenuItem : ToggleMenuItem
    {
        public CollectionToggleMenuItem(Live<BeatmapCollection> collection, IBeatmapInfo beatmap)
            : base(collection.PerformRead(c => c.Name), MenuItemType.Standard, state => Task.Run(() =>
            {
                collection.PerformWrite(c =>
                {
                    if (state)
                        c.BeatmapMD5Hashes.Add(beatmap.MD5Hash);
                    else
                        c.BeatmapMD5Hashes.Remove(beatmap.MD5Hash);
                });
            }))
        {
            State.Value = collection.PerformRead(c => c.BeatmapMD5Hashes.Contains(beatmap.MD5Hash));
        }
    }
}
