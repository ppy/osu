// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards
{
    public class Storyboard
    {
        private readonly Dictionary<string, StoryboardLayer> layers = new Dictionary<string, StoryboardLayer>();
        public IEnumerable<StoryboardLayer> Layers => layers.Values;

        public BeatmapInfo BeatmapInfo = new BeatmapInfo();

        /// <summary>
        /// Whether the storyboard can fall back to skin sprites in case no matching storyboard sprites are found.
        /// </summary>
        public bool UseSkinSprites { get; set; }

        public bool HasDrawable => Layers.Any(l => l.Elements.Any(e => e.IsDrawable));

        /// <summary>
        /// Across all layers, find the earliest point in time that a storyboard element exists at.
        /// Will return null if there are no elements.
        /// </summary>
        /// <remarks>
        /// This iterates all elements and as such should be used sparingly or stored locally.
        /// </remarks>
        public double? EarliestEventTime => Layers.SelectMany(l => l.Elements).OrderBy(e => e.StartTime).FirstOrDefault()?.StartTime;

        /// <summary>
        /// Across all layers, find the latest point in time that a storyboard element ends at.
        /// Will return null if there are no elements.
        /// </summary>
        /// <remarks>
        /// This iterates all elements and as such should be used sparingly or stored locally.
        /// Videos and samples return StartTime as their EndTIme.
        /// </remarks>
        public double? LatestEventTime => Layers.SelectMany(l => l.Elements).OrderBy(e => e.GetEndTime()).LastOrDefault()?.GetEndTime();

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
                string backgroundPath = BeatmapInfo.BeatmapSet?.Metadata?.BackgroundFile;

                if (string.IsNullOrEmpty(backgroundPath))
                    return false;

                // Importantly, do this after the NullOrEmpty because EF may have stored the non-nullable value as null to the database, bypassing compile-time constraints.
                backgroundPath = backgroundPath.ToLowerInvariant();

                return GetLayer("Background").Elements.Any(e => e.Path.ToLowerInvariant() == backgroundPath);
            }
        }

        public DrawableStoryboard CreateDrawable(WorkingBeatmap working = null) =>
            new DrawableStoryboard(this);

        public Drawable CreateSpriteFromResourcePath(string path, TextureStore textureStore)
        {
            Drawable drawable = null;
            string storyboardPath = BeatmapInfo.BeatmapSet?.Files.Find(f => f.Filename.Equals(path, StringComparison.OrdinalIgnoreCase))?.FileInfo.StoragePath;

            if (!string.IsNullOrEmpty(storyboardPath))
                drawable = new Sprite { Texture = textureStore.Get(storyboardPath) };
            // if the texture isn't available locally in the beatmap, some storyboards choose to source from the underlying skin lookup hierarchy.
            else if (UseSkinSprites)
                drawable = new SkinnableSprite(path);

            return drawable;
        }
    }
}
