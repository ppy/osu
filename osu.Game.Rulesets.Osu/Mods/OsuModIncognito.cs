// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModIncognito : ModIncognito, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToDrawableHitObject
    {
        [SettingSource("Disable follow points", "The lines... where are they?")]
        public BindableBool DisableFollowPoints { get; } = new BindableBool();

        [SettingSource("No combo colours", "The combo colours won't tell you anything now...")]
        public BindableBool NoComboColours { get; } = new BindableBool();

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            if (DisableFollowPoints.Value)
                ((DrawableOsuRuleset)drawableRuleset).Playfield.FollowPoints.Hide();
        }

        public void ApplyToDrawableHitObject(DrawableHitObject d)
        {
            if (NoComboColours.Value)
                d.OnUpdate += _ => d.AccentColour.Value = Color4.White;
        }
    }
}
