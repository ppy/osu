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

        /// <summary>
        /// How early before a hitobject's start time to trigger a hit.
        /// </summary>
        private const float relax_leniency = 3;

        public void Update(Playfield playfield)
        {
            bool requiresHold = false;
            bool requiresHit = false;

            double time = playfield.Clock.CurrentTime;

            foreach (var h in playfield.HitObjectContainer.AliveObjects.OfType<DrawableOsuHitObject>())
            {
                // we are not yet close enough to the object.
                if (time < h.HitObject.StartTime - relax_leniency)
                    break;

                // already hit or beyond the hittable end time.
                if (h.IsHit || (h.HitObject is IHasEndTime hasEnd && time > hasEnd.EndTime))
                    continue;

                switch (h)
                {
                    case DrawableHitCircle _:
                        if (!h.IsHovered)
                            break;

                        Debug.Assert(h.HitObject.HitWindows != null);
                        requiresHit |= h.HitObject.HitWindows.CanBeHit(time - h.HitObject.StartTime);
                        break;

                    case DrawableSlider slider:
                        requiresHold |= slider.Ball.IsHovered || h.IsHovered;
                        break;

                    case DrawableSpinner _:
                        requiresHold = true;
                        break;
                }
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
