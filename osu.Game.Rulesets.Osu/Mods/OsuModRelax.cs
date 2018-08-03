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

        public void Update(DrawableHitObject o)
        {
            const float relax_leniency = 3;

            if (!(o is DrawableOsuHitObject d))
                return;

            double t = d.Clock.CurrentTime;

            if (t >= d.HitObject.StartTime - relax_leniency)
            {
                if (d.HitObject is IHasEndTime e && t > e.EndTime || d.IsHit)
                    return;

                hitStill |= d is DrawableSlider s && (s.Ball.IsHovered || d.IsHovered) || d is DrawableSpinner;

                hitOnce |= d is DrawableHitCircle && d.IsHovered;
            }
        }

        public void Update(Playfield r)
        {
            var d = r.HitObjects.Objects.First(h => h is DrawableOsuHitObject) as DrawableOsuHitObject;
            if (hitOnce)
            {
                hit(d, false);
                hit(d, true);
            }
            hit(d, hitStill);

            hitOnce = false;
            hitStill = false;
        }

        private bool wasHit;
        private bool wasLeft;

        private void hit(DrawableOsuHitObject d, bool hitting)
        {
            if (wasHit == hitting)
                return;
            wasHit = hitting;

            var l = new ReplayState<OsuAction>
            {
                PressedActions = new List<OsuAction>()
            };
            if (hitting)
            {
                l.PressedActions.Add(wasLeft ? OsuAction.LeftButton : OsuAction.RightButton);
                wasLeft = !wasLeft;
            }
            d.OsuActionInputManager.HandleCustomInput(new InputState(), l);
        }
    }
}
