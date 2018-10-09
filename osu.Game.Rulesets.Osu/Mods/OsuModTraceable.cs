// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModTraceable : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Traceable";
        public override string ShortenedName => "TC";
        public override FontAwesome Icon => FontAwesome.fa_snapchat_ghost;
        public override ModType Type => ModType.Fun;
        public override string Description => "Put your faith in the approach circles...";
        public override double ScoreMultiplier => 1;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
            {
                if (drawable is DrawableHitCircle c)
                    c.HideButApproachCircle();
                if (drawable is DrawableSlider s)
                    s.HeadCircle.HideButApproachCircle();
            }
        }
    }
}
