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

        private readonly List<DrawableOsuHitObject> movingObjects = new List<DrawableOsuHitObject>();

        public void Update(Playfield playfield)
        {
            var cursorPos = playfield.Cursor.ActiveCursor.DrawPosition;

            // Avoid relocating judgment displays and hide follow points
            playfield.DisplayJudgements.Value = false;
            (playfield as OsuPlayfield)?.ConnectionLayer.Hide();

            // First move objects to new destination, then remove them from movingObjects list if they're too old
            movingObjects.RemoveAll(d =>
            {
                var h = d.HitObject;
                var currentTime = playfield.Clock.CurrentTime;
                var endTime = (h as IHasEndTime)?.EndTime ?? h.StartTime;
                d.ClearTransforms();

                switch (d)
                {
                    case DrawableHitCircle circle:

                        // 10ms earlier on the note to reduce chance of missing when clicking early / cursor moves fast
                        circle.MoveTo(cursorPos, Math.Max(0, endTime - currentTime - 10));
                        return currentTime > endTime;

                    case DrawableSlider slider:

                        // Move slider to cursor
                        if (currentTime < h.StartTime)
                        {
                            slider.MoveTo(cursorPos, Math.Max(0, h.StartTime - currentTime - 10));
                            return false;
                        }
                        // Move slider so that sliderball stays on the cursor
                        else
                        {
                            slider.MoveTo(cursorPos - slider.Ball.DrawPosition);
                            return currentTime > endTime;
                        }

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
            if (drawable is DrawableOsuHitObject hitobject)
                movingObjects.Add(hitobject);
        }
    }

    /*
     * TODOs
     *  - fix sliders reappearing at original position after their EndTime (see https://puu.sh/E7zT4/111cf9cdc8.gif)
     *  - relocate / hide slider headcircle's explosion, flash, ...
     *  - automate spinners
     *  - combine with OsuModRelax (?)
     *
     */
}
