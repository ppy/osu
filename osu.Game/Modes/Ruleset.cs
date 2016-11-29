//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Modes.UI;
using System.Reflection;
using osu.Framework.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace osu.Game.Modes
{
    public abstract class Ruleset
    {
        private static ConcurrentDictionary<PlayMode, Type> availableRulesets = new ConcurrentDictionary<PlayMode, Type>();

        public abstract ScoreOverlay CreateScoreOverlay();

        public abstract ScoreProcessor CreateScoreProcessor();

        public abstract HitRenderer CreateHitRendererWith(List<HitObject> objects);

        public abstract HitObjectParser CreateHitObjectParser();

        public static void Register(Ruleset ruleset) => availableRulesets.TryAdd(ruleset.PlayMode, ruleset.GetType());

        protected abstract PlayMode PlayMode { get; }

        public static Ruleset GetRuleset(PlayMode mode)
        {
            Type type;

            if (!availableRulesets.TryGetValue(mode, out type))
                return null;

            return Activator.CreateInstance(type) as Ruleset;
        }
    }
}
