// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
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
    public class TimelineHitObjectBlueprint : SelectionBlueprint<HitObject>
    {
        private const float circle_size = 38;

        private Container repeatsContainer;

        public Action<DragEvent> OnDragHandled;

        [UsedImplicitly]
        private readonly Bindable<double> startTime;

        private Bindable<int> indexInCurrentComboBindable;

        private Bindable<int> comboIndexBindable;
        private Bindable<int> comboIndexWithOffsetsBindable;

        private Bindable<Color4> displayColourBindable;

        private readonly ExtendableCircle circle;
        private readonly Border border;

        private readonly Container colouredComponents;
        private readonly OsuSpriteText comboIndexText;

        [Resolved]
        private ISkinSource skin { get; set; }

        public TimelineHitObjectBlueprint(HitObject item)
            : base(item)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            startTime = item.StartTimeBindable.GetBoundCopy();
            startTime.BindValueChanged(time => X = (float)time.NewValue, true);

            RelativePositionAxes = Axes.X;

            RelativeSizeAxes = Axes.X;
            Height = circle_size;

            AddRangeInternal(new Drawable[]
            {
                circle = new ExtendableCircle
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                border = new Border
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                colouredComponents = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        comboIndexText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.Centre,
                            Y = -1,
                            Font = OsuFont.Default.With(size: circle_size * 0.5f, weight: FontWeight.Regular),
                        },
                    }
                },
            });

            if (item is IHasDuration)
            {
                colouredComponents.Add(new DragArea(item)
                {
                    OnDragHandled = e => OnDragHandled?.Invoke(e)
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            switch (Item)
            {
                case IHasDisplayColour displayColour:
                    displayColourBindable = displayColour.DisplayColour.GetBoundCopy();
                    displayColourBindable.BindValueChanged(_ => updateColour(), true);
                    break;

                case IHasComboInformation comboInfo:
                    indexInCurrentComboBindable = comboInfo.IndexInCurrentComboBindable.GetBoundCopy();
                    indexInCurrentComboBindable.BindValueChanged(_ => updateComboIndex(), true);

                    comboIndexBindable = comboInfo.ComboIndexBindable.GetBoundCopy();
                    comboIndexWithOffsetsBindable = comboInfo.ComboIndexWithOffsetsBindable.GetBoundCopy();

                    comboIndexBindable.BindValueChanged(_ => updateColour());
                    comboIndexWithOffsetsBindable.BindValueChanged(_ => updateColour(), true);

                    skin.SourceChanged += updateColour;
                    break;
            }
        }

        protected override void OnSelected()
        {
            // base logic hides selected blueprints when not selected, but timeline doesn't do that.
            updateColour();
        }

        protected override void OnDeselected()
        {
            // base logic hides selected blueprints when not selected, but timeline doesn't do that.
            updateColour();
        }

        private void updateComboIndex() => comboIndexText.Text = (indexInCurrentComboBindable.Value + 1).ToString();

        private void updateColour()
        {
            Color4 colour;

            switch (Item)
            {
                case IHasDisplayColour displayColour:
                    colour = displayColour.DisplayColour.Value;
                    break;

                case IHasComboInformation combo:
                    colour = combo.GetComboColour(skin);
                    break;

                default:
                    return;
            }

            if (IsSelected)
                border.Show();
            else
                border.Hide();

            if (Item is IHasDuration duration && duration.Duration > 0)
                circle.Colour = ColourInfo.GradientHorizontal(colour, colour.Lighten(0.4f));
            else
                circle.Colour = colour;

            var col = circle.Colour.TopLeft.Linear;
            colouredComponents.Colour = OsuColour.ForegroundTextColourFor(col);
        }

        private SamplePointPiece sampleOverrideDisplay;
        private DifficultyPointPiece difficultyOverrideDisplay;

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        private DifficultyControlPoint difficultyControlPoint;
        private SampleControlPoint sampleControlPoint;

        protected override void Update()
        {
            base.Update();

            // no bindable so we perform this every update
            float duration = (float)(Item.GetEndTime() - Item.StartTime);

            if (Width != duration)
            {
                Width = duration;

                // kind of haphazard but yeah, no bindables.
                if (Item is IHasRepeats repeats)
                    updateRepeats(repeats);
            }

            if (difficultyControlPoint != Item.DifficultyControlPoint)
            {
                difficultyControlPoint = Item.DifficultyControlPoint;
                difficultyOverrideDisplay?.Expire();

                if (Item.DifficultyControlPoint != null && Item is IHasDistance)
                {
                    AddInternal(difficultyOverrideDisplay = new DifficultyPointPiece(Item)
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.BottomCentre
                    });
                }
            }

            if (sampleControlPoint != Item.SampleControlPoint)
            {
                sampleControlPoint = Item.SampleControlPoint;
                sampleOverrideDisplay?.Expire();

                if (Item.SampleControlPoint != null)
                {
                    AddInternal(sampleOverrideDisplay = new SamplePointPiece(Item)
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.TopCentre
                    });
                }
            }
        }

        private void updateRepeats(IHasRepeats repeats)
        {
            repeatsContainer?.Expire();

            colouredComponents.Add(repeatsContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
            });

            for (int i = 0; i < repeats.RepeatCount; i++)
            {
                repeatsContainer.Add(new Tick
                {
                    X = (float)(i + 1) / (repeats.RepeatCount + 1)
                });
            }
        }

        protected override bool ShouldBeConsideredForInput(Drawable child) => true;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            circle.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => circle.ScreenSpaceDrawQuad;

        public override Vector2 ScreenSpaceSelectionPoint => ScreenSpaceDrawQuad.TopLeft;

        private class Tick : Circle
        {
            public Tick()
            {
                Size = new Vector2(circle_size / 4);
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.Centre;
                RelativePositionAxes = Axes.X;
            }
        }

        public class DragArea : Circle
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

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
                FinishTransforms();
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
                float scale = 0.5f;

                if (hasMouseDown)
                    scale = 0.6f;
                else if (IsHovered)
                    scale = 0.7f;

                this.ScaleTo(scale, 200, Easing.OutQuint);
                this.FadeTo(IsHovered || hasMouseDown ? 1f : 0.9f, 200, Easing.OutQuint);
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

            private ScheduledDelegate dragOperation;

            protected override void OnDrag(DragEvent e)
            {
                base.OnDrag(e);

                // schedule is temporary to ensure we don't process multiple times on a single update frame. we need to find a better method of doing this.
                // without it, a hitobject's endtime may not always be in a valid state (ie. sliders, which needs to recompute their path).
                dragOperation?.Cancel();
                dragOperation = Scheduler.Add(() =>
                {
                    OnDragHandled?.Invoke(e);

                    if (timeline.SnapScreenSpacePositionToValidTime(e.ScreenSpaceMousePosition).Time is double time)
                    {
                        switch (hitObject)
                        {
                            case IHasRepeats repeatHitObject:
                                double proposedDuration = time - hitObject.StartTime;

                                if (e.CurrentState.Keyboard.ShiftPressed)
                                {
                                    if (hitObject.DifficultyControlPoint == DifficultyControlPoint.DEFAULT)
                                        hitObject.DifficultyControlPoint = new DifficultyControlPoint();

                                    double newVelocity = hitObject.DifficultyControlPoint.SliderVelocity * (repeatHitObject.Duration / proposedDuration);

                                    if (Precision.AlmostEquals(newVelocity, hitObject.DifficultyControlPoint.SliderVelocity))
                                        return;

                                    hitObject.DifficultyControlPoint.SliderVelocity = newVelocity;
                                    beatmap.Update(hitObject);
                                }
                                else
                                {
                                    // find the number of repeats which can fit in the requested time.
                                    double lengthOfOneRepeat = repeatHitObject.Duration / (repeatHitObject.RepeatCount + 1);
                                    int proposedCount = Math.Max(0, (int)Math.Round(proposedDuration / lengthOfOneRepeat) - 1);

                                    if (proposedCount == repeatHitObject.RepeatCount)
                                        return;

                                    repeatHitObject.RepeatCount = proposedCount;
                                    beatmap.Update(hitObject);
                                }

                                break;

                            case IHasDuration endTimeHitObject:
                                double snappedTime = Math.Max(hitObject.StartTime, beatSnapProvider.SnapTime(time));

                                if (endTimeHitObject.EndTime == snappedTime || Precision.AlmostEquals(snappedTime, hitObject.StartTime, beatmap.GetBeatLengthAtTime(snappedTime)))
                                    return;

                                endTimeHitObject.Duration = snappedTime - hitObject.StartTime;
                                beatmap.Update(hitObject);
                                break;
                        }
                    }
                });
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                base.OnDragEnd(e);

                OnDragHandled?.Invoke(null);
                changeHandler?.EndChange();
            }
        }

        public class Border : ExtendableCircle
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Content.Child.Alpha = 0;
                Content.Child.AlwaysPresent = true;

                Content.BorderColour = colours.Yellow;
                Content.EdgeEffect = new EdgeEffectParameters();
            }
        }

        /// <summary>
        /// A circle with externalised end caps so it can take up the full width of a relative width area.
        /// </summary>
        public class ExtendableCircle : CompositeDrawable
        {
            protected readonly Circle Content;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Content.ReceivePositionalInputAt(screenSpacePos);

            public override Quad ScreenSpaceDrawQuad => Content.ScreenSpaceDrawQuad;

            public ExtendableCircle()
            {
                Padding = new MarginPadding { Horizontal = -circle_size / 2f };
                InternalChild = Content = new Circle
                {
                    BorderColour = OsuColour.Gray(0.75f),
                    BorderThickness = 4,
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                        Colour = Color4.Black.Opacity(0.4f)
                    }
                };
            }
        }
    }
}
