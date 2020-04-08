// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Tau.Objects;
using osu.Game.Rulesets.Tau.Objects.Drawables;
using osu.Game.Rulesets.Tau.UI;
using osu.Game.Rulesets.UI;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.Tau.Mods
{
    public class TauModRelax : ModRelax, IUpdatableByPlayfield, IApplicableToDrawableRuleset<TauHitObject>
    {
        public override string Description => @"You don't need to click. Give your clicking/tapping fingers a break from the heat of things.";

        public void Update(Playfield playfield)
        {
            bool requiresHold = false;
            bool requiresHit = false;

            const float relax_leniency = 3;

            foreach (var drawable in playfield.HitObjectContainer.AliveObjects)
            {
                if (!(drawable is DrawabletauHitObject tauHit))
                    continue;

                double time = tauHit.Clock.CurrentTime;
                double relativetime = time - tauHit.HitObject.StartTime;

                if (time < tauHit.HitObject.StartTime + relax_leniency) continue;

                if (tauHit.HitObject is IHasEndTime hasEnd && time > hasEnd.EndTime || tauHit.IsHit)
                    continue;

                if (tauHit is DrawabletauHitObject)
                {
                    Debug.Assert(tauHit.HitObject.HitWindows != null);

                    var play = (TauPlayfield)playfield;
                    if (tauHit.HitObject.HitWindows.CanBeHit(relativetime) && play.CheckIfWeCanValidate(tauHit))
                        requiresHit = true;
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

        private TauInputManager tauInputManager;

        private void addAction(bool hitting)
        {
            if (wasHit == hitting)
                return;

            wasHit = hitting;

            var state = new ReplayState<TauAction>
            {
                PressedActions = new List<TauAction>()
            };

            if (hitting)
            {
                state.PressedActions.Add(wasLeft ? TauAction.LeftButton : TauAction.RightButton);
                wasLeft = !wasLeft;
            }

            state.Apply(tauInputManager.CurrentState, tauInputManager);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TauHitObject> drawableRuleset)
        {
            // grab the input manager for future use.
            tauInputManager = (TauInputManager)drawableRuleset.KeyBindingInputManager;
            tauInputManager.AllowUserPresses = false;
        }
    }
}
