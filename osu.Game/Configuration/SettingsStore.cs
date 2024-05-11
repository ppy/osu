// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;

namespace osu.Game.Configuration
{
    public class SettingsStore
    {
        // this class mostly exists as a wrapper to avoid breaking the ruleset API (see usage in RulesetConfigManager).
        // it may cease to exist going forward, depending on how the structure of the config data layer changes.

        public readonly RealmAccess Realm;

        public SettingsStore(RealmAccess realm)
        {
            Realm = realm;
        }
    }
}
