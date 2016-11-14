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
        public abstract ScoreOverlay CreateScoreOverlay();

        public abstract HitRenderer CreateHitRendererWith(List<HitObject> objects);

        public static Ruleset GetRuleset(PlayMode mode)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                                .Where(a => a.FullName.Contains($@"osu.Game.Modes.{mode}"))
                                .SelectMany(a => a.GetTypes())
                                .Where(t => t.Name == $@"{mode}Ruleset")
                                .FirstOrDefault();

            if (type == null)
                return null;

            return Activator.CreateInstance(type) as Ruleset;
        }
    }
}
