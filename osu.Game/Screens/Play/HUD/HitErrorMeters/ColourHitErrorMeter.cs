// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

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
            judgementsFlow.Add(new DrawableJudgement(HitWindows.ResultFor(judgement.TimeOffset)));
        }

        private class DrawableJudgement : CircularContainer
        {
            private readonly Box background;
            private readonly HitResult result;

            public DrawableJudgement(HitResult result)
            {
                this.result = result;

                Masking = true;
                Size = new Vector2(8);
                Child = background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                switch (result)
                {
                    case HitResult.Miss:
                        background.Colour = colours.Red;
                        break;

                    case HitResult.Meh:
                        background.Colour = colours.Yellow;
                        break;

                    case HitResult.Ok:
                        background.Colour = colours.Green;
                        break;

                    case HitResult.Good:
                        background.Colour = colours.GreenLight;
                        break;

                    case HitResult.Great:
                        background.Colour = colours.Blue;
                        break;

                    default:
                        background.Colour = colours.BlueLight;
                        break;
                }
            }
        }
    }
}
