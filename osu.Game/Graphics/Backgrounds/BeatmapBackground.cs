// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osuTK.Graphics;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class BeatmapBackground : Background
    {
        public readonly WorkingBeatmap Beatmap;

        private readonly string fallbackTextureName;

        private Bindable<BackgroundScaleMode> scaleMode { get; set; }

        private readonly Bindable<float> letterboxWidth = new Bindable<float>();

        public BeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName = @"Backgrounds/bg1")
        {
            Beatmap = beatmap;
            this.fallbackTextureName = fallbackTextureName;

            Box leftLetterbox;
            Box rightLetterbox;

            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    leftLetterbox = new Box
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    rightLetterbox = new Box
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    }
                }
            });

            letterboxWidth.BindValueChanged(margin =>
            {
                leftLetterbox.ResizeWidthTo(margin.NewValue / 2f);
                rightLetterbox.ResizeWidthTo(margin.NewValue / 2f);
            }, true);
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

                    letterboxWidth.Value = 0f;
                    break;

                case BackgroundScaleMode.ScaleToFit:
                    Sprite.FillMode = FillMode.Fit;

                    letterboxWidth.Value = (DrawWidth - Sprite.DrawWidth) / DrawWidth;
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
