// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters
{
    public class ColourHitErrorMeter : HitErrorMeter
    {
        private const int bar_height = 200;

        private readonly FillFlowContainer judgementsFlow;

        public ColourHitErrorMeter(HitWindows hitWindows)
            : base(hitWindows)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = judgementsFlow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.X,
                Height = bar_height,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 2),
                Masking = true,
            };
        }

        public override void OnNewJudgement(JudgementResult judgement)
        {
            judgementsFlow.Add(new DrawableJudgement(GetColourForHitResult(HitWindows.ResultFor(judgement.TimeOffset))));
        }

        private class DrawableJudgement : CircularContainer
        {
            public DrawableJudgement(Color4 colour)
            {
                Masking = true;
                Size = new Vector2(8);
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour
                };
            }
        }
    }
}
