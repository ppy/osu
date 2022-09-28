// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.Osu.Mods
{
    public interface IOsuModRelaxExtensions : IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToPlayer
    {
        bool IsDownState { get; set; }
        bool WasLeft { get; set; }
        OsuAction LeftAction { get; }
        OsuAction RightAction { get; }

        OsuInputManager InputManager { get; set; }

        ReplayState<OsuAction> State { get; set; }
        double LastStateChangeTime { get; set; }

        bool HasReplay { get; set; }
    }

    public static class OsuModRelaxExtensions
    {
        /// <summary>
        /// How early before a hitobject's start time to trigger a hit.
        /// </summary>
        private const float relax_leniency = 3;

        public static void ApplyToDrawableRuleset(this IOsuModRelaxExtensions mod, DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // grab the input manager for future use.
            mod.InputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;
        }

        public static void ApplyToPlayer(this IOsuModRelaxExtensions mod, Player player)
        {
            if (mod.InputManager.ReplayInputHandler != null)
            {
                mod.HasReplay = true;
                return;
            }

            if (mod is ModRelax)
                mod.InputManager.AllowUserPresses = false;
        }

        public static void Update(this IOsuModRelaxExtensions mod, Playfield playfield)
        {
            if (mod.HasReplay)
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
                if ((mod is ModRelax ? h.IsHit : h.ResultDisplayed) || (h.HitObject is IHasDuration hasEnd && time > hasEnd.EndTime))
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

                        requiresHold |= slider.Ball.IsHovered || h.IsHovered;
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
            else if (mod.IsDownState && time - mod.LastStateChangeTime > AutoGenerator.KEY_UP_DELAY)
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
                if (mod.IsDownState == down)
                    return;

                mod.IsDownState = down;
                mod.LastStateChangeTime = time;

                mod.State = new ReplayState<OsuAction>
                {
                    PressedActions = new List<OsuAction>()
                };

                if (down)
                {
                    mod.State.PressedActions.Add(mod.WasLeft ? mod.LeftAction : mod.RightAction);
                    mod.WasLeft = !mod.WasLeft;
                }

                mod.State.Apply(mod.InputManager.CurrentState, mod.InputManager);
            }
        }
    }

    public class OsuModRelax : ModRelax, IOsuModRelaxExtensions
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot), typeof(OsuModMagnetised), typeof(OsuModAlternate), typeof(OsuModSingleTap) }).ToArray();
        public override LocalisableString Description => @"You don't need to click. Give your clicking/tapping fingers a break from the heat of things.";

        public bool IsDownState { get; set; }
        public bool WasLeft { get; set; }
        public OsuInputManager InputManager { get; set; } = null!;
        public ReplayState<OsuAction> State { get; set; } = null!;
        public double LastStateChangeTime { get; set; }
        public bool HasReplay { get; set; }

        // These are required because these methods fulfill other interfaces.
        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset) => OsuModRelaxExtensions.ApplyToDrawableRuleset(this, drawableRuleset);
        public void ApplyToPlayer(Player player) => OsuModRelaxExtensions.ApplyToPlayer(this, player);
        public void Update(Playfield playfield) => OsuModRelaxExtensions.Update(this, playfield);

        public OsuAction LeftAction => OsuAction.LeftButton;
        public OsuAction RightAction => OsuAction.RightButton;
    }

    public class OsuModRelaxDisplay : ModRelaxDisplay, IOsuModRelaxExtensions, IApplicableToDrawableHitObject
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot), typeof(OsuModMagnetised), typeof(OsuModAlternate), typeof(OsuModSingleTap) }).ToArray();
        public override LocalisableString Description => @"It looks like you have relax on, but your clicks are still scored. Just focus on your aim - and make sure to tap perfectly.";

        public bool IsDownState { get; set; }
        public bool WasLeft { get; set; }
        public OsuInputManager InputManager { get; set; } = null!;
        public ReplayState<OsuAction> State { get; set; } = null!;
        public double LastStateChangeTime { get; set; }
        public bool HasReplay { get; set; }

        // These are required because these methods fulfill other interfaces.
        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset) => OsuModRelaxExtensions.ApplyToDrawableRuleset(this, drawableRuleset);
        public void ApplyToPlayer(Player player) => OsuModRelaxExtensions.ApplyToPlayer(this, player);
        public void Update(Playfield playfield) => OsuModRelaxExtensions.Update(this, playfield);

        public OsuAction LeftAction => OsuAction.UnscoredLeftButton;
        public OsuAction RightAction => OsuAction.UnscoredRightButton;

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (!HasReplay)
                drawable.DisplayScoredResult = false;
        }
    }
}
