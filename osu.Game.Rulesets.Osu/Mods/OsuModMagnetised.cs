// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Timing;
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
    internal class OsuModMagnetised : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Magnetised";
        public override string Acronym => "MG";
        public override IconUsage? Icon => FontAwesome.Solid.Magnet;
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "No need to chase the circles – your cursor is a magnet!";
        public override double ScoreMultiplier => 0.5;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModAutopilot), typeof(OsuModWiggle), typeof(OsuModTransform), typeof(ModAutoplay), typeof(OsuModRelax), typeof(OsuModRepel), typeof(OsuModBubbles), typeof(OsuModDepth) };

        [SettingSource("Attraction strength", "How strong the pull is.", 0)]
        public BindableFloat AttractionStrength { get; } = new BindableFloat(0.5f)
        {
            Precision = 0.05f,
            MinValue = 0.05f,
            MaxValue = 1.0f,
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Hide judgment displays and follow points as they won't make any sense.
            // Judgements can potentially be turned on in a future where they display at a position relative to their drawable counterpart.
            drawableRuleset.Playfield.DisplayJudgements.Value = false;
            (drawableRuleset.Playfield as OsuPlayfield)?.FollowPoints.Hide();
        }

        public void Update(Playfield playfield)
        {
            var cursorPos = playfield.Cursor.AsNonNull().ActiveCursor.DrawPosition;

            foreach (var entry in playfield.HitObjectContainer.AliveEntries)
            {
                var drawable = entry.Value;

                switch (drawable)
                {
                    case DrawableHitCircle circle:
                        easeTo(playfield.Clock, circle, cursorPos);
                        break;

                    case DrawableSlider slider:

                        if (!slider.HeadCircle.Result.HasResult)
                            easeTo(playfield.Clock, slider, cursorPos);
                        else
                            easeTo(playfield.Clock, slider, cursorPos - slider.Ball.DrawPosition);

                        break;
                }
            }
        }

        private void easeTo(IFrameBasedClock clock, DrawableHitObject hitObject, Vector2 destination)
        {
            double dampLength = Interpolation.Lerp(3000, 40, AttractionStrength.Value);

            float x = (float)Interpolation.DampContinuously(hitObject.X, destination.X, dampLength, clock.ElapsedFrameTime);
            float y = (float)Interpolation.DampContinuously(hitObject.Y, destination.Y, dampLength, clock.ElapsedFrameTime);

            hitObject.Position = new Vector2(x, y);
        }
    }
}
