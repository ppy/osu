// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using static osu.Game.Rulesets.Osu.Replays.Movers.MoverUtilExtensions;

namespace osu.Game.Rulesets.Osu.Replays.Movers
{
    public class MomentumMover : Mover
    {
        private float offset => restrictAngle * MathF.PI / 180.0f;

        private Vector2 last;
        private BezierCurveCubic curve;
        private readonly float jumpMult;
        private readonly float offsetMult;
        private readonly bool skipStacks;
        private readonly bool streamRestrict;
        private readonly float restrictArea;
        private readonly float restrictAngle;
        private readonly float streamMult;
        private readonly bool restrictInvert;
        private readonly float durationTrigger;
        private readonly float durationMult;

        private readonly float streamArea;
        private readonly float restrictAngleAdd;
        private readonly float restrictAngleSub;
        private readonly float equalPosBounce;
        private readonly bool sliderPredict;
        private readonly bool interpolateAngles;
        private readonly bool invertAngleInterpolation;

        public MomentumMover()
        {
            var config = OsuRulesetConfigManager.Instance;
            jumpMult = config.Get<float>(OsuRulesetSetting.JumpMult);
            offsetMult = config.Get<float>(OsuRulesetSetting.AngleOffset);
            skipStacks = config.Get<bool>(OsuRulesetSetting.SkipStackAngles);
            restrictInvert = config.Get<bool>(OsuRulesetSetting.RestrictInvert);
            restrictAngle = config.Get<float>(OsuRulesetSetting.RestrictAngle);
            restrictArea = config.Get<float>(OsuRulesetSetting.RestrictArea);
            streamRestrict = config.Get<bool>(OsuRulesetSetting.StreamRestrict);
            streamMult = config.Get<float>(OsuRulesetSetting.StreamMult);
            durationTrigger = config.Get<float>(OsuRulesetSetting.DurationTrigger);
            durationMult = config.Get<float>(OsuRulesetSetting.DurationMult);

            streamArea = config.Get<float>(OsuRulesetSetting.StreamArea);
            equalPosBounce = config.Get<float>(OsuRulesetSetting.EqualPosBounce);
            restrictAngleAdd = config.Get<float>(OsuRulesetSetting.RestrictAngleAdd);
            restrictAngleSub = config.Get<float>(OsuRulesetSetting.RestrictAngleSub);
            sliderPredict = config.Get<bool>(OsuRulesetSetting.SliderPredict);
            interpolateAngles = config.Get<bool>(OsuRulesetSetting.InterpolateAngles);
            invertAngleInterpolation = config.Get<bool>(OsuRulesetSetting.InvertAngleInterpolation);
        }

        private bool isSame(OsuHitObject o1, OsuHitObject o2)
        {
            return o1.StackedPosition == o2.StackedPosition || skipStacks && o1.Position == o2.Position;
        }

        private (float, bool) nextAngle()
        {
            var h = Beatmap.HitObjects;

            for (var i = ObjectIndex + 1; i < h.Count - 1; ++i)
            {
                var o = h[i];
                if (o is Slider s) return (s.GetStartAngle(), true);
                if (!isSame(o, h[i + 1])) return (o.StackedPosition.AngleRV(h[i + 1].StackedPosition), false);
            }

            return ((h[^1] as Slider)?.GetEndAngle()
                    ?? ((Start as Slider)?.GetEndAngle() ?? StartPos.AngleRV(last)) + MathF.PI, false);
        }

        public override void OnObjChange()
        {
            OsuHitObject next = null;

            if (Beatmap.HitObjects.Count - 1 > ObjectIndex + 2) next = Beatmap.HitObjects[ObjectIndex + 2];

            var stream = false;
            float sq1 = 0, sq2 = 0;

            if (next != null)
            {
                stream = IsStream(Start, End, next) && streamRestrict;
                sq1 = Vector2.DistanceSquared(StartPos, EndPos);
                sq2 = Vector2.DistanceSquared(EndPos, next.StackedPosition);
            }

            var area = restrictArea * MathF.PI / 180f;
            var sarea = streamArea * MathF.PI / 180f;
            var mult = jumpMult;
            var distance = Vector2.Distance(StartPos, EndPos);

            var startSlider = Start as Slider;
            var (a2, fromLong) = nextAngle();
            var a1 = (ObjectsDuring[ObjectIndex] ? startSlider?.GetStartAngle() + MathF.PI : startSlider?.GetEndAngle()) ?? (ObjectIndex == 0 ? a2 + MathF.PI : StartPos.AngleRV(last));
            var ac = a2 - EndPos.AngleRV(StartPos);

            if (End is Slider s2 && !isSame(Start, End) && sliderPredict)
            {
                var pos = Start.StackedPosition;
                var pos2 = EndPos;
                var s2a = s2.GetStartAngle();
                var dst2 = Vector2.Distance(pos, pos2);
                pos2 = new Vector2(s2a, dst2 * mult) + pos2;
                a2 = pos.AngleRV(pos2);
            }
            else
                a2 = Start.StackedPosition.AngleRV(EndPos);

            float a;

            if (sarea > 0 && stream && anorm(ac) < anorm(2 * MathF.PI - sarea))
            {
                a = StartPos.AngleRV(EndPos);
                const float sangle = MathF.PI * 0.5f;

                if (anorm(a1 - a) > MathF.PI)
                    a2 = a - sangle;
                else
                    a2 = a + sangle;

                mult = streamMult;
            }
            else if (!fromLong && area > 0 && MathF.Abs(anorm2(ac)) < area)
            {
                a = EndPos.AngleRV(StartPos);

                if (anorm(a2 - a) < offset != restrictInvert)
                    a2 = a + restrictAngleAdd * MathF.PI / 180f;
                else
                    a2 = a - restrictAngleSub * MathF.PI / 180f;

                mult = jumpMult;
            }
            else if (next != null && !fromLong && interpolateAngles)
            {
                var r = sq1 / (sq1 + sq2);
                a = StartPos.AngleRV(EndPos);

                if (invertAngleInterpolation)
                    r = sq2 / (sq1 + sq2);

                if (!isSame(Start, End))
                    a2 = a + r * anorm2(a2 - a);

                mult = offsetMult;
            }

            var bounce = !(End is IHasDuration) && isSame(Start, End);

            if (equalPosBounce > 0 && bounce)
            {
                a1 = StartPos.AngleRV(last);
                a2 = a1 + MathF.PI;
                distance = Vector2.Distance(last, StartPos);
                mult = equalPosBounce;
            }

            var duration = (float)(EndTime - StartTime);

            if (durationTrigger > 0 && duration >= durationTrigger)
                mult *= durationMult * (duration / durationTrigger);

            var p1 = V2FromRad(a1, distance * mult) + StartPos;
            var p2 = V2FromRad(a2, distance * mult) + EndPos;

            if (!bounce) last = p2;

            curve = new BezierCurveCubic(StartPos, EndPos, p1, p2);
        }

        private float anorm(float a)
        {
            const float pi2 = 2 * MathF.PI;
            a %= pi2;

            if (a < 0)
                a += pi2;

            return a;
        }

        private float anorm2(float a)
        {
            a = anorm(a);

            if (a > MathF.PI)
                a = -(2 * MathF.PI - a);

            return a;
        }

        public override Vector2 Update(double time) => curve.CalculatePoint(T(time));
    }
}
