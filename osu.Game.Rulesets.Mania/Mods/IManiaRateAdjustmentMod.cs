// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Mods
{
    /// <summary>
    /// May be attached to rate-adjustment mods to adjust hit windows adjust relative to gameplay rate.
    /// </summary>
    /// <remarks>
    /// Historically, in osu!mania, hit windows are expected to adjust relative to the gameplay rate such that the real-world hit window remains the same.
    /// </remarks>
    public interface IManiaRateAdjustmentMod : IApplicableToHitObject
    {
        BindableNumber<double> SpeedChange { get; }

        void IApplicableToHitObject.ApplyToHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Note:
                    ((ManiaHitWindows)hitObject.HitWindows).SpeedMultiplier = SpeedChange.Value;
                    break;

                case HoldNote hold:
                    ((ManiaHitWindows)hold.Head.HitWindows).SpeedMultiplier = SpeedChange.Value;
                    ((ManiaHitWindows)hold.Tail.HitWindows).SpeedMultiplier = SpeedChange.Value;
                    break;
            }
        }
    }
}
