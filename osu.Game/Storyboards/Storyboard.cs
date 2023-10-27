// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards
{
    public class Storyboard
    {
        private readonly Dictionary<string, StoryboardLayer> layers = new Dictionary<string, StoryboardLayer>();
        public IEnumerable<StoryboardLayer> Layers => layers.Values;

        public BeatmapInfo BeatmapInfo = new BeatmapInfo();

        /// <summary>
        /// Whether the storyboard should prefer textures from the current skin before using local storyboard textures.
        /// </summary>
        public bool UseSkinSprites { get; set; }

        public bool HasDrawable => Layers.Any(l => l.Elements.Any(e => e.IsDrawable));

        /// <summary>
        /// Across all layers, find the earliest point in time that a storyboard element exists at.
        /// Will return null if there are no elements.
        /// </summary>
        /// <remarks>
        /// This iterates all elements and as such should be used sparingly or stored locally.
        /// Video and background events are not included to match stable.
        /// </remarks>
        public double? EarliestEventTime => Layers.SelectMany(l => l.Elements)
                                                  .Where(e => e is not StoryboardVideo)
                                                  .MinBy(e => e.StartTime)?.StartTime;

        /// <summary>
        /// Across all layers, find the latest point in time that a storyboard element ends at.
        /// Will return null if there are no elements.
        /// </summary>
        /// <remarks>
        /// This iterates all elements and as such should be used sparingly or stored locally.
        /// Samples return StartTime as their EndTIme.
        /// Video and background events are not included to match stable.
        /// </remarks>
        public double? LatestEventTime => Layers.SelectMany(l => l.Elements)
                                                .Where(e => e is not StoryboardVideo)
                                                .MaxBy(e => e.GetEndTime())?.GetEndTime();

        /// <summary>
        /// Depth of the currently front-most storyboard layer, excluding the overlay layer.
        /// </summary>
        private int minimumLayerDepth;

        public Storyboard()
        {
            layers.Add("Video", new StoryboardVideoLayer("Video", 4, false));
            layers.Add("Background", new StoryboardLayer("Background", 3));
            layers.Add("Fail", new StoryboardLayer("Fail", 2) { VisibleWhenPassing = false, });
            layers.Add("Pass", new StoryboardLayer("Pass", 1) { VisibleWhenFailing = false, });
            layers.Add("Foreground", new StoryboardLayer("Foreground", minimumLayerDepth = 0));

            layers.Add("Overlay", new StoryboardLayer("Overlay", int.MinValue));
        }

        public StoryboardLayer GetLayer(string name)
        {
            if (!layers.TryGetValue(name, out var layer))
                layers[name] = layer = new StoryboardLayer(name, --minimumLayerDepth);

            return layer;
        }

        /// <summary>
        /// Whether the beatmap's background should be hidden while this storyboard is being displayed.
        /// </summary>
        public bool ReplacesBackground
        {
            get
            {
                string backgroundPath = BeatmapInfo.Metadata.BackgroundFile;

                if (string.IsNullOrEmpty(backgroundPath))
                    return false;

                // Importantly, do this after the NullOrEmpty because EF may have stored the non-nullable value as null to the database, bypassing compile-time constraints.
                backgroundPath = backgroundPath.ToLowerInvariant();

                return GetLayer("Background").Elements.Any(e => e.Path.ToLowerInvariant() == backgroundPath);
            }
        }

        public virtual DrawableStoryboard CreateDrawable(IReadOnlyList<Mod>? mods = null) =>
            new DrawableStoryboard(this, mods);

        private static readonly string[] image_extensions = { @".png", @".jpg" };

        public virtual string? GetStoragePathFromStoryboardPath(string path)
        {
            string? resolvedPath = null;

            if (Path.HasExtension(path))
            {
                resolvedPath = BeatmapInfo.BeatmapSet?.GetPathForFile(path);
            }
            else
            {
                // Some old storyboards don't include a file extension, so let's best guess at one.
                foreach (string ext in image_extensions)
                {
                    if ((resolvedPath = BeatmapInfo.BeatmapSet?.GetPathForFile($"{path}{ext}")) != null)
                        break;
                }
            }

            return resolvedPath;
        }
    }
}
