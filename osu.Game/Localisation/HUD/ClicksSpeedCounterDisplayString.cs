// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class ClicksSpeedCounterDisplayString
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.ClicksSpeedDisplayString";

        public static LocalisableString ClicksSpeedDisplay => new TranslatableString(getKey(@"clicks_speed_display"), "Clicks Speed Unit");

        public static LocalisableString ClicksSpeedDisplayDescription => new TranslatableString(getKey(@"clicks_speed_display_description"), "Which unit should be used to display clicks speed.");

        public static LocalisableString ClicksSpeedDisplayUnitBpm => new TranslatableString(getKey(@"clicks_speed_display_unit_bpm"), "BPM");

        public static LocalisableString ClicksSpeedDisplayUnitCps => new TranslatableString(getKey(@"clicks_speed_display_unit_cps"), "Clicks per second");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
