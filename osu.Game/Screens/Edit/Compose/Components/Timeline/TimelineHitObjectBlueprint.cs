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
    public class TimelineHitObjectBlueprint : SelectionBlueprint<HitObject>
    {
        private const float circle_size = 38;

        private Container repeatsContainer;

        public Action<DragEvent> OnDragHandled;

        [UsedImplicitly]
        private readonly Bindable<double> startTime;

        private Bindable<int> indexInCurrentComboBindable;
        private Bindable<int> comboIndexBindable;

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

            if (Item is IHasComboInformation comboInfo)
            {
                indexInCurrentComboBindable = comboInfo.IndexInCurrentComboBindable.GetBoundCopy();
                indexInCurrentComboBindable.BindValueChanged(_ => updateComboIndex(), true);

                comboIndexBindable = comboInfo.ComboIndexBindable.GetBoundCopy();
                comboIndexBindable.BindValueChanged(_ => updateComboColour(), true);

                skin.SourceChanged += updateComboColour;
            }
        }

        protected override void OnSelected()
        {
            // base logic hides selected blueprints when not selected, but timeline doesn't do that.
            updateComboColour();
        }

        protected override void OnDeselected()
        {
            // base logic hides selected blueprints when not selected, but timeline doesn't do that.
            updateComboColour();
        }

        private void updateComboIndex() => comboIndexText.Text = (indexInCurrentComboBindable.Value + 1).ToString();

        private void updateComboColour()
        {
            if (!(Item is IHasComboInformation combo))
                return;

            var comboColours = skin.GetConfig<GlobalSkinColours, IReadOnlyList<Color4>>(GlobalSkinColours.ComboColours)?.Value ?? Array.Empty<Color4>();
            var comboColour = combo.GetComboColour(comboColours);

            if (IsSelected)
            {
                border.Show();
                comboColour = comboColour.Lighten(0.3f);
            }
            else
            {
                border.Hide();
            }

            if (Item is IHasDuration duration && duration.Duration > 0)
                circle.Colour = ColourInfo.GradientHorizontal(comboColour, comboColour.Lighten(0.4f));
            else
                circle.Colour = comboColour;

            var col = circle.Colour.TopLeft.Linear;
            colouredComponents.Colour = OsuColour.ForegroundTextColourFor(col);
        }

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
                repeatsContainer.Add(new Circle
                {
                    Size = new Vector2(circle_size / 3),
                    Alpha = 0.2f,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    X = (float)(i + 1) / (repeats.RepeatCount + 1),
                });
            }
        }

        protected override bool ShouldBeConsideredForInput(Drawable child) => true;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            circle.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => circle.ScreenSpaceDrawQuad;

        public override Vector2 ScreenSpaceSelectionPoint => ScreenSpaceDrawQuad.TopLeft;

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
                if (hasMouseDown)
                {
                    this.ScaleTo(0.7f, 200, Easing.OutQuint);
                }
                else if (IsHovered)
                {
                    this.ScaleTo(0.8f, 200, Easing.OutQuint);
                }
                else
                {
                    this.ScaleTo(0.6f, 200, Easing.OutQuint);
                }

                this.FadeTo(IsHovered || hasMouseDown ? 0.8f : 0.2f, 200, Easing.OutQuint);
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
