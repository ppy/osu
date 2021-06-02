// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;

namespace osu.Game.Graphics.Backgrounds
{
    public class BeatmapBackground : Background
    {
        public readonly WorkingBeatmap Beatmap;

        private readonly string fallbackTextureName;

        [Resolved]
        private LargeTextureStore textures { get; set; }

        public BeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName = @"Backgrounds/bg1")
        {
            Beatmap = beatmap;
            this.fallbackTextureName = fallbackTextureName;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Initialize();
        }

        protected virtual void Initialize() => Sprite.Texture = Beatmap?.Background ?? textures.Get(fallbackTextureName);
    }
}
