// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters
{
    public class ColourHitErrorMeter : HitErrorMeter
    {
        private const int animation_duration = 200;
        private const int drawable_judgement_size = 8;
        private readonly JudgementFlow judgementsFlow;

        [SettingSource("Colour hit number", "number of coloured hits")]
        public BindableNumber<int> HitCircleAmount { get; } = new BindableNumber<int>(20)
        {
            MinValue = 1,
            MaxValue = 30,
            Precision = 1
        };

        [SettingSource("Opacity", "Visibility of object")]
        public BindableNumber<float> HitOpacity { get; } = new BindableNumber<float>(1)
        {
            MinValue = 0.01f,
            MaxValue = 1,
            Precision = .01f
        };

        [SettingSource("Spacing", "space between hit colour circles")]
        public BindableNumber<float> HitSpacing { get; } = new BindableNumber<float>(2)
        {
            MinValue = 0,
            MaxValue = 10,
            Precision = .1f
        };

        public ColourHitErrorMeter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = judgementsFlow = new JudgementFlow();
        }

        protected override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.Type.IsScorable() || judgement.Type.IsBonus())
                return;

            judgementsFlow.Push(GetColourForHitResult(judgement.Type), HitCircleAmount.Value);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            HitOpacity.BindValueChanged(_ => judgementsFlow.Alpha = HitOpacity.Value, true);
            HitSpacing.BindValueChanged(_ => judgementsFlow.Spacing = new Vector2(0, HitSpacing.Value), true);
            HitSpacing.BindValueChanged(_ => judgementsFlow.Height = HitCircleAmount.Value * (drawable_judgement_size + HitSpacing.Value) - HitSpacing.Value, true);
            HitCircleAmount.BindValueChanged(_ => judgementsFlow.Height = HitCircleAmount.Value * (drawable_judgement_size + HitSpacing.Value) - HitSpacing.Value, true);
            HitCircleAmount.BindValueChanged(_ => judgementsFlow.Clear(), true);
        }

        public override void Clear() => judgementsFlow.Clear();

        private class JudgementFlow : FillFlowContainer<HitErrorCircle>
        {
            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Reverse();

            public JudgementFlow()
            {
                Width = drawable_judgement_size;
                Direction = FillDirection.Vertical;
                LayoutDuration = animation_duration;
                LayoutEasing = Easing.OutQuint;
            }

            public void Push(Color4 colour, int amount)
            {
                Add(new HitErrorCircle(colour, drawable_judgement_size));

                if (Children.Count > amount)
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
