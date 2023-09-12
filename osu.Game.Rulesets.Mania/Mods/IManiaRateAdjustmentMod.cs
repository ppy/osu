// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    /// <summary>
    /// May be attached to rate-adjustment mods to adjust hit windows adjust relative to gameplay rate.
    /// </summary>
    /// <remarks>
    /// Historically, in osu!mania, hit windows are expected to adjust relative to the gameplay rate such that the real-world hit window remains the same.
    /// </remarks>
    public interface IManiaRateAdjustmentMod : IApplicableToDifficulty, IApplicableToHitObject
    {
        BindableNumber<double> SpeedChange { get; }

        HitWindows HitWindows { get; set; }

        void IApplicableToDifficulty.ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            HitWindows = new ManiaHitWindows(SpeedChange.Value);
            HitWindows.SetDifficulty(difficulty.OverallDifficulty);
        }

        void IApplicableToHitObject.ApplyToHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Note:
                    hitObject.HitWindows = HitWindows;
                    break;

                case HoldNote hold:
                    hold.Head.HitWindows = HitWindows;
                    hold.Tail.HitWindows = HitWindows;
                    break;
            }
        }
    }
}
