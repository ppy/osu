// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
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
        private const int animation_duration = 200;

        private readonly JudgementFlow judgementsFlow;

        public ColourHitErrorMeter(HitWindows hitWindows)
            : base(hitWindows)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = judgementsFlow = new JudgementFlow();
        }

        public override void OnNewJudgement(JudgementResult judgement) => judgementsFlow.Push(GetColourForHitResult(HitWindows.ResultFor(judgement.TimeOffset)));

        private class JudgementFlow : FillFlowContainer<HitErrorCircle>
        {
            private const int max_available_judgements = 20;
            private const int drawable_judgement_size = 8;
            private const int spacing = 2;

            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Reverse();

            public JudgementFlow()
            {
                AutoSizeAxes = Axes.X;
                Height = max_available_judgements * (drawable_judgement_size + spacing) - spacing;
                Spacing = new Vector2(0, spacing);
                Direction = FillDirection.Vertical;
                LayoutDuration = animation_duration;
                LayoutEasing = Easing.OutQuint;
            }

            public void Push(Color4 colour)
            {
                Add(new HitErrorCircle(colour, drawable_judgement_size));

                if (Children.Count > max_available_judgements)
                    Children.FirstOrDefault(c => !c.IsRemoved)?.Remove();
            }
        }

        private class HitErrorCircle : Container
        {
            public bool IsRemoved { get; private set; }

            private readonly Circle circle;

            public HitErrorCircle(Color4 colour, int size)
            {
                Size = new Vector2(size);
                Child = circle = new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Colour = colour
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                circle.FadeInFromZero(animation_duration, Easing.OutQuint);
                circle.MoveToY(-DrawSize.Y);
                circle.MoveToY(0, animation_duration, Easing.OutQuint);
            }

            public void Remove()
            {
                IsRemoved = true;

                this.FadeOut(animation_duration, Easing.OutQuint).Expire();
            }
        }
    }
}
