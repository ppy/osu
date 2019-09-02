// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModRelax : ModRelax, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Description => @"You don't need to click. Give your clicking/tapping fingers a break from the heat of things.";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModAutopilot)).ToArray();

        public void Update(Playfield playfield)
        {
            bool requiresHold = false;
            bool requiresHit = false;

            const float relax_leniency = 3;

            foreach (var drawable in playfield.HitObjectContainer.AliveObjects)
            {
                if (!(drawable is DrawableOsuHitObject osuHit))
                    continue;

                double time = osuHit.Clock.CurrentTime;
                double relativetime = time - osuHit.HitObject.StartTime;

                if (time < osuHit.HitObject.StartTime - relax_leniency) continue;

                if ((osuHit.HitObject is IHasEndTime hasEnd && time > hasEnd.EndTime) || osuHit.IsHit)
                    continue;

                if (osuHit is DrawableHitCircle && osuHit.IsHovered)
                {
                    Debug.Assert(osuHit.HitObject.HitWindows != null);
                    requiresHit |= osuHit.HitObject.HitWindows.CanBeHit(relativetime);
                }

                requiresHold |= (osuHit is DrawableSlider slider && (slider.Ball.IsHovered || osuHit.IsHovered)) || osuHit is DrawableSpinner;
            }

            if (requiresHit)
            {
                addAction(false);
                addAction(true);
            }

            addAction(requiresHold);
        }

        private bool wasHit;
        private bool wasLeft;

        private OsuInputManager osuInputManager;

        private void addAction(bool hitting)
        {
            if (wasHit == hitting)
                return;

            wasHit = hitting;

            var state = new ReplayState<OsuAction>
            {
                PressedActions = new List<OsuAction>()
            };

            if (hitting)
            {
                state.PressedActions.Add(wasLeft ? OsuAction.LeftButton : OsuAction.RightButton);
                wasLeft = !wasLeft;
            }

            state.Apply(osuInputManager.CurrentState, osuInputManager);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // grab the input manager for future use.
            osuInputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
            osuInputManager.AllowUserPresses = false;
        }
    }
}
