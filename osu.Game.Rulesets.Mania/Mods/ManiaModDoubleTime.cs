// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModDoubleTime : ModDoubleTime, IManiaRateAdjustmentMod
    {
        public HitWindows HitWindows { get; set; } = new ManiaHitWindows();
    }
}
