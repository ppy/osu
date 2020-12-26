// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
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
        private const float thickness = 5;
        private const float shadow_radius = 5;
        private const float circle_size = 34;

        public Action<DragEvent> OnDragHandled;

        [UsedImplicitly]
        private readonly Bindable<double> startTime;

        private Bindable<int> indexInCurrentComboBindable;
        private Bindable<int> comboIndexBindable;

        private readonly Circle circle;
        private readonly DragBar dragBar;
        private readonly List<Container> shadowComponents = new List<Container>();
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
                    Font = OsuFont.Numeric.With(size: circle_size / 2, weight: FontWeight.Black),
                },
            });

            circle = new Circle
            {
                Size = new Vector2(circle_size),
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.Centre,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = shadow_radius,
                    Colour = Color4.Black
                },
            };

            shadowComponents.Add(circle);

            if (hitObject is IHasDuration)
            {
                DragBar dragBarUnderlay;
                Container extensionBar;

                mainComponents.AddRange(new Drawable[]
                {
                    extensionBar = new Container
                    {
                        Masking = true,
                        Size = new Vector2(1, thickness),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativePositionAxes = Axes.X,
                        RelativeSizeAxes = Axes.X,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Radius = shadow_radius,
                            Colour = Color4.Black
                        },
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                    circle,
                    // only used for drawing the shadow
                    dragBarUnderlay = new DragBar(null),
                    // cover up the shadow on the join
                    new Box
                    {
                        Height = thickness,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                    },
                    dragBar = new DragBar(hitObject) { OnDragHandled = e => OnDragHandled?.Invoke(e) },
                });

                shadowComponents.Add(dragBarUnderlay);
                shadowComponents.Add(extensionBar);
            }
            else
            {
                mainComponents.Add(circle);
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
                mainComponents.Colour = ColourInfo.GradientHorizontal(comboColour, Color4.White);
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

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            base.ReceivePositionalInputAt(screenSpacePos) ||
            circle.ReceivePositionalInputAt(screenSpacePos) ||
            dragBar?.ReceivePositionalInputAt(screenSpacePos) == true;

        protected override void OnSelected()
        {
            updateShadows();
        }

        private void updateShadows()
        {
            foreach (var s in shadowComponents)
            {
                if (State == SelectionState.Selected)
                {
                    s.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = shadow_radius / 2,
                        Colour = Color4.Orange,
                    };
                }
                else
                {
                    s.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = shadow_radius,
                        Colour = State == SelectionState.Selected ? Color4.Orange : Color4.Black
                    };
                }
            }
        }

        protected override void OnDeselected()
        {
            updateShadows();
        }

        public override Quad SelectionQuad
        {
            get
            {
                // correctly include the circle in the selection quad region, as it is usually outside the blueprint itself.
                var leftQuad = circle.ScreenSpaceDrawQuad;
                var rightQuad = dragBar?.ScreenSpaceDrawQuad ?? ScreenSpaceDrawQuad;

                return new Quad(leftQuad.TopLeft, Vector2.ComponentMax(rightQuad.TopRight, leftQuad.TopRight),
                    leftQuad.BottomLeft, Vector2.ComponentMax(rightQuad.BottomRight, leftQuad.BottomRight));
            }
        }

        public override Vector2 ScreenSpaceSelectionPoint => ScreenSpaceDrawQuad.TopLeft;

        public class DragBar : Container
        {
            private readonly HitObject hitObject;

            [Resolved]
            private Timeline timeline { get; set; }

            public Action<DragEvent> OnDragHandled;

            public override bool HandlePositionalInput => hitObject != null;

            public DragBar(HitObject hitObject)
            {
                this.hitObject = hitObject;

                CornerRadius = 2;
                Masking = true;
                Size = new Vector2(5, 1);
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

                            if (endTimeHitObject.EndTime == snappedTime)
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
    }
}
