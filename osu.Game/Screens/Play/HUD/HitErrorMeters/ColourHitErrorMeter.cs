// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        private const int default_shape_alpha = 0;
        private const int animation_duration = 200;
        private const int drawable_judgement_size = 8;

        [SettingSource("Hit error amount", "Number of hit error shapes")]
        public BindableNumber<int> HitShapeCount { get; } = new BindableNumber<int>(20)
        {
            MinValue = 1,
            MaxValue = 30,
            Precision = 1
        };

        [SettingSource("Opacity", "Visibility of object")]
        public BindableNumber<float> HitShapeOpacity { get; } = new BindableNumber<float>(1)
        {
            MinValue = 0.01f,
            MaxValue = 1,
            Precision = .01f,
        };

        [SettingSource("Spacing", "Space between hit error shapes")]
        public BindableNumber<float> HitShapeSpacing { get; } = new BindableNumber<float>(2)
        {
            MinValue = 0,
            MaxValue = 10,
            Precision = .1f
        };

        [SettingSource("Shape", "What shape to use for hit errors")]
        public Bindable<ShapeStyle> HitShape { get; } = new Bindable<ShapeStyle>();

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

            judgementsFlow.Push(GetColourForHitResult(judgement.Type), HitShapeCount.Value);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            HitShapeOpacity.BindValueChanged(_ => judgementsFlow.Alpha = HitShapeOpacity.Value, true);
            HitShapeSpacing.BindValueChanged(_ =>
            {
                judgementsFlow.Height = HitShapeCount.Value * (drawable_judgement_size + HitShapeSpacing.Value) - HitShapeSpacing.Value;
                judgementsFlow.Spacing = new Vector2(0, HitShapeSpacing.Value);
            }, true);
            HitShapeCount.BindValueChanged(_ =>
            {
                judgementsFlow.Clear();
                judgementsFlow.Height = HitShapeCount.Value * (drawable_judgement_size + HitShapeSpacing.Value) - HitShapeSpacing.Value;
            }, true);
            HitShape.BindValueChanged(_ =>
            {
                judgementsFlow.ValueParser = getShapeStyle(HitShape.Value);
                judgementsFlow.Clear();
            }, true);
        }

        public override void Clear() => judgementsFlow.Clear();

        private class JudgementFlow : FillFlowContainer<HitErrorShape>
        {
            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Reverse();
            internal string ValueParser = null!;

            public JudgementFlow()
            {
                Width = drawable_judgement_size;
                Direction = FillDirection.Vertical;
                LayoutDuration = animation_duration;
                LayoutEasing = Easing.OutQuint;
            }

            public void Push(Color4 colour, int maxErrorShapeCount)
            {
                Add(new HitErrorShape(colour, drawable_judgement_size, ValueParser));

                if (Children.Count > maxErrorShapeCount)
                    Children.FirstOrDefault(c => !c.IsRemoved)?.Remove();
            }
        }

        private class HitErrorShape : Container
        {
            public bool IsRemoved { get; private set; }

            public HitErrorShape(Color4 colour, int size, string shape)
            {
                Size = new Vector2(size);

                switch (shape)
                {
                    case "circle":
                        Child = new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = default_shape_alpha,
                            Colour = colour
                        };
                        break;

                    case "square":
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = default_shape_alpha,
                            Colour = colour
                        };
                        break;
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

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

        private string getShapeStyle(ShapeStyle shape)
        {
            switch (shape)
            {
                case ShapeStyle.Circle:
                    return "circle";

                case ShapeStyle.Square:
                    return "square";

                default:
                    throw new ArgumentOutOfRangeException(nameof(shape), shape, @"Unsupported animation style");
            }
        }

        public enum ShapeStyle
        {
            Circle,
            Square
        }
    }
}
