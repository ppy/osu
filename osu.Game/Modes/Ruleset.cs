//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Modes.UI;
using System.Reflection;
using osu.Framework.Extensions;
using System;
using System.Linq;

namespace osu.Game.Modes
{
    public abstract class Ruleset
    {
        private static List<Type> availableRulesets = new List<Type>();

        public abstract ScoreOverlay CreateScoreOverlay();

        public abstract HitRenderer CreateHitRendererWith(List<HitObject> objects);

        public abstract HitObjectParser CreateHitObjectParser();

        public static void Register(Ruleset ruleset) => availableRulesets.Add(ruleset.GetType());

        public static Ruleset GetRuleset(PlayMode mode)
        {
            Type type = availableRulesets.FirstOrDefault(t => t.Name == $@"{mode}Ruleset");

            if (type == null)
                return null;

            return Activator.CreateInstance(type) as Ruleset;
        }
    }
}
