// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModVisualAdjusts : ModVisualAdjusts<OsuHitObject, DrawableOsuRuleset>
    {
        [SettingSource("Disable follow points", "No more hints for where to follow...")]
        public DrawableRulesetVisualAdjustSetting DisableFollowPoints { get; } = new DrawableRulesetVisualAdjustSetting(ruleset => ruleset.Playfield.FollowPoints.Hide());

        [SettingSource("No combo colours", "The combo colours won't tell you anything now...")]
        public HitObjectVisibilityVisualAdjustSetting NoComboColours { get; }

        private void applyNoComboColours(DrawableHitObject dho) => dho.AccentColour.Value = Colour4.White;

        public OsuModVisualAdjusts()
        {
            NoComboColours = new HitObjectVisibilityVisualAdjustSetting(applyNoComboColours, applyNoComboColours);
        }
    }
}
