// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacySpinnerApproachCircle : CompositeDrawable
    {
        private DrawableSpinner drawableSpinner;

        [CanBeNull]
        private Sprite approachCircle;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject, ISkinSource source)
        {
            drawableSpinner = (DrawableSpinner)drawableHitObject;

            AutoSizeAxes = Axes.Both;

            var spinnerProvider = source.FindProvider(s =>
                s.GetTexture("spinner-circle") != null ||
                s.GetTexture("spinner-top") != null);

            if (spinnerProvider is DefaultLegacySkin)
                return;

            InternalChild = approachCircle = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = source.GetTexture("spinner-approachcircle"),
                Scale = new Vector2(1.86f),
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
            switch (drawableHitObject)
            {
                case DrawableSpinner spinner:
                    using (BeginAbsoluteSequence(spinner.HitObject.StartTime))
                        approachCircle?.ScaleTo(0.1f, spinner.HitObject.Duration);

                    break;
            }
        }
    }
}
