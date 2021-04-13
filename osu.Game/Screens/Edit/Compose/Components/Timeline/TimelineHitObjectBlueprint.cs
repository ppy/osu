// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineHitObjectBlueprint : SelectionBlueprint
    {
        private const float shadow_radius = 5;
        private const float circle_size = 38;

        public Action<DragEvent> OnDragHandled;

        [UsedImplicitly]
        private readonly Bindable<double> startTime;

        private Bindable<int> indexInCurrentComboBindable;
        private Bindable<int> comboIndexBindable;

        private readonly Drawable circle;

        private readonly Container mainComponents;
        private readonly OsuSpriteText comboIndexText;

        [Resolved]
        private ISkinSource skin { get; set; }

        public TimelineHitObjectBlueprint(HitObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            startTime = hitObject.StartTimeBindable.GetBoundCopy();
            startTime.BindValueChanged(time => X = (float)time.NewValue, true);

            RelativePositionAxes = Axes.X;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddRangeInternal(new Drawable[]
            {
                mainComponents = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
                comboIndexText = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    Y = -1,
                    Font = OsuFont.Default.With(size: circle_size * 0.5f, weight: FontWeight.Regular),
                },
            });

            circle = new ExtendableCircle
            {
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(1, circle_size),
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            };

            mainComponents.Add(circle);

            if (hitObject is IHasDuration)
            {
                mainComponents.Add(new DragArea(hitObject)
                {
                    OnDragHandled = e => OnDragHandled?.Invoke(e)
                });
            }

            updateShadows();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (HitObject is IHasComboInformation comboInfo)
            {
                indexInCurrentComboBindable = comboInfo.IndexInCurrentComboBindable.GetBoundCopy();
                indexInCurrentComboBindable.BindValueChanged(_ => updateComboIndex(), true);

                comboIndexBindable = comboInfo.ComboIndexBindable.GetBoundCopy();
                comboIndexBindable.BindValueChanged(_ => updateComboColour(), true);

                skin.SourceChanged += updateComboColour;
            }
        }

        private void updateComboIndex() => comboIndexText.Text = (indexInCurrentComboBindable.Value + 1).ToString();

        private void updateComboColour()
        {
            if (!(HitObject is IHasComboInformation combo))
                return;

            var comboColours = skin.GetConfig<GlobalSkinColours, IReadOnlyList<Color4>>(GlobalSkinColours.ComboColours)?.Value ?? Array.Empty<Color4>();
            var comboColour = combo.GetComboColour(comboColours);

            if (HitObject is IHasDuration)
                mainComponents.Colour = ColourInfo.GradientHorizontal(comboColour, comboColour.Lighten(0.4f));
            else
                mainComponents.Colour = comboColour;

            var col = mainComponents.Colour.TopLeft.Linear;
            float brightness = col.R + col.G + col.B;

            // decide the combo index colour based on brightness?
            comboIndexText.Colour = brightness > 0.5f ? Color4.Black : Color4.White;
        }

        protected override void Update()
        {
            base.Update();

            // no bindable so we perform this every update
            float duration = (float)(HitObject.GetEndTime() - HitObject.StartTime);

            if (Width != duration)
            {
                Width = duration;

                // kind of haphazard but yeah, no bindables.
                if (HitObject is IHasRepeats repeats)
                    updateRepeats(repeats);
            }
        }

        private Container repeatsContainer;

        private void updateRepeats(IHasRepeats repeats)
        {
            repeatsContainer?.Expire();

            mainComponents.Add(repeatsContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
            });

            for (int i = 0; i < repeats.RepeatCount; i++)
            {
                repeatsContainer.Add(new Circle
                {
                    Size = new Vector2(circle_size / 2),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    X = (float)(i + 1) / (repeats.RepeatCount + 1),
                });
            }
        }

        protected override bool ShouldBeConsideredForInput(Drawable child) => true;

        protected override void OnSelected()
        {
            updateShadows();
        }

        private void updateShadows()
        {
        }

        protected override void OnDeselected()
        {
            updateShadows();
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            circle.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => circle.ScreenSpaceDrawQuad;

        public override Vector2 ScreenSpaceSelectionPoint => ScreenSpaceDrawQuad.TopLeft;

        public class DragArea : Container
        {
            private readonly HitObject hitObject;

            [Resolved]
            private Timeline timeline { get; set; }

            public Action<DragEvent> OnDragHandled;

            public override bool HandlePositionalInput => hitObject != null;

            public DragArea(HitObject hitObject)
            {
                this.hitObject = hitObject;

                CornerRadius = circle_size / 2;
                Masking = true;
                Size = new Vector2(circle_size, 1);
                Anchor = Anchor.CentreRight;
                Origin = Anchor.Centre;
                RelativePositionAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            private bool hasMouseDown;

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                hasMouseDown = true;
                updateState();
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                hasMouseDown = false;
                updateState();
                base.OnMouseUp(e);
            }

            private void updateState()
            {
                Colour = IsHovered || hasMouseDown ? Color4.OrangeRed : Color4.White;
            }

            [Resolved]
            private EditorBeatmap beatmap { get; set; }

            [Resolved]
            private IBeatSnapProvider beatSnapProvider { get; set; }

            [Resolved(CanBeNull = true)]
            private IEditorChangeHandler changeHandler { get; set; }

            protected override bool OnDragStart(DragStartEvent e)
            {
                changeHandler?.BeginChange();
                return true;
            }

            protected override void OnDrag(DragEvent e)
            {
                base.OnDrag(e);

                OnDragHandled?.Invoke(e);

                if (timeline.SnapScreenSpacePositionToValidTime(e.ScreenSpaceMousePosition).Time is double time)
                {
                    switch (hitObject)
                    {
                        case IHasRepeats repeatHitObject:
                            // find the number of repeats which can fit in the requested time.
                            var lengthOfOneRepeat = repeatHitObject.Duration / (repeatHitObject.RepeatCount + 1);
                            var proposedCount = Math.Max(0, (int)Math.Round((time - hitObject.StartTime) / lengthOfOneRepeat) - 1);

                            if (proposedCount == repeatHitObject.RepeatCount)
                                return;

                            repeatHitObject.RepeatCount = proposedCount;
                            beatmap.Update(hitObject);
                            break;

                        case IHasDuration endTimeHitObject:
                            var snappedTime = Math.Max(hitObject.StartTime, beatSnapProvider.SnapTime(time));

                            if (endTimeHitObject.EndTime == snappedTime || Precision.AlmostEquals(snappedTime, hitObject.StartTime, beatmap.GetBeatLengthAtTime(snappedTime)))
                                return;

                            endTimeHitObject.Duration = snappedTime - hitObject.StartTime;
                            beatmap.Update(hitObject);
                            break;
                    }
                }
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                base.OnDragEnd(e);

                OnDragHandled?.Invoke(null);
                changeHandler?.EndChange();
            }
        }

        /// <summary>
        /// A circle with externalised end caps so it can take up the full width of a relative width area.
        /// TODO: figure how to do this with a single circle to avoid pixel-misaligned edges.
        /// </summary>
        public class ExtendableCircle : Container
        {
            private readonly Circle rightCircle;
            private readonly Circle leftCircle;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            {
                return base.ReceivePositionalInputAt(screenSpacePos)
                       || leftCircle.ReceivePositionalInputAt(screenSpacePos)
                       || rightCircle.ReceivePositionalInputAt(screenSpacePos);
            }

            public override Quad ScreenSpaceDrawQuad
            {
                get
                {
                    var leftQuad = leftCircle.ScreenSpaceDrawQuad;

                    if (Width == 0)
                    {
                        return leftQuad;
                    }

                    var rightQuad = rightCircle.ScreenSpaceDrawQuad;

                    return new Quad(leftQuad.TopLeft, rightQuad.TopRight, leftQuad.BottomLeft, rightQuad.BottomRight);
                }
            }

            public ExtendableCircle()
            {
                var effect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = shadow_radius,
                    Colour = Color4.Black.Opacity(0.4f)
                };

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        Height = circle_size,
                        RelativeSizeAxes = Axes.X,
                        Masking = true,
                        AlwaysPresent = true,
                        EdgeEffect = effect,
                    },
                    leftCircle = new Circle
                    {
                        EdgeEffect = effect,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(circle_size)
                    },
                    rightCircle = new Circle
                    {
                        EdgeEffect = effect,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(circle_size)
                    },
                    new Box
                    {
                        Height = circle_size,
                        RelativeSizeAxes = Axes.X,
                    },
                };
            }
        }
    }
}
