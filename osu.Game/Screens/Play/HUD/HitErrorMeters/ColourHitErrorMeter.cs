// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
        internal const int MAX_DISPLAYED_JUDGEMENTS = 20;

        private const int animation_duration = 200;
        private const int drawable_judgement_size = 8;
        private const int spacing = 2;

        private readonly JudgementFlow judgementsFlow;

        public ColourHitErrorMeter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = judgementsFlow = new JudgementFlow();
        }

        protected override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.Type.IsScorable() || judgement.Type.IsBonus())
                return;

            judgementsFlow.Push(GetColourForHitResult(judgement.Type));
        }

        public override void Clear() => judgementsFlow.Clear();

        private class JudgementFlow : FillFlowContainer<HitErrorCircle>
        {
            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Reverse();

            public JudgementFlow()
            {
                AutoSizeAxes = Axes.X;
                Height = MAX_DISPLAYED_JUDGEMENTS * (drawable_judgement_size + spacing) - spacing;
                Spacing = new Vector2(0, spacing);
                Direction = FillDirection.Vertical;
                LayoutDuration = animation_duration;
                LayoutEasing = Easing.OutQuint;
            }

            public void Push(Color4 colour)
            {
                Add(new HitErrorCircle(colour, drawable_judgement_size));

                if (Children.Count > MAX_DISPLAYED_JUDGEMENTS)
                    Children.FirstOrDefault(c => !c.IsRemoved)?.Remove();
            }
        }

        internal class HitErrorCircle : Container
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
