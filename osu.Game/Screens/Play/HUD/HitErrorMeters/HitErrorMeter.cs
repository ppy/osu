// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters
{
    public abstract class HitErrorMeter : CompositeDrawable, ISkinnableDrawable
    {
        protected HitWindows HitWindows { get; private set; }

        [Resolved]
        private ScoreProcessor processor { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(DrawableRuleset drawableRuleset)
        {
            HitWindows = drawableRuleset?.FirstAvailableHitWindows ?? HitWindows.Empty;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            processor.NewJudgement += onNewJudgement;
        }

        private void onNewJudgement(JudgementResult result)
        {
            if (result.HitObject.HitWindows?.WindowFor(HitResult.Miss) == 0)
                return;

            OnNewJudgement(result);
        }

        protected abstract void OnNewJudgement(JudgementResult judgement);

        protected Color4 GetColourForHitResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return colours.Red;

                case HitResult.Meh:
                    return colours.Yellow;

                case HitResult.Ok:
                    return colours.Green;

                case HitResult.Good:
                    return colours.GreenLight;

                case HitResult.Great:
                    return colours.Blue;

                default:
                    return colours.BlueLight;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (processor != null)
                processor.NewJudgement -= onNewJudgement;
        }
    }
}
