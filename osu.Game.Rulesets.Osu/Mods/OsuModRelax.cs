// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.States;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModRelax : ModRelax, IApplicableFailOverride, IUpdatableByPlayfield
    {
        public override string Description => @"You don't need to click. Give your clicking/tapping fingers a break from the heat of things.";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModAutopilot)).ToArray();

        public bool AllowFail => false;

        public void Update(Playfield playfield)
        {
            bool hitStill = false;
            bool hitOnce = false;

            const float relax_leniency = 3;

            foreach (var drawable in playfield.HitObjects.Objects)
            {
                if (!(drawable is DrawableOsuHitObject osuHit))
                    continue;

                double time = osuHit.Clock.CurrentTime;

                if (osuHit.IsAlive && time >= osuHit.HitObject.StartTime - relax_leniency)
                {
                    if (osuHit.HitObject is IHasEndTime hasEnd && time > hasEnd.EndTime || osuHit.IsHit)
                        continue;

                    hitStill |= osuHit is DrawableSlider slider && (slider.Ball.IsHovered || osuHit.IsHovered) || osuHit is DrawableSpinner;

                    hitOnce |= osuHit is DrawableHitCircle && osuHit.IsHovered;
                }
            }

            var osuHitSample = playfield.HitObjects.Objects.First(d => d is DrawableOsuHitObject) as DrawableOsuHitObject;
            if (hitOnce)
            {
                hit(osuHitSample, false);
                hit(osuHitSample, true);
            }
            hit(osuHitSample, hitStill);
        }

        private bool wasHit;
        private bool wasLeft;

        private void hit(DrawableOsuHitObject osuHit, bool hitting)
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
            osuHit.OsuActionInputManager.HandleCustomInput(new InputState(), state);
        }
    }
}
