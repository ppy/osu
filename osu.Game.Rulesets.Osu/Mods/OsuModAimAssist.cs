// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Osu.UI;
using osuTK;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModAimAssist : Mod, IUpdatableByPlayfield
    {
        public override string Name => "Aim Assist";
        public override string Acronym => "AA";
        public override IconUsage? Icon => FontAwesome.Solid.MousePointer;
        public override ModType Type => ModType.Fun;
        public override string Description => "No need to chase the circle, the circle chases you";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModAutopilot), typeof(OsuModWiggle), typeof(OsuModTransform), typeof(ModAutoplay) };

        public const float SPIN_RADIUS = 50; // same as OsuAutoGeneratorBase.SPIN_RADIUS

        private DrawableSpinner activeSpinner;
        private float spinnerAngle; // in radians

        public void Update(Playfield playfield)
        {
            var cursorPos = playfield.Cursor.ActiveCursor.DrawPosition;
            double currentTime = playfield.Clock.CurrentTime;

            // Judgment displays would all be cramped onto the cursor
            playfield.DisplayJudgements.Value = false;

            // FIXME: Hide follow points
            //(playfield as OsuPlayfield)?.ConnectionLayer.Hide();

            // If object too old, remove from movingObjects list, otherwise move to new destination
            foreach (var drawable in playfield.HitObjectContainer.AliveObjects.OfType<DrawableOsuHitObject>())
            {
                var h = drawable.HitObject;
                double endTime = h.GetEndTime();

                switch (drawable)
                {
                    case DrawableHitCircle circle:

                        // 10ms earlier on the note to reduce chance of missing when clicking early / cursor moves fast
                        circle.MoveTo(cursorPos, Math.Max(0, endTime - currentTime - 10));

                        break;

                    case DrawableSlider slider:

                        // Move slider to cursor
                        if (currentTime < h.StartTime)
                        {
                            slider.MoveTo(cursorPos, Math.Max(0, h.StartTime - currentTime - 10));
                        }
                        // Move slider so that sliderball stays on the cursor
                        else
                        {
                            // FIXME: Hide flashes
                            //slider.HeadCircle.Hide();
                            slider.MoveTo(cursorPos - slider.Ball.DrawPosition);
                        }

                        break;

                    case DrawableSpinner spinner:

                        // Move spinner to cursor
                        if (currentTime < h.StartTime)
                        {
                            spinner.MoveTo(cursorPos, Math.Max(0, h.StartTime - currentTime - 10));
                        }
                        else
                        {
                            // TODO:
                            //   - get current angle to cursor
                            //   - move clockwise(?)
                            //   - call spinner.RotationTracker.AddRotation

                            // TODO: Remove
                            //spinnerAngle = 0;
                            //activeSpinner = spinner;
                        }

                        break;

                    default:
                        continue;
                }
            }

            // Move active spinner around the cursor
            if (activeSpinner != null)
            {
                double spinnerEndTime = activeSpinner.HitObject.GetEndTime();

                if (currentTime > spinnerEndTime)
                {
                    activeSpinner = null;
                    spinnerAngle = 0;
                }
                else
                {
                    const float additional_degrees = 4;
                    float added_degrees = additional_degrees * (float)Math.PI / 180;
                    spinnerAngle += added_degrees;

                    //int spinsRequired = activeSpinner.HitObject.SpinsRequired;
                    //float spunDegrees = activeSpinner.Result.RateAdjustedRotation;
                    //double timeLeft = spinnerEndTime - currentTime;

                    // Visual progress
                    activeSpinner.MoveTo(new Vector2((float)(SPIN_RADIUS * Math.Cos(spinnerAngle) + cursorPos.X), (float)(SPIN_RADIUS * Math.Sin(spinnerAngle) + cursorPos.Y)));

                    // Logical progress
                    activeSpinner.RotationTracker.AddRotation(added_degrees);
                    Console.WriteLine($"added_degrees={added_degrees}");
                    //activeSpinner.Disc.RotationAbsolute += additional_degrees;
                }
            }
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
