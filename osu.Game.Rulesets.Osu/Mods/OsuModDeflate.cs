// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDeflate : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Deflate";
        public override string ShortenedName => "DF";
        public override FontAwesome Icon => FontAwesome.fa_compress;
        public override ModType Type => ModType.Fun;
        public override string Description => "Become one with the approach circle...";
        public override double ScoreMultiplier => 1;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
                drawable.ApplyCustomUpdateState += drawableOnApplyCustomUpdateState;
        }

        protected void drawableOnApplyCustomUpdateState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableHitCircle d))
                return;
            d.ApproachCircle.Hide();
            var h = d.HitObject;
            using (d.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
            {
                var origScale = d.Scale;
                d.ScaleTo(1.1f, 1)      // if duration = 0 then components (i.e. flash) scale with it -> we don't want that
                    .Then()
                    .ScaleTo(origScale, h.TimePreempt)
                    .Then()
                    .ScaleTo(d.Scale * 1.5f, 400, Easing.OutQuad);  // reapply overwritten ScaleTo
            }
        }
    }
}
