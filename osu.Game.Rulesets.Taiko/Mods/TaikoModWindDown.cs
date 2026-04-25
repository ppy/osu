// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Logging;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModWindDown : ModWindDown, IApplicableToDrawableHitObject
    {
        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            //Increase flying time to match stable. Because osu!stable does not make flying hits go faster with DoubleTime
            if (drawable is DrawableTaikoHitObject taiko)
            {
                taiko.FlyingTimeAfterHit = 300.0 * SpeedChange.Value;
            }
        }
    }
}
