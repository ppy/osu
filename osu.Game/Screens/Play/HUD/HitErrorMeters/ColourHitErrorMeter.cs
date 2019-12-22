// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
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
        private const int max_available_judgements = 20;

        private readonly JudgementFlow judgementsFlow;
        private readonly BindableList<(Color4 colour, JudgementResult result)> judgements = new BindableList<(Color4 colour, JudgementResult result)>();

        public ColourHitErrorMeter(HitWindows hitWindows)
            : base(hitWindows)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = judgementsFlow = new JudgementFlow();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            judgementsFlow.Judgements.BindTo(judgements);
        }

        public override void OnNewJudgement(JudgementResult judgement)
        {
            judgements.Add((GetColourForHitResult(HitWindows.ResultFor(judgement.TimeOffset)), judgement));

            if (judgements.Count > max_available_judgements)
                judgements.RemoveAt(0);
        }

        private class JudgementFlow : FillFlowContainer<DrawableResult>
        {
            private const int drawable_judgement_size = 8;
            private const int spacing = 2;
            private const int animation_duration = 200;

            public readonly BindableList<(Color4 colour, JudgementResult result)> Judgements = new BindableList<(Color4 colour, JudgementResult result)>();

            private int runningDepth;

            public JudgementFlow()
            {
                AutoSizeAxes = Axes.X;
                Height = max_available_judgements * (drawable_judgement_size + spacing);
                Spacing = new Vector2(0, spacing);
                Direction = FillDirection.Vertical;
                LayoutDuration = animation_duration;
                LayoutEasing = Easing.OutQuint;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Judgements.ItemsAdded += push;
                Judgements.ItemsRemoved += pop;
            }

            private void push(IEnumerable<(Color4 colour, JudgementResult result)> judgements)
            {
                var (colour, result) = judgements.Single();
                var drawableJudgement = new DrawableResult(colour, result, drawable_judgement_size)
                {
                    Alpha = 0,
                };

                Insert(runningDepth--, drawableJudgement);
                drawableJudgement.FadeInFromZero(animation_duration, Easing.OutQuint);
            }

            private void pop(IEnumerable<(Color4 colour, JudgementResult result)> judgements)
            {
                var (colour, result) = judgements.Single();
                Children.FirstOrDefault(c => c.Result == result).FadeOut(animation_duration, Easing.OutQuint).Expire();
            }
        }

        private class DrawableResult : CircularContainer
        {
            public JudgementResult Result { get; private set; }

            public DrawableResult(Color4 colour, JudgementResult result, int size)
            {
                Result = result;

                Masking = true;
                Size = new Vector2(size);
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour
                };
            }
        }
    }
}
