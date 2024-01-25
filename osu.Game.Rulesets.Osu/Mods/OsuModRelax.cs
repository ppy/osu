// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModRelax : ModRelax, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToPlayer, IHasNoTimedInputs
    {
        public override LocalisableString Description => @"You don't need to click. Give your clicking/tapping fingers a break from the heat of things.";

        public override Type[] IncompatibleMods =>
            base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot), typeof(OsuModMagnetised), typeof(OsuModAlternate), typeof(OsuModSingleTap) }).ToArray();

        /// <summary>
        /// How early before a hitobject's start time to trigger a hit.
        /// </summary>
        private const float relax_leniency = 3;

        private bool isDownState;
        private bool wasLeft;

        private OsuInputManager osuInputManager = null!;

        private ReplayState<OsuAction> state = null!;
        private double lastStateChangeTime;

        private bool hasReplay;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // grab the input manager for future use.
            osuInputManager = ((DrawableOsuRuleset)drawableRuleset).KeyBindingInputManager;
        }

        public void ApplyToPlayer(Player player)
        {
            if (osuInputManager.ReplayInputHandler != null)
            {
                hasReplay = true;
                return;
            }

            osuInputManager.AllowGameplayInputs = false;
        }

        public void Update(Playfield playfield)
        {
            if (hasReplay)
                return;

            bool requiresHold = false;
            bool requiresHit = false;

            double time = playfield.Clock.CurrentTime;

            foreach (var h in playfield.HitObjectContainer.AliveObjects.OfType<DrawableOsuHitObject>())
            {
                // we are not yet close enough to the object.
                if (time < h.HitObject.StartTime - relax_leniency)
                    break;

                // already hit or beyond the hittable end time.
                if (h.IsHit || (h.HitObject is IHasDuration hasEnd && time > hasEnd.EndTime))
                    continue;

                switch (h)
                {
                    case DrawableHitCircle circle:
                        handleHitCircle(circle);
                        break;

                    case DrawableSlider slider:
                        // Handles cases like "2B" beatmaps, where sliders may be overlapping and simply holding is not enough.
                        if (!slider.HeadCircle.IsHit)
                            handleHitCircle(slider.HeadCircle);

                        requiresHold |= slider.SliderInputManager.IsMouseInFollowArea(true);
                        break;

                    case DrawableSpinner spinner:
                        requiresHold |= spinner.HitObject.SpinsRequired > 0;
                        break;
                }
            }

            if (requiresHit)
            {
                changeState(false);
                changeState(true);
            }

            if (requiresHold)
                changeState(true);
            else if (isDownState && time - lastStateChangeTime > AutoGenerator.KEY_UP_DELAY)
                changeState(false);

            void handleHitCircle(DrawableHitCircle circle)
            {
                if (!circle.HitArea.IsHovered)
                    return;

                Debug.Assert(circle.HitObject.HitWindows != null);
                requiresHit |= circle.HitObject.HitWindows.CanBeHit(time - circle.HitObject.StartTime);
            }

            void changeState(bool down)
            {
                if (isDownState == down)
                    return;

                isDownState = down;
                lastStateChangeTime = time;

                state = new ReplayState<OsuAction>
                {
                    PressedActions = new List<OsuAction>()
                };

                if (down)
                {
                    state.PressedActions.Add(wasLeft ? OsuAction.LeftButton : OsuAction.RightButton);
                    wasLeft = !wasLeft;
                }

                state.Apply(osuInputManager.CurrentState, osuInputManager);
            }
        }
    }
}
