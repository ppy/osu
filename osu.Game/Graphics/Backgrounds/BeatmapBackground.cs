// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Configuration;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class BeatmapBackground : Background
    {
        public readonly WorkingBeatmap Beatmap;

        private readonly string fallbackTextureName;

        private readonly Box blackBackground;

        private IBindable<BackgroundScalingMode> scalingMode = null!;

        public BeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName = @"Backgrounds/bg1")
        {
            Beatmap = beatmap;
            this.fallbackTextureName = fallbackTextureName;

            AddInternal(blackBackground = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue,
                FillMode = FillMode.Fill,
                Colour = Colour4.Black,
            });
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures, OsuConfigManager config)
        {
            Sprite.Texture = Beatmap?.GetBackground() ?? textures.Get(fallbackTextureName);
            scalingMode = config.GetBindable<BackgroundScalingMode>(OsuSetting.BackgroundScalingMode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scalingMode.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateDisplay()
        {
            switch (scalingMode.Value)
            {
                case BackgroundScalingMode.ScaleToFill:
                    Sprite.FillMode = FillMode.Fill;

                    blackBackground.Hide();
                    break;

                case BackgroundScalingMode.ScaleToFit:
                    Sprite.FillMode = FillMode.Fit;

                    blackBackground.Show();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
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
