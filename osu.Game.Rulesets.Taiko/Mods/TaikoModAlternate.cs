// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModAlternate : ModAlternate<TaikoHitObject, TaikoAction>
    {
        [SettingSource("Playstyle", "Preferred Alternate Playstyle")]
        public Bindable<Playstyle> Style { get; } = new Bindable<Playstyle>();

        [SettingSource("Allow any key after drum roll")]
        public BindableBool ResetAfterDrumRoll { get; } = new BindableBool();

        private TaikoAction? prevPressedAction;
        private TaikoHitObject prevHitObject;
        private double prevActionTime;
        private bool strongHitSucceeded;
        private const double strong_hit_window = 30;

        protected override void ResetActionStates()
        {
            prevPressedAction = null;
        }

        protected override bool OnPressed(TaikoAction action)
        {
            if (!ShouldCheckForInput)
                return false;

            bool blockInput;

            var hitObject = HitObjects.FirstOrDefault(h =>
            {
                var time = Interceptor.Time.Current;
                var window = (h is DrumRoll) ? ((h.NestedHitObjects.FirstOrDefault() as DrumRollTick)?.HitWindow ?? 0) : h.HitWindows.WindowFor(HitResult.Miss);
                var endTime = (h as IHasDuration)?.EndTime ?? h.StartTime;
                return (time > h.StartTime - window) && (time < endTime + window);
            });

            var previous = HitObjects.ElementAtOrDefault(HitObjects.IndexOf(hitObject) - 1);
            if ((ResetAfterDrumRoll.Value && previous is DrumRoll) || previous is Swell)
                ResetActionStates();

            if (hitObject?.IsStrong ?? false)
                blockInput = handleStrongHit(hitObject, action);
            else if (hitObject is Swell)
                blockInput = false;
            else if (hitObject is IHasDuration)
                blockInput = action == prevPressedAction;
            else
                blockInput = shouldBlock(action);

            if (!strongHitSucceeded)
            {
                prevPressedAction = action;
                prevActionTime = Interceptor.Time.Current;
                prevHitObject = hitObject;
            }

            strongHitSucceeded = false;

            return blockInput;
        }

        protected override bool OnReleased(TaikoAction action) => false;

        private bool shouldBlock(TaikoAction action)
        {
            var blockInput = false;

            switch (Style.Value)
            {
                case Playstyle.KDDK:
                    if (prevPressedAction == TaikoAction.LeftRim || prevPressedAction == TaikoAction.LeftCentre)
                        blockInput = action == TaikoAction.LeftRim || action == TaikoAction.LeftCentre;

                    if (prevPressedAction == TaikoAction.RightRim || prevPressedAction == TaikoAction.RightCentre)
                        blockInput = action == TaikoAction.RightRim || action == TaikoAction.RightCentre;
                    break;

                case Playstyle.KKDD:
                    blockInput = action == prevPressedAction;
                    break;
            }

            return blockInput;
        }

        private bool handleStrongHit(TaikoHitObject hitObject, TaikoAction action)
        {
            if ((hitObject == prevHitObject) && (Interceptor.Time.Current - prevActionTime <= strong_hit_window))
            {
                var actionsToCheck = (prevPressedAction, action);

                if (compareUnordered(actionsToCheck, (TaikoAction.LeftRim, TaikoAction.RightRim)) ||
                    compareUnordered(actionsToCheck, (TaikoAction.LeftCentre, TaikoAction.RightCentre)))
                {
                    ResetActionStates();
                    strongHitSucceeded = true;

                    return false;
                }

                return true;
            }
            else
                return shouldBlock(action);

            bool compareUnordered((TaikoAction?, TaikoAction) actual, (TaikoAction, TaikoAction) expected)
                => (actual.Item1 == expected.Item1 && actual.Item2 == expected.Item2) ||
                   (actual.Item1 == expected.Item2 && actual.Item2 == expected.Item1);
        }

        public enum Playstyle
        {
            KDDK,
            KKDD
        }
    }
}
