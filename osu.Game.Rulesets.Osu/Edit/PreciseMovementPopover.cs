// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class PreciseMovementPopover : OsuPopover
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        private readonly Dictionary<HitObject, Vector2> initialPositions = new Dictionary<HitObject, Vector2>();
        private RectangleF initialSurroundingQuad;

        private BindableNumber<float> xBindable = null!;
        private BindableNumber<float> yBindable = null!;

        private SliderWithTextBoxInput<float> xInput = null!;
        private OsuCheckbox relativeCheckbox = null!;

        public PreciseMovementPopover()
        {
            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                Width = 220,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(20),
                Children = new Drawable[]
                {
                    xInput = new SliderWithTextBoxInput<float>("X:")
                    {
                        Current = xBindable = new BindableNumber<float>
                        {
                            Precision = 1,
                        },
                        Instantaneous = true,
                        TabbableContentContainer = this,
                    },
                    new SliderWithTextBoxInput<float>("Y:")
                    {
                        Current = yBindable = new BindableNumber<float>
                        {
                            Precision = 1,
                        },
                        Instantaneous = true,
                        TabbableContentContainer = this,
                    },
                    relativeCheckbox = new OsuCheckbox(false)
                    {
                        RelativeSizeAxes = Axes.X,
                        LabelText = "Relative movement",
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScheduleAfterChildren(() => xInput.TakeFocus());
        }

        protected override void PopIn()
        {
            base.PopIn();
            editorBeatmap.BeginChange();
            initialPositions.AddRange(editorBeatmap.SelectedHitObjects.Where(ho => ho is not Spinner).Select(ho => new KeyValuePair<HitObject, Vector2>(ho, ((IHasPosition)ho).Position)));
            initialSurroundingQuad = GeometryUtils.GetSurroundingQuad(initialPositions.Keys.Cast<IHasPosition>()).AABBFloat;

            Debug.Assert(initialPositions.Count > 0);

            if (initialPositions.Count > 1)
            {
                relativeCheckbox.Current.Value = true;
                relativeCheckbox.Current.Disabled = true;
            }

            relativeCheckbox.Current.BindValueChanged(_ => relativeChanged(), true);
            xBindable.BindValueChanged(_ => applyPosition());
            yBindable.BindValueChanged(_ => applyPosition());
        }

        protected override void PopOut()
        {
            base.PopOut();
            if (IsLoaded) editorBeatmap.EndChange();
        }

        private void relativeChanged()
        {
            // reset bindable bounds to something that is guaranteed to be larger than any previous value.
            // this prevents crashes that can happen in the middle of changing the bounds, as updating both bound ends at the same is not atomic -
            // if the old and new bounds are disjoint, assigning X first can produce a situation where MinValue > MaxValue.
            (xBindable.MinValue, xBindable.MaxValue) = (float.MinValue, float.MaxValue);
            (yBindable.MinValue, yBindable.MaxValue) = (float.MinValue, float.MaxValue);

            float previousX = xBindable.Value;
            float previousY = yBindable.Value;

            if (relativeCheckbox.Current.Value)
            {
                xBindable.MinValue = 0 - Math.Max(initialSurroundingQuad.TopLeft.X, 0);
                xBindable.MaxValue = OsuPlayfield.BASE_SIZE.X - Math.Min(initialSurroundingQuad.BottomRight.X, OsuPlayfield.BASE_SIZE.X);

                yBindable.MinValue = 0 - Math.Max(initialSurroundingQuad.TopLeft.Y, 0);
                yBindable.MaxValue = OsuPlayfield.BASE_SIZE.Y - Math.Min(initialSurroundingQuad.BottomRight.Y, OsuPlayfield.BASE_SIZE.Y);

                xBindable.Default = yBindable.Default = 0;

                if (initialPositions.Count == 1)
                {
                    var initialPosition = initialPositions.Single().Value;
                    xBindable.Value = previousX - initialPosition.X;
                    yBindable.Value = previousY - initialPosition.Y;
                }
            }
            else
            {
                Debug.Assert(initialPositions.Count == 1);
                var initialPosition = initialPositions.Single().Value;

                var quadRelativeToPosition = new RectangleF(initialSurroundingQuad.Location - initialPosition, initialSurroundingQuad.Size);

                if (initialSurroundingQuad.Width < OsuPlayfield.BASE_SIZE.X)
                {
                    xBindable.MinValue = 0 - quadRelativeToPosition.TopLeft.X;
                    xBindable.MaxValue = OsuPlayfield.BASE_SIZE.X - quadRelativeToPosition.BottomRight.X;
                }
                else
                    xBindable.MinValue = xBindable.MaxValue = initialPosition.X;

                if (initialSurroundingQuad.Height < OsuPlayfield.BASE_SIZE.Y)
                {
                    yBindable.MinValue = 0 - quadRelativeToPosition.TopLeft.Y;
                    yBindable.MaxValue = OsuPlayfield.BASE_SIZE.Y - quadRelativeToPosition.BottomRight.Y;
                }
                else
                    yBindable.MinValue = yBindable.MaxValue = initialPosition.Y;

                xBindable.Default = initialPosition.X;
                yBindable.Default = initialPosition.Y;

                xBindable.Value = xBindable.Default + previousX;
                yBindable.Value = yBindable.Default + previousY;
            }
        }

        private void applyPosition()
        {
            // can happen if popover is dismissed by a keyboard key press while dragging UI controls
            // it doesn't cause a crash, but it looks wrong
            if (!editorBeatmap.TransactionActive)
                return;

            editorBeatmap.PerformOnSelection(ho =>
            {
                if (!initialPositions.TryGetValue(ho, out var initialPosition))
                    return;

                var pos = new Vector2(xBindable.Value, yBindable.Value);
                if (relativeCheckbox.Current.Value)
                    ((IHasPosition)ho).Position = initialPosition + pos;
                else
                    ((IHasPosition)ho).Position = pos;
            });
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.Select && !e.Repeat)
            {
                this.HidePopover();
                return true;
            }

            return base.OnPressed(e);
        }
    }
}
