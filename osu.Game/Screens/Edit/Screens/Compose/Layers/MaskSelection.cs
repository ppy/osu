// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Lists;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Types;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    /// <summary>
    /// A box which surrounds <see cref="HitObjectMask"/>s and provides interactive handles, context menus etc.
    /// </summary>
    public class MaskSelection : CompositeDrawable
    {
        public const float BORDER_RADIUS = 2;

        private readonly MaskContainer maskContainer;

        private readonly SortedList<HitObjectMask> selectedMasks;

        private Drawable outline;

        public MaskSelection(MaskContainer maskContainer)
        {
            // todo: remove this
            this.maskContainer = maskContainer;

            selectedMasks = new SortedList<HitObjectMask>(maskContainer.Compare);

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Alpha = 0;

            maskContainer.MaskSelected += onSelected;
            maskContainer.MaskDeselected += onDeselected;
            maskContainer.MaskSelectionRequested += onSelectionRequested;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = outline = new Container
            {
                Masking = true,
                BorderThickness = BORDER_RADIUS,
                BorderColour = colours.Yellow,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    AlwaysPresent = true,
                    Alpha = 0
                }
            };
        }

        #region User Input Handling

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => handleInput(state);

        protected override bool OnDragStart(InputState state) => handleInput(state);

        protected override bool OnDragEnd(InputState state) => true;

        private bool handleInput(InputState state)
        {
            if (!selectedMasks.Any(m => m.ReceiveMouseInputAt(state.Mouse.NativeState.PositionMouseDown ?? state.Mouse.NativeState.Position)))
                return false;

            UpdateVisibility();
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            // Todo: Various forms of snapping

            foreach (var mask in selectedMasks)
            {
                switch (mask.HitObject.HitObject)
                {
                    case IHasEditablePosition editablePosition:
                        editablePosition.OffsetPosition(state.Mouse.Delta);
                        break;
                }
            }

            return true;
        }

        #endregion

        #region Selection Handling

        private void onSelected(HitObjectMask mask) => selectedMasks.Add(mask);

        private void onDeselected(HitObjectMask mask)
        {
            selectedMasks.Remove(mask);

            // We don't want to update visibility if > 0, since we may be deselecting masks during drag-selection
            if (selectedMasks.Count == 0)
                UpdateVisibility();
        }

        private void onSelectionRequested(HitObjectMask mask)
        {
            if (GetContainingInputManager().CurrentState.Keyboard.ControlPressed)
            {
                if (mask.State == Visibility.Visible)
                    // we don't want this deselection to affect input for this frame.
                    Schedule(() => mask.Deselect());
                else
                    mask.Select();
            }
            else
            {
                if (mask.State == Visibility.Visible)
                    return;

                maskContainer.DeselectAll();
                mask.Select();
            }

            UpdateVisibility();
        }

        #endregion

        /// <summary>
        /// Updates whether this <see cref="MaskSelection"/> is visible.
        /// </summary>
        internal void UpdateVisibility()
        {
            if (selectedMasks.Count > 0)
                Show();
            else
                Hide();
        }

        protected override void Update()
        {
            base.Update();

            if (selectedMasks.Count == 0)
                return;

            // Move the rectangle to cover the hitobjects
            var topLeft = new Vector2(float.MaxValue, float.MaxValue);
            var bottomRight = new Vector2(float.MinValue, float.MinValue);

            bool hasSelection = false;

            foreach (var mask in selectedMasks)
            {
                topLeft = Vector2.ComponentMin(topLeft, ToLocalSpace(mask.SelectionQuad.TopLeft));
                bottomRight = Vector2.ComponentMax(bottomRight, ToLocalSpace(mask.SelectionQuad.BottomRight));
            }

            topLeft -= new Vector2(5);
            bottomRight += new Vector2(5);

            outline.Size = bottomRight - topLeft;
            outline.Position = topLeft;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            maskContainer.MaskSelected -= onSelected;
            maskContainer.MaskDeselected -= onDeselected;
            maskContainer.MaskSelectionRequested -= onSelectionRequested;
        }
    }
}
