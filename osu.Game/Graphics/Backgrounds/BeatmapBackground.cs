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

namespace osu.Game.Graphics.Backgrounds
{
    public partial class BeatmapBackground : Background
    {
        public readonly WorkingBeatmap Beatmap;

        private readonly string fallbackTextureName;

        private readonly Container letterboxContainer;

        private readonly Box topLetterbox;
        private readonly Box rightLetterbox;
        private readonly Box bottomLetterbox;
        private readonly Box leftLetterbox;

        private IBindable<BackgroundScalingMode> scalingMode = null!;

        public BeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName = @"Backgrounds/bg1")
        {
            Beatmap = beatmap;
            this.fallbackTextureName = fallbackTextureName;

            AddInternal(letterboxContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                Children = new Drawable[]
                {
                    topLetterbox = new Box
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black,
                    },
                    rightLetterbox = new Box
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black,
                    },
                    bottomLetterbox = new Box
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black,
                    },
                    leftLetterbox = new Box
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black,
                    },
                }
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

        protected override void Update()
        {
            base.Update();

            if (scalingMode.Value == BackgroundScalingMode.ScaleToFit)
                updateLetterboxDimensions();
        }

        private void updateDisplay()
        {
            switch (scalingMode.Value)
            {
                case BackgroundScalingMode.ScaleToFill:
                    Sprite.FillMode = FillMode.Fill;

                    letterboxContainer.Hide();
                    updateLetterboxDimensions(reset: true);
                    break;

                case BackgroundScalingMode.ScaleToFit:
                    Sprite.FillMode = FillMode.Fit;

                    letterboxContainer.Show();
                    updateLetterboxDimensions();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            BufferedContainer?.ForceRedraw();
        }

        private void updateLetterboxDimensions(bool reset = false)
        {
            float letterboxWidth = 0f;
            float letterboxHeight = 0f;

            if (!reset)
            {
                letterboxWidth = Math.Max(0f, (DrawWidth - Sprite.DrawWidth) / DrawWidth) / 2;
                letterboxHeight = Math.Max(0f, (DrawHeight - Sprite.DrawHeight) / DrawHeight) / 2;
            }

            topLetterbox.Height = letterboxHeight;
            rightLetterbox.Width = letterboxWidth;
            bottomLetterbox.Height = letterboxHeight;
            leftLetterbox.Width = letterboxWidth;
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
