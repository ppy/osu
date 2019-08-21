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
using osuTK;

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
        public override Type[] IncompatibleMods => new[] { typeof(OsuModAutopilot), typeof(OsuModWiggle), typeof(OsuModTransform), typeof(ModAutoplay) };

        private readonly List<DrawableOsuHitObject> movingObjects = new List<DrawableOsuHitObject>();
        private DrawableSpinner activeSpinner;
        private double spinnerAngle; // in radians

        public void Update(Playfield playfield)
        {
            var cursorPos = playfield.Cursor.ActiveCursor.DrawPosition;
            var currentTime = playfield.Clock.CurrentTime;

            // Avoid relocating judgment displays and hide follow points
            playfield.DisplayJudgements.Value = false;
            (playfield as OsuPlayfield)?.ConnectionLayer.Hide();

            // If object too old, remove from movingObjects list, otherwise move to new destination
            movingObjects.RemoveAll(d =>
            {
                var h = d.HitObject;
                var endTime = (h as IHasEndTime)?.EndTime ?? h.StartTime;

                // Object no longer required to be moved -> remove from list
                if (currentTime > endTime)
                    return true;

                switch (d)
                {
                    case DrawableHitCircle circle:

                        // 10ms earlier on the note to reduce chance of missing when clicking early / cursor moves fast
                        circle.MoveTo(cursorPos, Math.Max(0, endTime - currentTime - 10));
                        return false;

                    case DrawableSlider slider:

                        // Move slider to cursor
                        if (currentTime < h.StartTime)
                        {
                            slider.MoveTo(cursorPos, Math.Max(0, h.StartTime - currentTime - 10));
                        }
                        // Move slider so that sliderball stays on the cursor
                        else
                        {
                            slider.HeadCircle.Hide(); // temporary solution to supress HeadCircle's explosion, flash, ... at wrong location
                            slider.MoveTo(cursorPos - slider.Ball.DrawPosition);
                        }

                        return false;

                    case DrawableSpinner spinner:

                        // Move spinner to cursor
                        if (currentTime < h.StartTime)
                        {
                            spinner.MoveTo(cursorPos, Math.Max(0, h.StartTime - currentTime - 10));
                            return false;
                        }
                        else
                        {
                            spinnerAngle = 0;
                            activeSpinner = spinner;
                            return true;
                        }

                    default:
                        return true;
                }
            });

            if (activeSpinner != null)
            {
                if (currentTime > (activeSpinner.HitObject as IHasEndTime)?.EndTime)
                {
                    activeSpinner = null;
                    spinnerAngle = 0;
                }
                else
                {
                    const float additional_degrees = 4;
                    const int dist_from_cursor = 30;
                    spinnerAngle += additional_degrees * Math.PI / 180;

                    // Visual progress
                    activeSpinner.MoveTo(new Vector2((float)(dist_from_cursor * Math.Cos(spinnerAngle) + cursorPos.X), (float)(dist_from_cursor * Math.Sin(spinnerAngle) + cursorPos.Y)));

                    // Logical progress
                    activeSpinner.Disc.RotationAbsolute += additional_degrees;
                }
            }
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
     *  - find nicer way to handle slider headcircle explosion, flash, ...
     *  - add Aim Assist as incompatible mod for Autoplay (?)
     *
     */
}
