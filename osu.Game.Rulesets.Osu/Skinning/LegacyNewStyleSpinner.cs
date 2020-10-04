// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    /// <summary>
    /// Legacy skinned spinner with two main spinning layers, one fixed overlay and one final spinning overlay.
    /// No background layer.
    /// </summary>
    public class LegacyNewStyleSpinner : CompositeDrawable
    {
        private Sprite discBottom;
        private Sprite discTop;
        private Sprite spinningMiddle;
        private Sprite fixedMiddle;

        private DrawableSpinner drawableSpinner;

        private const float final_scale = 0.625f;

        [BackgroundDependencyLoader]
        private void load(ISkinSource source, DrawableHitObject drawableObject)
        {
            drawableSpinner = (DrawableSpinner)drawableObject;

            Scale = new Vector2(final_scale);

            InternalChildren = new Drawable[]
            {
                discBottom = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = source.GetTexture("spinner-bottom")
                },
                discTop = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = source.GetTexture("spinner-top")
                },
                fixedMiddle = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = source.GetTexture("spinner-middle")
                },
                spinningMiddle = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = source.GetTexture("spinner-middle2")
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            drawableSpinner.ApplyCustomUpdateState += updateStateTransforms;
            updateStateTransforms(drawableSpinner, drawableSpinner.State.Value);
        }

        private void updateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            if (!(drawableHitObject is DrawableSpinner))
                return;

            var spinner = (Spinner)drawableSpinner.HitObject;

            using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimePreempt, true))
                this.FadeOut();

            using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimeFadeIn / 2, true))
                this.FadeInFromZero(spinner.TimeFadeIn / 2);

            using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimePreempt, true))
            {
                fixedMiddle.FadeColour(Color4.White);

                using (BeginDelayedSequence(spinner.TimePreempt, true))
                    fixedMiddle.FadeColour(Color4.Red, spinner.Duration);
            }
        }

        protected override void Update()
        {
            base.Update();
            spinningMiddle.Rotation = discTop.Rotation = drawableSpinner.RotationTracker.Rotation;
            discBottom.Rotation = discTop.Rotation / 3;

            Scale = new Vector2(final_scale * (0.8f + (float)Interpolation.ApplyEasing(Easing.Out, drawableSpinner.Progress) * 0.2f));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableSpinner != null)
                drawableSpinner.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
