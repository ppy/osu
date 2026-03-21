// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModBadApple : Mod, IApplicableToDrawableHitObject, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield
    {
        public override string Name => "Bad Apple";
        public override LocalisableString Description => "Dodge the beat!";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "BA";
        public override Type[] IncompatibleMods => new[] { typeof(CatchModAutoplay) };

        private Catcher catcher = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            catcher = ((CatchPlayfield)drawableRuleset.Playfield).Catcher;

            // Don't show caught fruits as they aren't technically being caught.
            catcher.CatchFruitOnPlate = false;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableCatchHitObject catchHitObject)
            {
                catchHitObject.CheckPosition = hitObject => !catcher.CanCatch(hitObject);
            }
        }

        public void Update(Playfield playfield)
        {
            // Block hyperdashing to avoid hyperdashes when two objects appear at the same time.
            catcher.SetHyperDashState(1, -1);
        }
    }
}
