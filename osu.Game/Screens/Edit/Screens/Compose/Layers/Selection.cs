// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
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
    public class SelectionBox : CompositeDrawable
    {
        public const float BORDER_RADIUS = 2;

        private readonly MaskContainer maskContainer;

        private readonly SortedList<HitObjectMask> selectedMasks;
        private IEnumerable<HitObjectMask> selectableMasks => maskContainer.AliveMasks;

        private Drawable outline;

        public SelectionBox(MaskContainer maskContainer)
        {
            this.maskContainer = maskContainer;

            selectedMasks = new SortedList<HitObjectMask>(maskContainer.Compare);

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Alpha = 0;

            maskContainer.MaskSelected += onSelected;
            maskContainer.MaskDeselected += onDeselected;
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

        /// <summary>
        /// Handle input on currently selectable or already selected masks.
        /// Keep in mind that selectedMasks may contain masks for non-current objects, which we still want to handle input while selected.
        /// </summary>
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => selectableMasks.Reverse().Concat(selectedMasks).Any(m => m.ReceiveMouseInputAt(screenSpacePos));

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            // If masks are overlapping, make sure we don't change the selection if the overlapped portion is pressed
            if (selectedMasks.Any(m => m.ReceiveMouseInputAt(state.Mouse.NativeState.Position)))
                return true;

            DeselectAll();
            selectableMasks.Reverse().First(m => m.ReceiveMouseInputAt(state.Mouse.NativeState.Position)).Select();

            UpdateVisibility();
            return true;
        }

        protected override bool OnClick(InputState state)
        {
            // If there's only mask, this isn't going to change anything, so we can save on doing some processing here
            if (selectedMasks.Count == 1)
                return true;

            var toSelect = selectedMasks.Reverse().First(m => m.ReceiveMouseInputAt(state.Mouse.NativeState.Position));

            DeselectAll();
            toSelect.Select();

            UpdateVisibility();
            return true;
        }

        protected override bool OnDragStart(InputState state) => true;

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

        protected override bool OnDragEnd(InputState state) => true;

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

        /// <summary>
        /// Deselects all selected <see cref="HitObjectMask"/>s.
        /// </summary>
        public void DeselectAll() => selectedMasks.ToList().ForEach(m => m.Deselect());

        #endregion

        /// <summary>
        /// Updates whether this <see cref="SelectionBox"/> is visible.
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
        }
    }
}
