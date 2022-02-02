// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

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

        private IFrameStableClock gameplayClock;

        [SettingSource("Assist strength", "Change the distance notes should travel towards you.", 0)]
        public BindableFloat AssistStrength { get; } = new BindableFloat(0.5f)
        {
            Precision = 0.05f,
            MinValue = 0.1f,
            MaxValue = 1.0f,
        };

        private const float spin_radius = 50;

        private OsuInputManager inputManager;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Grab the input manager for future use
            inputManager = (OsuInputManager)drawableRuleset.KeyBindingInputManager;

            gameplayClock = drawableRuleset.FrameStableClock;

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
                        easeTo(circle, cursorPos);

                        break;

                    case DrawableSlider slider:

                        // Move slider to cursor
                        if (!slider.HeadCircle.Result.HasResult)
                        {
                            easeTo(slider, cursorPos);
                        }
                        // Move slider so that sliderball stays on the cursor
                        else
                        {
                            slider.HeadCircle.Hide(); // hide flash, triangles, ... so they don't move with slider
                            easeTo(slider, cursorPos - slider.Ball.DrawPosition);
                            // FIXME: some sliders re-appearing at their original position for a single frame when they're done
                        }

                        break;

                    case DrawableSpinner spinner:

                        // Move spinner _next_ to cursor
                        if (currentTime < h.StartTime)
                        {
                            easeTo(spinner, cursorPos + new Vector2(0, -spin_radius));
                        }
                        else
                        {
                            // Move spinner visually
                            Vector2 delta = new Vector2(spin_radius);
                            float angle = (float)gameplayClock.CurrentTime * 10;

                            // Move spinner logically
                            if (inputManager?.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton) ?? false)
                            {
                                var targetPos = new Vector2(
                                    delta.X * MathF.Cos(angle) - delta.Y * MathF.Sin(angle) + cursorPos.X,
                                    delta.X * MathF.Sin(angle) + delta.Y * MathF.Cos(angle) + cursorPos.Y
                                );

                                easeTo(spinner, targetPos);
                            }
                        }

                        break;
                }
            }
        }

        private void easeTo(DrawableHitObject hitObject, Vector2 destination)
        {
            double dampLength = Interpolation.Lerp(500, 50, AssistStrength.Value);

            float x = (float)Interpolation.DampContinuously(hitObject.X, destination.X, dampLength, gameplayClock.ElapsedFrameTime);
            float y = (float)Interpolation.DampContinuously(hitObject.Y, destination.Y, dampLength, gameplayClock.ElapsedFrameTime);

            hitObject.Position = new Vector2(x, y);
        }
    }
}
