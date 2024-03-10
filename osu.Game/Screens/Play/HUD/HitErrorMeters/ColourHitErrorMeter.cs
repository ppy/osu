// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation.HUD;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters
{
    [Cached]
    public partial class ColourHitErrorMeter : HitErrorMeter
    {
        private const int animation_duration = 200;
        private const int drawable_judgement_size = 8;

        [SettingSource(typeof(ColourHitErrorMeterStrings), nameof(ColourHitErrorMeterStrings.JudgementCount), nameof(ColourHitErrorMeterStrings.JudgementCountDescription))]
        public BindableNumber<int> JudgementCount { get; } = new BindableNumber<int>(20)
        {
            MinValue = 1,
            MaxValue = 50,
        };

        [SettingSource(typeof(ColourHitErrorMeterStrings), nameof(ColourHitErrorMeterStrings.JudgementSpacing), nameof(ColourHitErrorMeterStrings.JudgementSpacingDescription))]
        public BindableNumber<float> JudgementSpacing { get; } = new BindableNumber<float>(2)
        {
            MinValue = 0,
            MaxValue = 10,
        };

        [SettingSource(typeof(ColourHitErrorMeterStrings), nameof(ColourHitErrorMeterStrings.JudgementShape), nameof(ColourHitErrorMeterStrings.JudgementShapeDescription))]
        public Bindable<ShapeStyle> JudgementShape { get; } = new Bindable<ShapeStyle>();

        private readonly DrawablePool<HitErrorShape> judgementShapePool;
        private readonly JudgementFlow judgementsFlow;

        public ColourHitErrorMeter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                judgementShapePool = new DrawablePool<HitErrorShape>(50),
                judgementsFlow = new JudgementFlow
                {
                    JudgementShape = { BindTarget = JudgementShape },
                    JudgementSpacing = { BindTarget = JudgementSpacing },
                    JudgementCount = { BindTarget = JudgementCount }
                }
            };
        }

        protected override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.Type.IsScorable() || judgement.Type.IsBonus())
                return;

            judgementsFlow.Push(judgementShapePool.Get(shape => shape.Colour = GetColourForHitResult(judgement.Type)));
        }

        public override void Clear()
        {
            foreach (var j in judgementsFlow)
            {
                j.ClearTransforms();
                j.Expire();
            }
        }

        private partial class JudgementFlow : FillFlowContainer<HitErrorShape>
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

                JudgementCount.BindValueChanged(_ =>
                {
                    removeExtraJudgements();
                    updateMetrics();
                });

                JudgementSpacing.BindValueChanged(_ => updateMetrics(), true);
            }

            public void Push(HitErrorShape shape)
            {
                Add(shape);
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

        public partial class HitErrorShape : PoolableDrawable
        {
            public bool IsRemoved { get; private set; }

            public readonly Bindable<ShapeStyle> Shape = new Bindable<ShapeStyle>();

            [Resolved]
            private ColourHitErrorMeter hitErrorMeter { get; set; } = null!;

            private Container content = null!;

            public HitErrorShape()
            {
                Size = new Vector2(drawable_judgement_size);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                InternalChild = content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                };

                Shape.BindTo(hitErrorMeter.JudgementShape);
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
            }

            protected override void PrepareForUse()
            {
                base.PrepareForUse();

                this.FadeInFromZero(animation_duration, Easing.OutQuint)
                    // On pool re-use, start flow animation from (0,0).
                    .MoveTo(Vector2.Zero);

                content.MoveToY(-DrawSize.Y)
                       .MoveToY(0, animation_duration, Easing.OutQuint);
            }

            protected override void FreeAfterUse()
            {
                base.FreeAfterUse();
                IsRemoved = false;
            }

            public void Remove()
            {
                IsRemoved = true;

                this.FadeOut(animation_duration, Easing.OutQuint)
                    .Expire();
            }
        }

        public enum ShapeStyle
        {
            [LocalisableDescription(typeof(ColourHitErrorMeterStrings), nameof(ColourHitErrorMeterStrings.ShapeStyleCircle))]
            Circle,

            [LocalisableDescription(typeof(ColourHitErrorMeterStrings), nameof(ColourHitErrorMeterStrings.ShapeStyleSquare))]
            Square
        }
    }
}
