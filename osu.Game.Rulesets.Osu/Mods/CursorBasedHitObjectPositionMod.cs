// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Timing;
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
    internal abstract class CursorBasedHitObjectPositionMod : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override ModType Type => ModType.Fun;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModAutopilot), typeof(OsuModWiggle), typeof(OsuModTransform), typeof(ModAutoplay) };

        protected abstract double GetDampLength(DrawableHitObject hitObject, Vector2 cursorPos);

        protected abstract Vector2 GetBaseDestination(DrawableHitObject drawable, Vector2 cursorPos);

        private void easeTo(DrawableHitObject hitObject, Vector2 destination, Vector2 cursorPos, IFrameBasedClock clock)
        {
            double dampLength = GetDampLength(hitObject, cursorPos);

            float x = (float)Interpolation.DampContinuously(hitObject.X, destination.X, dampLength, clock.ElapsedFrameTime);
            float y = (float)Interpolation.DampContinuously(hitObject.Y, destination.Y, dampLength, clock.ElapsedFrameTime);

            hitObject.Position = new Vector2(x, y);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
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
                var destination = GetBaseDestination(drawable, cursorPos);

                switch (drawable)
                {
                    case DrawableSlider slider:
                        destination = slider.HeadCircle.Result.HasResult ? destination - slider.Ball.DrawPosition : cursorPos;
                        break;
                }

                easeTo(drawable, destination, cursorPos, playfield.Clock);
            }
        }
    }
}
