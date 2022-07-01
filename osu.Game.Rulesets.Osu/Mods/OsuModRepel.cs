// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Utils;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModRepel : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Repel";
        public override string Acronym => "RP";
        public override ModType Type => ModType.Fun;
        public override string Description => "Hit objects run away!";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModAutopilot), typeof(OsuModWiggle), typeof(OsuModTransform), typeof(ModAutoplay), typeof(OsuModMagnetised) };

        private IFrameStableClock gameplayClock;

        [SettingSource("Repulsion strength", "How strong the repulsion is.", 0)]
        public BindableFloat RepulsionStrength { get; } = new BindableFloat(0.6f)
        {
            Precision = 0.05f,
            MinValue = 0.05f,
            MaxValue = 1.0f,
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            gameplayClock = drawableRuleset.FrameStableClock;

            // Hide judgment displays and follow points as they won't make any sense.
            // Judgements can potentially be turned on in a future where they display at a position relative to their drawable counterpart.
            drawableRuleset.Playfield.DisplayJudgements.Value = false;
            (drawableRuleset.Playfield as OsuPlayfield)?.FollowPoints.Hide();
        }

        public void Update(Playfield playfield)
        {
            var cursorPos = playfield.Cursor.ActiveCursor.DrawPosition;

            foreach (var drawable in playfield.HitObjectContainer.AliveObjects)
            {
                float x = Math.Clamp(2 * drawable.X - cursorPos.X, 0, OsuPlayfield.BASE_SIZE.X);
                float y = Math.Clamp(2 * drawable.Y - cursorPos.Y, 0, OsuPlayfield.BASE_SIZE.Y);

                if (drawable.HitObject is Slider thisSlider)
                {
                    var possibleMovementBounds = OsuHitObjectGenerationUtils.CalculatePossibleMovementBounds(thisSlider);

                    x = possibleMovementBounds.Width < 0
                        ? x
                        : Math.Clamp(x, possibleMovementBounds.Left, possibleMovementBounds.Right);

                    y = possibleMovementBounds.Height < 0
                        ? y
                        : Math.Clamp(y, possibleMovementBounds.Top, possibleMovementBounds.Bottom);
                }

                var destination = new Vector2(x, y);

                switch (drawable)
                {
                    case DrawableHitCircle circle:
                        easeTo(circle, destination);
                        break;

                    case DrawableSlider slider:

                        if (!slider.HeadCircle.Result.HasResult)
                            easeTo(slider, destination);
                        else
                            easeTo(slider, destination - slider.Ball.DrawPosition);

                        break;
                }
            }
        }

        private void easeTo(DrawableHitObject hitObject, Vector2 destination)
        {
            double dampLength = Interpolation.Lerp(3000, 40, 0.8 * RepulsionStrength.Value);

            float x = (float)Interpolation.DampContinuously(hitObject.X, destination.X, dampLength, gameplayClock.ElapsedFrameTime);
            float y = (float)Interpolation.DampContinuously(hitObject.Y, destination.Y, dampLength, gameplayClock.ElapsedFrameTime);

            hitObject.Position = new Vector2(x, y);
        }
    }
}
