// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning
{
    /// <summary>
    /// Legacy skinned spinner with one main spinning layer and a background layer.
    /// </summary>
    public class LegacyOldStyleSpinner : CompositeDrawable
    {
        private DrawableSpinner drawableSpinner;
        private Sprite disc;
        private Container metre;

        private const float background_y_offset = 20;

        private const float sprite_scale = 1 / 1.6f;

        [BackgroundDependencyLoader]
        private void load(ISkinSource source, DrawableHitObject drawableObject)
        {
            drawableSpinner = (DrawableSpinner)drawableObject;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Sprite
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Texture = source.GetTexture("spinner-background"),
                    Y = background_y_offset,
                    Scale = new Vector2(sprite_scale)
                },
                disc = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = source.GetTexture("spinner-circle"),
                    Scale = new Vector2(sprite_scale)
                },
                metre = new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = background_y_offset,
                    Masking = true,
                    Child = new Sprite
                    {
                        Texture = source.GetTexture("spinner-metre"),
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                    },
                    Scale = new Vector2(0.625f)
                }
            };
        }

        private Vector2 metreFinalSize;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.FadeOut();
            drawableSpinner.State.BindValueChanged(updateStateTransforms, true);

            metreFinalSize = metre.Size = metre.Child.Size;
        }

        private void updateStateTransforms(ValueChangedEvent<ArmedState> state)
        {
            var spinner = drawableSpinner.HitObject;

            using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimeFadeIn / 2, true))
                this.FadeInFromZero(spinner.TimeFadeIn / 2);
        }

        protected override void Update()
        {
            base.Update();
            disc.Rotation = drawableSpinner.RotationTracker.Rotation;
            metre.Height = getMetreHeight(drawableSpinner.Progress);
        }

        private const int total_bars = 10;

        private float getMetreHeight(float progress)
        {
            progress = Math.Min(99, progress * 100);

            int barCount = (int)progress / 10;

            // todo: add SpinnerNoBlink support
            if (RNG.NextBool(((int)progress % 10) / 10f))
                barCount++;

            return (float)barCount / total_bars * metreFinalSize.Y;
        }
    }
}
