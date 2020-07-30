// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;

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

        private DrawableSpinner drawableSpinner;

        [BackgroundDependencyLoader]
        private void load(ISkinSource source, DrawableHitObject drawableObject)
        {
            drawableSpinner = (DrawableSpinner)drawableObject;

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
                new Sprite
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

            this.FadeOut();
            drawableSpinner.State.BindValueChanged(updateStateTransforms, true);
        }

        private void updateStateTransforms(ValueChangedEvent<ArmedState> state)
        {
            var spinner = drawableSpinner.HitObject;

            using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimePreempt / 2, true))
                this.FadeInFromZero(spinner.TimePreempt / 2);
        }

        protected override void Update()
        {
            base.Update();
            spinningMiddle.Rotation = discTop.Rotation = drawableSpinner.RotationTracker.Rotation;
            discBottom.Rotation = discTop.Rotation / 3;
        }
    }
}
