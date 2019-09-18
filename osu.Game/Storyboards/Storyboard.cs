// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Storyboards.Drawables;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Storyboards
{
    public class Storyboard
    {
        private readonly Dictionary<string, StoryboardLayer> layers = new Dictionary<string, StoryboardLayer>();
        public IEnumerable<StoryboardLayer> Layers => layers.Values;

        public BeatmapInfo BeatmapInfo = new BeatmapInfo();

        public bool HasDrawable => Layers.Any(l => l.Elements.Any(e => e.IsDrawable));

        public Storyboard()
        {
            layers.Add("Background", new StoryboardLayer("Background", 3));
            layers.Add("Fail", new StoryboardLayer("Fail", 2) { EnabledWhenPassing = false, });
            layers.Add("Pass", new StoryboardLayer("Pass", 1) { EnabledWhenFailing = false, });
            layers.Add("Foreground", new StoryboardLayer("Foreground", 0));
        }

        public StoryboardLayer GetLayer(string name)
        {
            StoryboardLayer layer;
            if (!layers.TryGetValue(name, out layer))
                layers[name] = layer = new StoryboardLayer(name, layers.Values.Min(l => l.Depth) - 1);

            return layer;
        }

        /// <summary>
        /// Whether the beatmap's background should be hidden while this storyboard is being displayed.
        /// </summary>
        public bool ReplacesBackground
        {
            get
            {
                var backgroundPath = BeatmapInfo.BeatmapSet?.Metadata?.BackgroundFile?.ToLowerInvariant();
                if (backgroundPath == null)
                    return false;

                return GetLayer("Background").Elements.Any(e => e.Path.ToLowerInvariant() == backgroundPath);
            }
        }

        public DrawableStoryboard CreateDrawable(WorkingBeatmap working = null)
        {
            var drawable = new DrawableStoryboard(this);
            drawable.Width = drawable.Height * (BeatmapInfo.WidescreenStoryboard ? 16 / 9f : 4 / 3f);
            return drawable;
        }
    }
}
