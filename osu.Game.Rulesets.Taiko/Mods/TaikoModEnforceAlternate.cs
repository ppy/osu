// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModEnforceAlternate : ModEnforceAlternate, IApplicableToDrawableRuleset<TaikoHitObject>, IApplicableToHealthProcessor, IUpdatableByPlayfield
    {
        public override IconUsage? Icon => FontAwesome.Solid.SignLanguage;

        private HealthProcessor healthProcessor;

        private MultiAction blockedSide;
        private MultiAction blockedRim;
        private MultiAction blockedCentre;

        private int blockedKeystrokes;

        private DrawableTaikoHitObject nextHitObject;
        private bool? objectInRange;

        [SettingSource("Playstyle")]
        public Bindable<Playstyle> KeyConfiguration { get; } = new Bindable<Playstyle>
        {
            Default = Playstyle.KDDK,
            Value = Playstyle.KDDK
        };

        [SettingSource("Allow repeats on color change")]
        public Bindable<bool> RepeatOnColorChange { get; } = new BindableBool
        {
            Default = true,
            Value = true,
            Disabled = true
        };

        [SettingSource("Allow repeats after drumrolls")]
        public Bindable<bool> RepeatAfterDrumroll { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        [SettingSource("Allow repeats after large notes")]
        public Bindable<bool> RepeatAfterLargeNote { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        public TaikoModEnforceAlternate()
        {
            KeyConfiguration.ValueChanged += ev => RepeatOnColorChange.Disabled = ev.NewValue == Playstyle.KDDK;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            var inputManager = (TaikoInputManager)drawableRuleset.KeyBindingInputManager;
            inputManager.OnRulesetAction += handleBindings;
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            this.healthProcessor = healthProcessor;

            if (CauseFail.Value)
                healthProcessor.FailConditions += FailCondition;
        }

        protected bool FailCondition(HealthProcessor healthProcessor, JudgementResult result) => blockedKeystrokes > 0;

        public void Update(Playfield playfield)
        {
            if (RepeatAfterDrumroll.Value && nextHitObject is DrawableDrumRoll roll && roll.AllJudged)
            {
                if (roll.HitObject.IsStrong)
                {
                    blockedRim = MultiAction.None;
                    blockedCentre = MultiAction.None;
                }
                else
                    blockedCentre = MultiAction.None;

                blockedSide = MultiAction.None;
            }

            nextHitObject = playfield.HitObjectContainer.Objects.ToList().Find(o => o is DrawableTaikoHitObject && !o.AllJudged) as DrawableTaikoHitObject;

            if (nextHitObject is DrawableDrumRoll drumRoll)
                objectInRange = playfield.Time.Current >= drumRoll.HitObject.StartTime && playfield.Time.Current <= drumRoll.HitObject.EndTime;
            else
                objectInRange = nextHitObject?.HitObject.HitWindows.CanBeHit(playfield.Time.Current - nextHitObject.HitObject.StartTime);
        }

        private bool handleMultipleKeypresses(List<MultiAction> multiActions)
        {
            var allPressed = multiActions.Aggregate((all, next) => all | next);

            if (allPressed == MultiAction.LeftBoth || allPressed == MultiAction.RightBoth)
            {
                bool rimBlocked = multiActions.Any(a => a == blockedRim);
                bool centreBlocked = multiActions.Any(a => a == blockedCentre);

                bool sideBlocked = blockedSide != MultiAction.None && multiActions.All(a => a.HasFlag(blockedSide));

                if ((rimBlocked && centreBlocked) || sideBlocked)
                {
                    blockedKeystrokes++;

                    return true;
                }
                else
                {
                    blockedRim = multiActions.First(a => a.HasFlag(MultiAction.Rim));
                    blockedCentre = multiActions.First(a => a.HasFlag(MultiAction.Centre));

                    blockedSide = multiActions.First() & ~MultiAction.Rim & ~MultiAction.Centre;
                }
            }
            else if (RepeatAfterLargeNote.Value)
            {
                if (allPressed == MultiAction.LargeRim)
                    blockedRim = MultiAction.None;

                if (allPressed == MultiAction.LargeCentre)
                    blockedCentre = MultiAction.None;

                blockedSide = MultiAction.None;
            }

            return false;
        }

        private bool handleSingleKddk(bool exempt, List<MultiAction> multiActions)
        {
            if (exempt) return false;

            if (blockedSide != MultiAction.None && multiActions.All(a => a.HasFlag(blockedSide)))
            {
                blockedKeystrokes++;

                return true;
            }

            var newAction = blockedSide != MultiAction.None ? multiActions.Find(a => !a.HasFlag(blockedSide)) : multiActions.First();

            blockedSide = newAction & ~MultiAction.Rim & ~MultiAction.Centre;

            return false;
        }

        private bool handleSingleKkdd(bool exempt, List<MultiAction> multiActions)
        {
            var rim = multiActions.Find(a => a.HasFlag(MultiAction.Rim));
            var centre = multiActions.Find(a => a.HasFlag(MultiAction.Centre));

            if (RepeatOnColorChange.Value)
            {
                if (rim == MultiAction.None)
                    blockedRim = MultiAction.None;

                if (centre == MultiAction.None)
                    blockedCentre = MultiAction.None;
            }

            if (multiActions.All(a => a == blockedRim || a == blockedCentre) && !exempt)
            {
                blockedKeystrokes++;

                return true;
            }

            if (rim != MultiAction.None)
                blockedRim = rim;
            else if (RepeatOnColorChange.Value)
                blockedRim = MultiAction.None;

            if (centre != MultiAction.None)
                blockedCentre = centre;
            else if (RepeatOnColorChange.Value)
                blockedCentre = MultiAction.None;

            return false;
        }

        private bool handleBindings(List<TaikoAction> pressedActions)
        {
            if (healthProcessor.IsBreakTime.Value || !pressedActions.Any()) return false;

            var multiActions = new List<MultiAction>();

            pressedActions.ForEach(a =>
            {
                Enum.TryParse(a.ToString(), out MultiAction action);
                multiActions.Add(action);
            });

            bool exempt = nextHitObject?.HitObject.IsStrong == true && objectInRange == true;

            if (nextHitObject is DrawableHit hit)
                exempt &= hit.HitActions.Any(pressedActions.Contains);

            if (exempt && pressedActions.Count > 1)
                return handleMultipleKeypresses(multiActions);

            if (KeyConfiguration.Value == Playstyle.KDDK)
                return handleSingleKddk(exempt, multiActions);
            else
                return handleSingleKkdd(exempt, multiActions);
        }

        [Flags]
        private enum MultiAction
        {
            None = 0,

            Left = 1,
            Right = 2,

            Rim = 4,
            Centre = 8,

            LeftRim = Left | Rim,
            LeftCentre = Left | Centre,
            RightCentre = Right | Centre,
            RightRim = Right | Rim,

            LeftBoth = LeftRim | LeftCentre,
            RightBoth = RightRim | RightCentre,

            LargeRim = LeftRim | RightRim,
            LargeCentre = LeftCentre | RightCentre
        }

        public enum Playstyle
        {
            KDDK,
            KKDD
        }
    }
}
