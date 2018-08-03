// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.States;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModRelax : ModRelax, IApplicableFailOverride, IUpdatableByHitObject, IUpdatableByPlayfield
    {
        public override string Description => @"You don't need to click. Give your clicking/tapping fingers a break from the heat of things.";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModAutopilot)).ToArray();

        public bool AllowFail => false;

        private bool hitStill;
        private bool hitOnce;

        public void Update(DrawableHitObject drawable)
        {
            const float relax_leniency = 3;

            if (!(drawable is DrawableOsuHitObject osuHit))
                return;

            double time = osuHit.Clock.CurrentTime;

            if (time >= osuHit.HitObject.StartTime - relax_leniency)
            {
                if (osuHit.HitObject is IHasEndTime hasEnd && time > hasEnd.EndTime || osuHit.IsHit)
                    return;

                hitStill |= osuHit is DrawableSlider slider && (slider.Ball.IsHovered || osuHit.IsHovered) || osuHit is DrawableSpinner;

                hitOnce |= osuHit is DrawableHitCircle && osuHit.IsHovered;
            }
        }

        public void Update(Playfield playfield)
        {
            var osuHit = playfield.HitObjects.Objects.First(d => d is DrawableOsuHitObject) as DrawableOsuHitObject;
            if (hitOnce)
            {
                hit(osuHit, false);
                hit(osuHit, true);
            }
            hit(osuHit, hitStill);

            hitOnce = false;
            hitStill = false;
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
