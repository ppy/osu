// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        [SettingSource("Judgement count", "The number of displayed judgements")]
        public BindableNumber<int> JudgementCount { get; } = new BindableNumber<int>(20)
        {
            MinValue = 1,
            MaxValue = 50,
        };

        [SettingSource("Judgement spacing", "The space between each displayed judgement")]
        public BindableNumber<float> JudgementSpacing { get; } = new BindableNumber<float>(2)
        {
            MinValue = 0,
            MaxValue = 10,
        };

        [SettingSource("Judgement shape", "The shape of each displayed judgement")]
        public Bindable<ShapeStyle> JudgementShape { get; } = new Bindable<ShapeStyle>();

        private readonly JudgementFlow judgementsFlow;

        public ColourHitErrorMeter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = judgementsFlow = new JudgementFlow
            {
                JudgementShape = { BindTarget = JudgementShape },
                JudgementSpacing = { BindTarget = JudgementSpacing },
                JudgementCount = { BindTarget = JudgementCount }
            };
        }

        protected override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.Type.IsScorable() || judgement.Type.IsBonus())
                return;

            judgementsFlow.Push(GetColourForHitResult(judgement.Type));
        }

        public override void Clear() => judgementsFlow.Clear();

        private class JudgementFlow : FillFlowContainer<HitErrorShape>
        {
            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Reverse();

            public readonly Bindable<ShapeStyle> JudgementShape = new Bindable<ShapeStyle>();

            public readonly Bindable<float> JudgementSpacing = new Bindable<float>();

            public readonly Bindable<int> JudgementCount = new Bindable<int>();

            public JudgementFlow()
            {
                Width = drawable_judgement_size;
                Direction = FillDirection.Vertical;
                LayoutDuration = animation_duration;
                LayoutEasing = Easing.OutQuint;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                JudgementCount.BindValueChanged(count =>
                {
                    removeExtraJudgements();
                    updateMetrics();
                });

                JudgementSpacing.BindValueChanged(_ => updateMetrics(), true);
            }

            public void Push(Color4 colour)
            {
                Add(new HitErrorShape(colour, drawable_judgement_size)
                {
                    Shape = { BindTarget = JudgementShape },
                });

                removeExtraJudgements();
            }

            private void removeExtraJudgements()
            {
                var remainingChildren = Children.Where(c => !c.IsRemoved);

                while (remainingChildren.Count() > JudgementCount.Value)
                    remainingChildren.First().Remove();
            }

            private void updateMetrics()
            {
                Height = JudgementCount.Value * (drawable_judgement_size + JudgementSpacing.Value) - JudgementSpacing.Value;
                Spacing = new Vector2(0, JudgementSpacing.Value);
            }
        }

        public class HitErrorShape : Container
        {
            public bool IsRemoved { get; private set; }

            public readonly Bindable<ShapeStyle> Shape = new Bindable<ShapeStyle>();

            private readonly Color4 colour;

            private Container content = null!;

            public HitErrorShape(Color4 colour, int size)
            {
                this.colour = colour;
                Size = new Vector2(size);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Child = content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour
                };

                Shape.BindValueChanged(shape =>
                {
                    switch (shape.NewValue)
                    {
                        case ShapeStyle.Circle:
                            content.Child = new Circle { RelativeSizeAxes = Axes.Both };
                            break;

                        case ShapeStyle.Square:
                            content.Child = new Box { RelativeSizeAxes = Axes.Both };
                            break;
                    }
                }, true);

                content.FadeInFromZero(animation_duration, Easing.OutQuint);
                content.MoveToY(-DrawSize.Y);
                content.MoveToY(0, animation_duration, Easing.OutQuint);
            }

            public void Remove()
            {
                IsRemoved = true;

                this.FadeOut(animation_duration, Easing.OutQuint).Expire();
            }
        }

        public enum ShapeStyle
        {
            Circle,
            Square
        }
    }
}
