// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using static osu.Game.Rulesets.Osu.Replays.Movers.MoverUtilExtensions;

namespace osu.Game.Rulesets.Osu.Replays.Movers
{
    public class MomentumMover : BaseDanceMover
    {
        private readonly float jmult;
        private readonly float nmult;
        private readonly float offsetMult;
        private readonly bool skipstacks;
        private float offset => MathF.PI * offsetMult;

        private Vector2 p1;
        private Vector2 p2;
        private Vector2 last;

        public MomentumMover()
        {
            var c = OsuRulesetConfigManager.Instance;
            jmult = c.Get<float>(OsuRulesetSetting.JumpMulti);
            nmult = c.Get<float>(OsuRulesetSetting.NextJumpMulti);
            offsetMult = c.Get<float>(OsuRulesetSetting.AngleOffset);
            skipstacks = c.Get<bool>(OsuRulesetSetting.SkipStackAngles);
        }

        private bool same(OsuHitObject o1, OsuHitObject o2) => o1.StackedPosition == o2.StackedPosition || (skipstacks && o1.Position == o2.Position);

        private (float, bool) nextAngle()
        {
            var obj = Beatmap.HitObjects;

            for (var i = ObjectIndex + 1; i < obj.Count - 1; ++i)
            {
                var o = obj[i];
                if (o is Slider s) return (s.GetStartAngle(), true);
                if (!same(o, obj[i + 1])) return (o.StackedPosition.AngleRV(obj[i + 1].StackedPosition), false);
            }

            return ((obj[^1] as Slider)?.GetEndAngle()
                 ?? ((Start as Slider)?.GetEndAngle() ?? StartPos.AngleRV(last)) + MathF.PI, false);
        }

        public override void OnObjChange()
        {
            var dst = Vector2.Distance(StartPos, EndPos);

            var s = Start as Slider;
            var (a2, afs) = nextAngle();
            var a1 = (ObjectsDuring[ObjectIndex] ? s?.GetStartAngle() + MathF.PI : s?.GetEndAngle()) ?? (ObjectIndex == 0 ? a2 + MathF.PI : StartPos.AngleRV(last));

            p1 = V2FromRad(a1, dst * jmult) + StartPos;

            var a = EndPos.AngleRV(StartPos);
            if (!afs && MathF.Abs(a2 - a) < offset) a2 = a2 - a < offset ? a - offset : a + offset;
            p2 = V2FromRad(a2, dst * nmult) + EndPos;

            if (!(End is Slider) && !same(Start, End)) last = p2;
        }

        public override Vector2 Update(double time)
        {
            var t = T(time);
            var r = 1 - t;

            // cubic bézier curve
            return new Vector2(
                r * r * r * StartX
              + r * r * t * p1.X * 3
              + r * t * t * p2.X * 3
              + t * t * t * EndX,
                r * r * r * StartY
              + r * r * t * p1.Y * 3
              + r * t * t * p2.Y * 3
              + t * t * t * EndY
            );
        }
    }
}
