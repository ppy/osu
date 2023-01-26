// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
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
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineHitObjectBlueprint : SelectionBlueprint<HitObject>
    {
        private const float circle_size = 38;

        private Container? repeatsContainer;

        public Action<DragEvent?>? OnDragHandled = null!;

        [UsedImplicitly]
        private readonly Bindable<double> startTime;

        private Bindable<int>? indexInCurrentComboBindable;

        private Bindable<int>? comboIndexBindable;
        private Bindable<int>? comboIndexWithOffsetsBindable;

        private Bindable<Color4> displayColourBindable = null!;

        private readonly ExtendableCircle circle;
        private readonly Border border;

        private readonly Container colouredComponents;
        private readonly OsuSpriteText comboIndexText;

        private readonly IBindableList<StreamControlPoint> streamControlPoints = new BindableList<StreamControlPoint>();

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

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

            switch (item)
            {
                case IHasStreamPath streamHitObject:
                    streamControlPoints.BindTo(streamHitObject.StreamPath.ControlPoints);
                    streamControlPoints.BindCollectionChanged(recreateDragComponents, true);

                    break;

                case IHasDuration:
                    colouredComponents.Add(new DragArea(item)
                    {
                        OnDragHandled = e => OnDragHandled?.Invoke(e)
                    });
                    break;
            }
        }

        private void recreateDragComponents(object o, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            colouredComponents.Clear();
            addDragComponents(Item);
        }

        private void addDragComponents(HitObject item)
        {
            if (item is not IHasStreamPath streamHitObject) return;

            for (int i = 1; i < streamHitObject.StreamPath.ControlPoints.Count; i++)
            {
                colouredComponents.Add(new DragArea(item, i)
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

                case IHasMultipleComboInformation:
                    Item.DefaultsApplied += _ => updateColour();
                    skin.SourceChanged += updateColour;
                    break;

                case IHasComboInformation comboInfo:
                    indexInCurrentComboBindable = comboInfo.IndexInCurrentComboBindable.GetBoundCopy();
                    indexInCurrentComboBindable.BindValueChanged(_ =>
                    {
                        comboIndexText.Text = (indexInCurrentComboBindable.Value + 1).ToString();
                    }, true);

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

        private void updateColour()
        {
            if (IsSelected)
                border.Show();
            else
                border.Hide();

            Color4 colour;

            switch (Item)
            {
                case IHasStreamPath hasStreamPath and IHasMultipleComboInformation hasCombos when hasStreamPath.Duration > 0:
                    // Build a multi-colour gradient to represent each combo colour in the stream path
                    int i = 0;
                    Color4? prevColour = null;
                    double segmentStart = 0;
                    var streamPath = hasStreamPath.StreamPath.GetStreamPath();

                    var colourContainer = new Container
                    {
                        Depth = -1,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = circle_size / 2f }
                    };

                    foreach (var comboObject in hasCombos.ComboObjects)
                    {
                        if (i == streamPath.Count) break;

                        double time = streamPath[i++].Item2;
                        var currentColour = comboObject.GetComboColour(skin);

                        if (!prevColour.HasValue)
                        {
                            // Add a bit of colour for the part until before the first tick
                            colourContainer.Add(new Box
                            {
                                RelativeSizeAxes = Axes.Y,
                                Width = circle_size / 2f,
                                X = -circle_size / 2f,
                                Colour = currentColour
                            });

                            prevColour = currentColour;
                            continue;
                        }

                        if (currentColour != prevColour.Value || i == streamPath.Count)
                        {
                            // Add colour of this stream segment and start a gradient about 600 ms before the next segment
                            double segmentDuration = time - segmentStart;
                            double split = time - Math.Min(segmentDuration, 600);

                            colourContainer.Add(new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                Width = (float)((split - segmentStart) / hasStreamPath.Duration),
                                X = (float)(segmentStart / hasStreamPath.Duration),
                                Colour = prevColour.Value
                            });
                            colourContainer.Add(new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                Width = (float)((time - split) / hasStreamPath.Duration),
                                X = (float)(split / hasStreamPath.Duration),
                                Colour = ColourInfo.GradientHorizontal(prevColour.Value, currentColour)
                            });

                            segmentStart = time;
                        }

                        prevColour = currentColour;
                    }

                    if (prevColour.HasValue)
                    {
                        // Add a bit of colour for the part after the last tick
                        colourContainer.Add(new Box
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = circle_size / 2f,
                            Anchor = Anchor.TopRight,
                            Colour = prevColour.Value
                        });
                    }

                    circle.Content.Child = colourContainer;
                    colour = Color4.White;
                    break;

                case IHasDisplayColour displayColour:
                    colour = displayColour.DisplayColour.Value;
                    break;

                case IHasComboInformation combo:
                    colour = combo.GetComboColour(skin);
                    break;

                default:
                    colour = colourProvider.Highlight1;
                    break;
            }

            if (Item is IHasDuration duration && duration.Duration > 0)
                circle.Colour = ColourInfo.GradientHorizontal(colour, colour.Lighten(0.4f));
            else
                circle.Colour = colour;

            var averageColour = Interpolation.ValueAt(0.5, circle.Colour.TopLeft, circle.Colour.TopRight, 0, 1);
            colouredComponents.Colour = OsuColour.ForegroundTextColourFor(averageColour);
        }

        private SamplePointPiece? sampleOverrideDisplay;
        private DifficultyPointPiece? difficultyOverrideDisplay;

        private DifficultyControlPoint difficultyControlPoint = null!;
        private SampleControlPoint sampleControlPoint = null!;

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

            if (!ReferenceEquals(difficultyControlPoint, Item.DifficultyControlPoint))
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

            if (!ReferenceEquals(sampleControlPoint, Item.SampleControlPoint))
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

        private partial class Tick : Circle
        {
            public Tick()
            {
                Size = new Vector2(circle_size / 4);
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.Centre;
                RelativePositionAxes = Axes.X;
            }
        }

        public partial class DragArea : Circle
        {
            private readonly HitObject? hitObject;

            private readonly int? index;

            [Resolved]
            private EditorBeatmap beatmap { get; set; } = null!;

            [Resolved]
            private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

            [Resolved]
            private Timeline timeline { get; set; } = null!;

            [Resolved]
            private IEditorChangeHandler? changeHandler { get; set; }

            private ScheduledDelegate? dragOperation;

            public Action<DragEvent?>? OnDragHandled;

            public override bool HandlePositionalInput => hitObject != null;

            public DragArea(HitObject? hitObject, int? index = null)
            {
                this.hitObject = hitObject;
                this.index = index;

                CornerRadius = circle_size / 2;
                Masking = true;
                Size = new Vector2(circle_size, 1);
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.Centre;
                RelativePositionAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;
                X = 1;

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

            protected override void Update()
            {
                base.Update();

                // Update position to stream control point time
                if (index.HasValue && hitObject is IHasStreamPath streamHitObject)
                    X = (float)(streamHitObject.StreamPath.ControlPoints[index.Value].Time / streamHitObject.Duration);
            }

            protected override bool OnScroll(ScrollEvent e)
            {
                if (!index.HasValue || hitObject is not IHasStreamPath streamHitObject || !e.ShiftPressed) return base.OnScroll(e);

                // Change acceleration of stream control point
                changeHandler?.BeginChange();
                streamHitObject.StreamPath.ControlPoints[index.Value].Acceleration += e.ScrollDelta.X * 0.5d;
                beatmap.Update(hitObject);
                changeHandler?.EndChange();
                return true;
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                changeHandler?.BeginChange();
                return true;
            }

            protected override void OnDrag(DragEvent e)
            {
                base.OnDrag(e);

                // schedule is temporary to ensure we don't process multiple times on a single update frame. we need to find a better method of doing this.
                // without it, a hitobject's endtime may not always be in a valid state (ie. sliders, which needs to recompute their path).
                dragOperation?.Cancel();
                dragOperation = Scheduler.Add(() =>
                {
                    OnDragHandled?.Invoke(e);

                    if (timeline.FindSnappedPositionAndTime(e.ScreenSpaceMousePosition).Time is double time)
                    {
                        switch (hitObject)
                        {
                            case IHasRepeats repeatHitObject:
                                double proposedDuration = time - hitObject.StartTime;

                                if (e.CurrentState.Keyboard.ShiftPressed)
                                {
                                    if (ReferenceEquals(hitObject.DifficultyControlPoint, DifficultyControlPoint.DEFAULT))
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

                            case IHasStreamPath streamHitObject:
                                if (!index.HasValue) return;

                                var controlPoints = streamHitObject.StreamPath.ControlPoints;
                                double prevTime = index > 0 ? controlPoints[index.Value - 1].Time : 0;
                                double nextTime = index < controlPoints.Count - 1 ? controlPoints[index.Value + 1].Time : double.PositiveInfinity;
                                double beatLength = beatmap.GetBeatLengthAtTime(time);
                                double minTime = prevTime + beatLength;
                                double maxTime = nextTime - beatLength;
                                double clippedTime = MathHelper.Clamp(time - hitObject.StartTime, minTime, maxTime);

                                if (clippedTime == controlPoints[index.Value].Time || Precision.DefinitelyBigger(clippedTime, maxTime) || Precision.DefinitelyBigger(minTime, clippedTime)) return;

                                controlPoints[index.Value].Time = clippedTime;
                                controlPoints[index.Value].BeatLength = beatmap.GetBeatLengthAtTime(clippedTime);
                                beatmap.Update(hitObject);
                                break;

                            case IHasDuration endTimeHitObject:
                                double snappedTime = Math.Max(hitObject.StartTime + beatSnapProvider.GetBeatLengthAtTime(hitObject.StartTime), beatSnapProvider.SnapTime(time));

                                if (endTimeHitObject.EndTime == snappedTime)
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

                dragOperation?.Cancel();
                dragOperation = null;

                changeHandler?.EndChange();
                OnDragHandled?.Invoke(null);
            }
        }

        public partial class Border : ExtendableCircle
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
        public partial class ExtendableCircle : CompositeDrawable
        {
            public readonly Circle Content;

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
