// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Types;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    /// <summary>
    /// A box which surrounds <see cref="DrawableHitObject"/>s and provides interactive handles, context menus etc.
    /// </summary>
    public class SelectionBox : CompositeDrawable
    {
        public const float BORDER_RADIUS = 2;

        private readonly HashSet<HitObjectMask> selectedMasks = new HashSet<HitObjectMask>();

        private Drawable box;

        public SelectionBox()
        {
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = box = new Container
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

        public void AddMask(HitObjectMask mask)
        {
            mask.Selected += onSelected;
            mask.Deselected += onDeselected;
            mask.SingleSelectionRequested += onSingleSelectionRequested;
        }

        public void RemoveMask(HitObjectMask mask)
        {
            mask.Selected -= onSelected;
            mask.Deselected -= onDeselected;
            mask.SingleSelectionRequested -= onSingleSelectionRequested;
        }

        private void onSelected(HitObjectMask mask) => selectedMasks.Add(mask);

        private void onDeselected(HitObjectMask mask)
        {
            selectedMasks.Remove(mask);

            if (selectedMasks.Count == 0)
                FinishSelection();
        }

        private void onSingleSelectionRequested(HitObjectMask mask)
        {
            selectedMasks.Add(mask);
            FinishSelection();
        }

        // Only handle clicks on the selected masks
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => selectedMasks.Any(m => m.ReceiveMouseInputAt(screenSpacePos));

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnClick(InputState state)
        {
            if (state.Mouse.NativeState.PositionMouseDown == null)
                throw new InvalidOperationException("Click event received without a mouse down position.");

            // If the mouse has moved slightly, but hasn't been dragged, select the mask which would've handled the mouse down
            selectedMasks.First(m => m.ReceiveMouseInputAt(state.Mouse.NativeState.PositionMouseDown.Value)).TriggerOnMouseDown(state);
            return true;
        }

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            // Todo: Various forms of snapping

            foreach (var mask in selectedMasks)
            {
                switch (mask.HitObject)
                {
                    case IHasEditablePosition editablePosition:
                        editablePosition.OffsetPosition(state.Mouse.Delta);
                        break;
                }
            }

            return true;
        }

        protected override bool OnDragEnd(InputState state) => true;

        public void FinishSelection()
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

            // Todo: We might need to optimise this

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

            box.Size = bottomRight - topLeft;
            box.Position = topLeft;
        }
    }
}
