// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModAimAssist : Mod, IApplicableToDrawableHitObjects, IUpdatableByPlayfield
    {
        public override string Name => "AimAssist";
        public override string Acronym => "AA";
        public override IconUsage Icon => FontAwesome.Solid.MousePointer;
        public override ModType Type => ModType.Fun;
        public override string Description => "No need to chase the circle, the circle chases you";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModAutopilot), typeof(OsuModAutoplay), typeof(OsuModWiggle), typeof(OsuModTransform) };

        private HashSet<DrawableOsuHitObject> movingObjects = new HashSet<DrawableOsuHitObject>();
        private int updateCounter = 0;

        public void Update(Playfield playfield)
        {
            // Avoid crowded judgment displays
            playfield.DisplayJudgements.Value = false;

            // Object destination updated when cursor updates
            playfield.Cursor.ActiveCursor.OnUpdate += drawableCursor =>
            {
                // ... every 500th cursor update iteration
                // (lower -> potential lags ; higher -> easier to miss if cursor too fast)
                if (updateCounter++ < 500) return;
                updateCounter = 0;

                // First move objects to new destination, then remove them from movingObjects set if they're too old
                movingObjects.RemoveWhere(d =>
                {
                    var currentTime = playfield.Clock.CurrentTime;
                    var h = d.HitObject;
                    d.ClearTransforms();
                    switch (d)
                    {
                        case DrawableHitCircle circle:
                            d.MoveTo(drawableCursor.DrawPosition, Math.Max(0, h.StartTime - currentTime));
                            return currentTime > h.StartTime;
                        case DrawableSlider slider:

                            // Move slider to cursor
                            if (currentTime < h.StartTime)
                                d.MoveTo(drawableCursor.DrawPosition, Math.Max(0, h.StartTime - currentTime));

                            // Move slider so that sliderball stays on the cursor
                            else
                                d.MoveTo(drawableCursor.DrawPosition - slider.Ball.DrawPosition, Math.Max(0, h.StartTime - currentTime));
                            return currentTime > (h as IHasEndTime).EndTime - 50;
                        case DrawableSpinner spinner:
                            // TODO
                            return true;
                    }
                    return true; // never happens(?)
                });
            };
        }

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
                drawable.ApplyCustomUpdateState += drawableOnApplyCustomUpdateState;
        }

        private void drawableOnApplyCustomUpdateState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;
            var h = d.HitObject;
            using (d.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                movingObjects.Add(d);
        }
    }

    /*
     * TODOs
     *  - remove object timing glitches / artifacts
     *  - remove FollowPoints
     *  - automate sliders
     *  - combine with OsuModRelax (?)
     *  - must be some way to make this more effictient
     *
     */
}
