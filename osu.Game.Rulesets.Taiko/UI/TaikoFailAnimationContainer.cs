// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class TaikoFailAnimationContainer : FailAnimationContainer
    {
        public TaikoFailAnimationContainer(DrawableTaikoRuleset ruleset)
            : base(ruleset)
        {
        }

        protected override TransformSequence<DrawableHitObject> CreateHitObjectTransforms(DrawableHitObject hitObject, float rotation)
        {
            Vector2 originalPosition = hitObject.Position;
            Vector2 originalScale = hitObject.Scale;

            return hitObject.RotateTo(rotation, DURATION)
                            .ScaleTo(originalScale * 0.5f, DURATION)
                            .FadeOutFromOne(DURATION)
                            .MoveTo(originalPosition + new Vector2(0, 200), DURATION);
        }
    }
}
