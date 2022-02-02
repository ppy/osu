// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        public override string Description => "No need to chase the circle – the circle chases you!";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModAutopilot), typeof(OsuModWiggle), typeof(OsuModTransform), typeof(ModAutoplay) };

        private IFrameStableClock gameplayClock;

        [SettingSource("Assist strength", "How much this mod will assist you.", 0)]
        public BindableFloat AssistStrength { get; } = new BindableFloat(0.5f)
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
                switch (drawable)
                {
                    case DrawableHitCircle circle:
                        easeTo(circle, cursorPos);
                        break;

                    case DrawableSlider slider:

                        if (!slider.HeadCircle.Result.HasResult)
                            easeTo(slider, cursorPos);
                        else
                            easeTo(slider, cursorPos - slider.Ball.DrawPosition);

                        break;
                }
            }
        }

        private void easeTo(DrawableHitObject hitObject, Vector2 destination)
        {
            double dampLength = Interpolation.Lerp(3000, 40, AssistStrength.Value);

            float x = (float)Interpolation.DampContinuously(hitObject.X, destination.X, dampLength, gameplayClock.ElapsedFrameTime);
            float y = (float)Interpolation.DampContinuously(hitObject.Y, destination.Y, dampLength, gameplayClock.ElapsedFrameTime);

            hitObject.Position = new Vector2(x, y);
        }
    }
}
