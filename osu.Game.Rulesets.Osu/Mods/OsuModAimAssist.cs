// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Osu.UI;
using osuTK;
using System.Linq;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModAimAssist : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
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
        private OsuInputManager inputManager;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Grab the input manager for future use
            inputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;

            // Hide judgment displays and follow points
            drawableRuleset.Playfield.DisplayJudgements.Value = false;
            (drawableRuleset.Playfield as OsuPlayfield)?.FollowPoints.Hide();
        }

        public void Update(Playfield playfield)
        {
            var cursorPos = playfield.Cursor.ActiveCursor.DrawPosition;
            double currentTime = playfield.Clock.CurrentTime;

            // Move all currently alive object to new destination
            foreach (var drawable in playfield.HitObjectContainer.AliveObjects.OfType<DrawableOsuHitObject>())
            {
                var h = drawable.HitObject;

                switch (drawable)
                {
                    case DrawableHitCircle circle:

                        // 10ms earlier on the note to reduce chance of missing when clicking early / cursor moves fast
                        circle.MoveTo(cursorPos, Math.Max(0, h.StartTime - currentTime - 10));
                        // FIXME: some circles cause flash at original(?) position when clicked too early

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
                            slider.HeadCircle.Hide(); // hide flash, triangles, ... so they don't move with slider
                            slider.MoveTo(cursorPos - slider.Ball.DrawPosition);
                            // FIXME: some sliders re-appearing at their original position for a single frame when they're done
                        }

                        break;

                    case DrawableSpinner spinner:

                        // Move spinner _next_ to cursor
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

                            // Move spinner logically
                            if (inputManager?.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton) ?? false)
                            {
                                // Arbitrary value, might lead to some inconsistencies depending on clock rate, replay, ...
                                spinner.RotationTracker.AddRotation(2 * MathF.PI);
                            }
                        }

                        break;
                }
            }

            prevCursorPos = cursorPos;
        }
    }
}
