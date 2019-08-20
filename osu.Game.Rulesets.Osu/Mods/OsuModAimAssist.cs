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
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModAimAssist : Mod, IApplicableToDrawableHitObjects, IUpdatableByPlayfield
    {
        public override string Name => "Aim Assist";
        public override string Acronym => "AA";
        public override IconUsage Icon => FontAwesome.Solid.MousePointer;
        public override ModType Type => ModType.Fun;
        public override string Description => "No need to chase the circle, the circle chases you";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModAutopilot), typeof(OsuModAutoplay), typeof(OsuModWiggle), typeof(OsuModTransform) };

        private readonly HashSet<DrawableOsuHitObject> movingObjects = new HashSet<DrawableOsuHitObject>();

        public void Update(Playfield playfield)
        {
            var drawableCursor = playfield.Cursor.ActiveCursor;

            // Avoid crowded judgment displays and hide follow points
            playfield.DisplayJudgements.Value = false;
            (playfield as OsuPlayfield)?.ConnectionLayer.Hide();

            // First move objects to new destination, then remove them from movingObjects set if they're too old
            movingObjects.RemoveWhere(d =>
            {
                var currentTime = playfield.Clock.CurrentTime;
                var h = d.HitObject;

                switch (d)
                {
                    case DrawableHitCircle circle:
                        circle.MoveTo(drawableCursor.DrawPosition, Math.Max(0, h.StartTime - currentTime - 10));
                        return currentTime > h.StartTime;

                    case DrawableSlider slider:

                        // Move slider to cursor
                        if (currentTime < h.StartTime)
                            d.MoveTo(drawableCursor.DrawPosition, Math.Max(0, h.StartTime - currentTime - 10));

                        // Move slider so that sliderball stays on the cursor
                        else
                            d.MoveTo(drawableCursor.DrawPosition - slider.Ball.DrawPosition);
                        return currentTime > (h as IHasEndTime)?.EndTime;

                    case DrawableSpinner _:
                        // TODO
                        return true;

                    default:
                        return true;
                }
            });
        }

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
                drawable.ApplyCustomUpdateState += drawableOnApplyCustomUpdateState;
        }

        private void drawableOnApplyCustomUpdateState(DrawableHitObject drawable, ArmedState state)
        {
            if (drawable is DrawableOsuHitObject d)
                movingObjects.Add(d);
        }
    }

    /*
     * TODOs
     *  - remove object timing glitches / artifacts
     *  - automate spinners
     *  - combine with OsuModRelax (?)
     *
     */
}
