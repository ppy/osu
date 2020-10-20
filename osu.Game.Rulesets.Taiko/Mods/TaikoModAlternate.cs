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
        private TaikoHitObject currentHitObject;
        private TaikoHitObject lastHitObject;
        private double lastActionTime;
        private bool strongHitSucceeded;
        private const double strong_hit_window = 30;

        public override void SaveState(TaikoAction action)
        {
            base.SaveState(action);

            lastActionTime = Interceptor.Time.Current;
            lastHitObject = currentHitObject;
        }

        protected override bool OnPressed(TaikoAction action)
        {
            currentHitObject = getNextHitObject();

            bool blockInput;

            var previous = HitObjects.ElementAtOrDefault(HitObjects.IndexOf(currentHitObject) - 1);
            if ((ResetAfterDrumRoll.Value && previous is DrumRoll) || previous is Swell)
                LastActionPressed = null;

            switch (currentHitObject)
            {
                // Called at the second key press to allow two keys to be pressed in succession for strong hit objects
                case Hit h when h.IsStrong:
                    blockInput = handleStrongHit(currentHitObject, action);
                    break;

                // Allow key mashing for swells
                case Swell _:
                    blockInput = false;
                    break;

                // Ignore playstyle setting during drum rolls
                case IHasDuration d:
                    blockInput = action == LastActionPressed;
                    break;

                default:
                    blockInput = shouldBlock(action);
                    break;
            }

            if (!strongHitSucceeded)
                SaveState(action);

            strongHitSucceeded = false;

            return blockInput;
        }

        private TaikoHitObject getNextHitObject()
        {
            // Get the next incoming hit object while also accounting for drum rolls and hit windows
            return HitObjects.FirstOrDefault(h =>
            {
                double time = Interceptor.Time.Current;
                double window = (h is DrumRoll)
                    ? ((h.NestedHitObjects.FirstOrDefault() as DrumRollTick)?.HitWindow ?? 0)
                    : h.HitWindows.WindowFor(HitResult.Miss);
                double endTime = (h as IHasDuration)?.EndTime ?? h.StartTime;
                return (time > h.StartTime - window) && (time < endTime + window);
            });
        }

        private bool shouldBlock(TaikoAction action)
        {
            var blockInput = false;

            switch (Style.Value)
            {
                case Playstyle.KDDK:
                    if (LastActionPressed == TaikoAction.LeftRim || LastActionPressed == TaikoAction.LeftCentre)
                        blockInput = action == TaikoAction.LeftRim || action == TaikoAction.LeftCentre;

                    if (LastActionPressed == TaikoAction.RightRim || LastActionPressed == TaikoAction.RightCentre)
                        blockInput = action == TaikoAction.RightRim || action == TaikoAction.RightCentre;
                    break;

                case Playstyle.KKDD:
                    blockInput = action == LastActionPressed;
                    break;
            }

            return blockInput;
        }

        private bool handleStrongHit(TaikoHitObject hitObject, TaikoAction action)
        {
            if ((hitObject == lastHitObject) && (Interceptor.Time.Current - lastActionTime <= strong_hit_window))
            {
                var actionsToCheck = (LastActionPressed, action);

                if (compareUnordered(actionsToCheck, (TaikoAction.LeftRim, TaikoAction.RightRim)) ||
                    compareUnordered(actionsToCheck, (TaikoAction.LeftCentre, TaikoAction.RightCentre)))
                {
                    LastActionPressed = null;
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
