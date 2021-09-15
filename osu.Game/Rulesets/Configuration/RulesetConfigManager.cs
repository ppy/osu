// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Configuration;
using osu.Game.Database;

namespace osu.Game.Rulesets.Configuration
{
    public abstract class RulesetConfigManager<TLookup> : DatabasedConfigManager<TLookup>, IRulesetConfigManager
        where TLookup : struct, Enum
    {
        protected RulesetConfigManager(RealmContextFactory realmFactory, RulesetInfo ruleset, int? variant = null)
            : base(realmFactory, ruleset, variant)
        {
        }
    }
}
