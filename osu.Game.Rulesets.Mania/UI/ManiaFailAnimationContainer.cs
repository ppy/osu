// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public partial class ManiaFailAnimationContainer : FailAnimationContainer
    {
        public ManiaFailAnimationContainer(DrawableManiaRuleset ruleset)
            : base(ruleset)
        {
        }

        protected override void CreateHitObjectTransforms(DrawableHitObject hitObject, float rotation)
        {
            hitObject.RotateTo(rotation, DURATION)
                     .ScaleTo(hitObject.Scale * 0.5f, DURATION);
        }
    }
}
