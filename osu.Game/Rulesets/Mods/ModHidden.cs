// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHidden : Mod, IReadFromConfig
    {
        public override string Name => "Hidden";
        public override string ShortenedName => "HD";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override bool Ranked => true;

        protected Bindable<bool> IncreaseFirstObjectVisibility = new Bindable<bool>();

        public void ReadFromConfig(OsuConfigManager config)
        {
            IncreaseFirstObjectVisibility = config.GetBindable<bool>(OsuSetting.IncreaseFirstObjectVisibility);
        }

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables)
            {
                if (d == drawables.Last() && IncreaseFirstObjectVisibility)
                    return;

                d.ApplyCustomUpdateState += ApplyHiddenState;
				d.HitObject.TimeFadein = d.HitObject.TimePreempt * fade_in_duration_multiplier;
            }
        }

        protected virtual void ApplyHiddenState(DrawableHitObject hitObject, ArmedState state) { }
    }
}
