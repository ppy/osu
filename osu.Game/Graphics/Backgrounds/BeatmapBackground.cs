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

        private Bindable<BackgroundFillMode> fillMode { get; set; }

        public BeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName = @"Backgrounds/bg1")
        {
            Beatmap = beatmap;
            this.fallbackTextureName = fallbackTextureName;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures, OsuConfigManager config)
        {
            fillMode = config.GetBindable<BackgroundFillMode>(OsuSetting.BackgroundFillMode);

            updateBackgroundFillMode();

            Sprite.Texture = Beatmap?.GetBackground() ?? textures.Get(fallbackTextureName);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            fillMode.ValueChanged += _ => updateBackgroundFillMode();
        }

        private void updateBackgroundFillMode()
        {
            switch (fillMode.Value)
            {
                case BackgroundFillMode.StretchToFill:
                    Sprite.FillMode = FillMode.Stretch;
                    break;

                case BackgroundFillMode.ScaleToFill:
                    Sprite.FillMode = FillMode.Fill;
                    break;

                case BackgroundFillMode.ScaleToFit:
                    Sprite.FillMode = FillMode.Fit;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(fillMode), fillMode, null);
            }
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
