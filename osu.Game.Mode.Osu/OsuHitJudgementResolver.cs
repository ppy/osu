using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables;

namespace osu.Game.Modes.Osu
{
    class OsuHitJudgementResolver : HitJudgementResolver
    {
        double hit50 = 150;
        double hit100 = 80;
        double hit300 = 30;
        public override void CheckJudgement(DrawableHitObject h, JudgementInfo info)
        {
            DrawableHitCircle circle = h as DrawableHitCircle;
            if (circle != null)
            {
                if (!info.UserTriggered)
                {
                    if (info.TimeOffset > hit50)
                        info.Result = HitResult.Miss;
                    return;
                }

                double hitOffset = Math.Abs(info.TimeOffset);

                if (hitOffset < hit300)
                    info.Result = HitResult.Hit300;
                else if (hitOffset < hit100)
                    info.Result = HitResult.Hit100;
                else if (hitOffset < hit50)
                    info.Result = HitResult.Hit50;
                else
                    info.Result = HitResult.Miss;
            }
        }
    }
}
