// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using static osu.Game.Rulesets.Osu.Replays.Movers.MoverUtilExtensions;

namespace osu.Game.Rulesets.Osu.Replays.Movers
{
    public class BezierMover : DanceMover
    {
        private BezierCurve curve;
        private Vector2 pt = new Vector2(512f / 2f, 384 / 2f);
        private float previousSpeed;
        private readonly float aggressiveness;
        private readonly float sliderAggressiveness;

        public BezierMover()
        {
            var config = OsuRulesetConfigManager.Instance;
            aggressiveness = config.Get<float>(OsuRulesetSetting.BezierAggressiveness);
            sliderAggressiveness = config.Get<float>(OsuRulesetSetting.BezierSliderAggressiveness);
            previousSpeed = -1;
        }

        public override void OnObjChange()
        {
            var dist = Vector2.Distance(StartPos, EndPos);

            if (previousSpeed < 0)
            {
                previousSpeed = (float)(dist / Duration);
            }

            var genScale = previousSpeed;
            var s1 = Start as Slider;
            var s2 = End as Slider;
            var ok1 = s1 != null;
            var ok2 = s2 != null;
            float dst = 0, dst2 = 0, startAngle = 0, endAngle = 0;

            if (s1 != null)
            {
                dst = Vector2.Distance(s1.StackedPositionAt((StartTime - 10 - s1.StartTime) / s1.Duration), StartPos);
                endAngle = s1.GetEndAngle();
            }

            if (s2 != null)
            {
                dst2 = Vector2.Distance(s2.StackedPositionAt((EndTime + 10 - s2.StartTime) / s2.Duration), EndPos);
                startAngle = s2.GetStartAngle();
            }

            if (StartPos == EndPos)
                curve = new BezierCurve(StartPos, EndPos);
            else if (ok1 && ok2)
            {
                pt = V2FromRad(endAngle, dst * aggressiveness * sliderAggressiveness / 10) + StartPos;
                var pt2 = V2FromRad(startAngle, dst2 * aggressiveness * sliderAggressiveness / 10) + EndPos;

                curve = new BezierCurve(StartPos, pt, pt2, EndPos);
            }
            else if (ok1)
            {
                var pt1 = V2FromRad(endAngle, dst * aggressiveness * sliderAggressiveness / 10) + StartPos;
                pt = V2FromRad(EndPos.AngleRV(pt), genScale * aggressiveness) + EndPos;

                curve = new BezierCurve(StartPos, pt1, pt, EndPos);
            }
            else if (ok2)
            {
                pt = V2FromRad(StartPos.AngleRV(pt), genScale * aggressiveness) + StartPos;

                var pt1 = V2FromRad(startAngle, dst2 * aggressiveness * sliderAggressiveness / 10) + EndPos;

                curve = new BezierCurve(StartPos, pt, pt1, EndPos);
            }
            else
            {
                var angle = StartPos.AngleRV(pt);

                if (float.IsNaN(angle))
                    angle = 0;

                pt = V2FromRad(angle, previousSpeed * aggressiveness) + StartPos;
                curve = new BezierCurve(StartPos, pt, EndPos);
            }

            previousSpeed = (dist + 1.0f) / (float)Duration;
        }

        public override Vector2 Update(double time) => curve.CalculatePoint(T(time));
    }
}
