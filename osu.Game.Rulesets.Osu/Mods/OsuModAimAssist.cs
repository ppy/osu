// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
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

        private const float spin_radius = 30;
        private Vector2? prevCursorPos;

        public void Update(Playfield playfield)
        {
            var cursorPos = playfield.Cursor.ActiveCursor.DrawPosition;
            double currentTime = playfield.Clock.CurrentTime;

            // Hide judgment displays and follow points
            playfield.DisplayJudgements.Value = false;
            (playfield as OsuPlayfield)?.FollowPoints.Clear();

            // Move all currently alive object to new destination
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
                            spinner.MoveTo(cursorPos + new Vector2(0, -spin_radius), Math.Max(0, h.StartTime - currentTime - 10));
                        }
                        else
                        {
                            // Move spinner visually
                            Vector2 delta = spin_radius * (spinner.Position - prevCursorPos ?? cursorPos).Normalized();
                            const float angle = 3 * MathF.PI / 180; // radians per update, arbitrary value

                            // Rotation matrix
                            var targetPos = new Vector2(
                                delta.X * MathF.Cos(angle) - delta.Y * MathF.Sin(angle) + cursorPos.X,
                                delta.X * MathF.Sin(angle) + delta.Y * MathF.Cos(angle) + cursorPos.Y
                            );

                            spinner.MoveTo(targetPos);

                            // Logically finish spinner immediatly, no need for the user to click.
                            // Temporary workaround until spinner rotations are easier to handle, similar as Autopilot mod.
                            spinner.Result.RateAdjustedRotation = spinner.HitObject.SpinsRequired * 360;
                        }

                        break;
                }
            }

            prevCursorPos = cursorPos;
        }
    }
}
