// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Configuration;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class BeatmapBackground : Background
    {
        public readonly WorkingBeatmap Beatmap;

        private readonly string fallbackTextureName;

        private Bindable<BackgroundScaleMode> scaleMode { get; set; }

        public BeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName = @"Backgrounds/bg1")
        {
            Beatmap = beatmap;
            this.fallbackTextureName = fallbackTextureName;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures, OsuConfigManager config)
        {
            scaleMode = config.GetBindable<BackgroundScaleMode>(OsuSetting.BackgroundScaleMode);

            Sprite.Texture = Beatmap?.GetBackground() ?? textures.Get(fallbackTextureName);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scaleMode.BindValueChanged(_ => updateBackgroundScaleMode(), true);
        }

        private void updateBackgroundScaleMode()
        {
            switch (scaleMode.Value)
            {
                case BackgroundScaleMode.ScaleToFill:
                    Sprite.FillMode = FillMode.Fill;
                    break;

                case BackgroundScaleMode.ScaleToFit:
                    Sprite.FillMode = FillMode.Fit;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(scaleMode), scaleMode, null);
            }

            BufferedContainer?.ForceRedraw();
        }

        public override bool Equals(Background other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType()
                   && ((BeatmapBackground)other).Beatmap == Beatmap;
        }
    }
}
