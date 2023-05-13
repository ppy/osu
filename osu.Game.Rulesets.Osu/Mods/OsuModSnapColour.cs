// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Mod that colours <see cref="HitObject"/>s based on the musical division they are on
    /// </summary>
    public class OsuModSnapColour : ModSnapColour, IApplicableToBeatmap, IApplicableToDrawableHitObject
    {
        [Resolved]
        private OsuColour colours { get; set; } = new OsuColour();

        [Resolved(canBeNull: true)]
        private IBeatmap currentBeatmap { get; set; }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            //Store a reference to the current beatmap to look up the beat divisor when notes are drawn
            if (this.currentBeatmap != beatmap)
                this.currentBeatmap = beatmap;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (currentBeatmap.IsNull() || drawable.IsNull()) return;

            drawable.OnUpdate += _ =>
                drawable.AccentColour.Value = BindableBeatDivisor.GetColourFor(currentBeatmap.ControlPointInfo.GetClosestBeatDivisor(drawable.HitObject.StartTime), colours);
        }
    }
}
