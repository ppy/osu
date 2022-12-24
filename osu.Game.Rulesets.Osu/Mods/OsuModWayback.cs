// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using System;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModWayback : ModWayback, IModifiesCursorMovement, IApplicableToDrawableRuleset<OsuHitObject>, IHidesCursorTrail
    {
        public override LocalisableString Description => "Is the cursor there yet?";
        public override Type[] IncompatibleMods => new Type[] { typeof(ModNoScope), typeof(OsuModAutopilot), typeof(OsuModMagnetised) };
        public struct OsuWaybackMouseSnapshot
        {
            public float DeltaTime;
            public Vector2 Position;
        }
        private float deltaTimeAccumulator = 0f;
        private float timeSinceStart = 0f;
        private Queue<OsuWaybackMouseSnapshot> snapshots = new();

        public Vector2 UpdatePosition(Vector2 truePosition, float deltaTime)
        {
            snapshots.Enqueue(new()
            {
                DeltaTime = deltaTime,
                Position = truePosition
            });
            timeSinceStart += deltaTime;
            if (timeSinceStart > Delay.Value)
            {
                deltaTimeAccumulator += deltaTime;
                while (deltaTimeAccumulator > snapshots.Peek().DeltaTime)
                {
                    deltaTimeAccumulator -= snapshots.Dequeue().DeltaTime;
                }
            }
            return snapshots.Peek().Position;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> ruleset)
        {
            var osuPlayfield = (OsuPlayfield)ruleset.Playfield;
            osuPlayfield.Smoke.Alpha = 0;
        }
    }
}
