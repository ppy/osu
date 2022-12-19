// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using System;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osu.Framework.Logging;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModWayback : ModWayback, IModifiesCursorMovement, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToBeatmap, IHidesCursorTrail
    {
        public override LocalisableString Description => "Is the cursor there yet?";
        public override Type[] IncompatibleMods => new Type[] { typeof(ModNoScope), typeof(OsuModAutopilot), typeof(OsuModMagnetised) };
        private Vector2 delayedMousePosition = Vector2.Zero;

        public Vector2 UpdatePosition(Vector2 truePosition, float deltaTime)
        {
            delayedMousePosition = Vector2.Lerp(delayedMousePosition, truePosition, deltaTime * FollowSpeed.Value); // We cap the lerp value to prevent the game from crashing
            return delayedMousePosition;
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> ruleset)
        {
            var osuPlayfield = (OsuPlayfield)ruleset.Playfield;
            osuPlayfield.Smoke.Alpha = 0;
        }
    }
}
