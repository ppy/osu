// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public abstract class OsuEaseHitObjectPositionsMod : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModAutopilot), typeof(OsuModWiggle), typeof(OsuModTransform), typeof(ModAutoplay) };

        protected BindableFloat EasementStrength = new BindableFloat(0.5f);
        protected Vector2 CursorPosition;
        protected DrawableHitObject WorkingHitObject;
        protected virtual Vector2 DestinationVector => WorkingHitObject.Position;

        private IFrameStableClock gameplayClock;

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
            CursorPosition = playfield.Cursor.ActiveCursor.DrawPosition;

            foreach (var drawable in playfield.HitObjectContainer.AliveObjects)
            {
                WorkingHitObject = drawable;
                switch (drawable)
                {
                    case DrawableHitCircle circle:
                        EaseHitObjectPositionToVector(circle, DestinationVector);
                        break;

                    case DrawableSlider slider:

                        if (!slider.HeadCircle.Result.HasResult)
                            EaseHitObjectPositionToVector(slider, DestinationVector);
                        else
                            EaseHitObjectPositionToVector(slider, DestinationVector - slider.Ball.DrawPosition);

                        break;
                }
            }
        }

        protected void EaseHitObjectPositionToVector(DrawableHitObject hitObject, Vector2 destination)
        {
            double dampLength = Interpolation.Lerp(3000, 40, EasementStrength.Value);

            float x = (float)Interpolation.DampContinuously(hitObject.X, Math.Clamp(destination.X, 0, OsuPlayfield.BASE_SIZE.X), dampLength, gameplayClock.ElapsedFrameTime);
            float y = (float)Interpolation.DampContinuously(hitObject.Y, Math.Clamp(destination.Y, 0, OsuPlayfield.BASE_SIZE.Y), dampLength, gameplayClock.ElapsedFrameTime);

            hitObject.Position = new Vector2(x, y);
        }
    }
}
