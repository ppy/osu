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
using osu.Game.Configuration;
using osu.Framework.Bindables;

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

        [SettingSource("Assist strength", "Change the distance notes should travel towards you.", 0)]
        public BindableFloat AssistStrength { get; } = new BindableFloat(0.3f)
        {
            Precision = 0.05f,
            MinValue = 0.0f,
            MaxValue = 1.0f,
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Hide judgment displays and follow points
            drawableRuleset.Playfield.DisplayJudgements.Value = false;
            (drawableRuleset.Playfield as OsuPlayfield)?.FollowPoints.Hide();
        }

        public void Update(Playfield playfield)
        {
            Vector2 cursorPos = playfield.Cursor.ActiveCursor.DrawPosition;
            double currentTime = playfield.Clock.CurrentTime;

            foreach (var drawable in playfield.HitObjectContainer.AliveObjects.OfType<DrawableOsuHitObject>())
            {
                var h = drawable.HitObject;

                if (currentTime < h.StartTime && (drawable is DrawableHitCircle || drawable is DrawableSlider))
                {
                    double timeMoving = currentTime - (h.StartTime - h.TimePreempt);
                    float percentDoneMoving = (float)(timeMoving / h.TimePreempt);
                    float percentDistLeft = Math.Clamp(AssistStrength.Value - percentDoneMoving + 0.1f, 0, 1);

                    Vector2 targetPos = drawable.Position + percentDistLeft * (cursorPos - drawable.Position);
                    drawable.MoveTo(targetPos, h.StartTime - currentTime);
                }
            }
        }
    }
}
