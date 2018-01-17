// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Platform;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.Mania.Configuration
{
    public class ManiaConfigManager : RulesetConfigManager<ManiaSetting>
    {
        public ManiaConfigManager(Ruleset ruleset, Storage storage)
            : base(ruleset, storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            Set(ManiaSetting.ScrollTime, 1500.0, 50.0, 10000.0, 50.0);
        }
    }

    public enum ManiaSetting
    {
        ScrollTime
    }
}
