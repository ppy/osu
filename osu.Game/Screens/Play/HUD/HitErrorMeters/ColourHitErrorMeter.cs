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
            MaxValue = 30,
            Precision = 1
        };

        [SettingSource("Judgement spacing", "The space between each displayed judgement")]
        public BindableNumber<float> JudgementSpacing { get; } = new BindableNumber<float>(2)
        {
            MinValue = 0,
            MaxValue = 10,
            Precision = 0.1f
        };

        [SettingSource("Judgement shape", "The shape of each displayed judgement")]
        public Bindable<ShapeStyle> JudgementShape { get; } = new Bindable<ShapeStyle>();

        private readonly JudgementFlow judgementsFlow;

        public ColourHitErrorMeter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = judgementsFlow = new JudgementFlow
            {
                Shape = { BindTarget = JudgementShape }
            };
        }

        protected override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.Type.IsScorable() || judgement.Type.IsBonus())
                return;

            judgementsFlow.Push(GetColourForHitResult(judgement.Type), JudgementCount.Value);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            JudgementSpacing.BindValueChanged(_ =>
            {
                judgementsFlow.Height = JudgementCount.Value * (drawable_judgement_size + JudgementSpacing.Value) - JudgementSpacing.Value;
                judgementsFlow.Spacing = new Vector2(0, JudgementSpacing.Value);
            }, true);

            JudgementCount.BindValueChanged(_ =>
            {
                //Used to clear out the overflowing judgement children when the value is lowered
                judgementsFlow.RemoveAll(_ => true, true);
                judgementsFlow.Height = JudgementCount.Value * (drawable_judgement_size + JudgementSpacing.Value) - JudgementSpacing.Value;
            }, true);
        }

        public override void Clear() => judgementsFlow.Clear();

        private class JudgementFlow : FillFlowContainer<HitErrorShape>
        {
            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Reverse();

            public readonly Bindable<ShapeStyle> Shape = new Bindable<ShapeStyle>();

            public JudgementFlow()
            {
                Width = drawable_judgement_size;
                Direction = FillDirection.Vertical;
                LayoutDuration = animation_duration;
                LayoutEasing = Easing.OutQuint;
            }

            public void Push(Color4 colour, int maxErrorShapeCount)
            {
                Add(new HitErrorShape(colour, drawable_judgement_size)
                {
                    Shape = { BindTarget = Shape },
                });

                if (Children.Count > maxErrorShapeCount)
                    Children.FirstOrDefault(c => !c.IsRemoved)?.Remove();
            }
        }

        public class HitErrorShape : Container
        {
            public bool IsRemoved { get; private set; }

            public readonly Bindable<ShapeStyle> Shape = new Bindable<ShapeStyle>();

            private readonly Color4 colour;

            public HitErrorShape(Color4 colour, int size)
            {
                this.colour = colour;
                Size = new Vector2(size);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Shape.BindValueChanged(shape =>
                {
                    switch (shape.NewValue)
                    {
                        case ShapeStyle.Circle:
                            Child = new Circle
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                Colour = colour
                            };
                            break;

                        case ShapeStyle.Square:
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                Colour = colour
                            };
                            break;
                    }
                }, true);

                Child.FadeInFromZero(animation_duration, Easing.OutQuint);
                Child.MoveToY(-DrawSize.Y);
                Child.MoveToY(0, animation_duration, Easing.OutQuint);
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
