// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    public partial class EditorTableBackground : Container<EditorTableBackgroundRow>
    {
        public Action<int>? Selected;

        public const float ROW_HEIGHT = 25;

        private int rowCount;

        public int RowCount
        {
            get => rowCount;
            set
            {
                rowCount = value;
                Height = rowCount * ROW_HEIGHT;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!IsHovered)
            {
                for (int i = Children.Count - 1; i >= 0; i--)
                {
                    var child = Children[i];
                    if (child.State != RowState.Selected)
                        child.State = RowState.None;
                }

                return;
            }

            updateWhenHovered();
        }

        protected override bool OnHover(HoverEvent e) => true;

        private void updateWhenHovered()
        {
            float y = ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position).Y;

            EditorTableBackgroundRow? existingAtY = null;

            foreach (var child in this)
            {
                if (child.Y <= y && child.Y + child.Height > y)
                {
                    existingAtY = child;
                }
                else
                {
                    if (child.State != RowState.Selected)
                        child.State = RowState.None;
                }
            }

            if (existingAtY != null)
            {
                if (existingAtY.State != RowState.Selected)
                    existingAtY.State = RowState.Hovered;
            }
            else
            {
                int newItemIndex = getItemIndexAt(y);

                if (newItemIndex != -1)
                {
                    Add(new EditorTableBackgroundRow(newItemIndex)
                    {
                        Y = newItemIndex * ROW_HEIGHT
                    });
                }
            }
        }

        public EditorTableBackgroundRow Select(int itemIndex)
        {
            EditorTableBackgroundRow? existingIndexRepresentation = null;

            foreach (var child in this)
            {
                if (child.State == RowState.Selected)
                    child.State = RowState.None;

                if (child.Index == itemIndex)
                {
                    existingIndexRepresentation = child;
                    break;
                }
            }

            if (existingIndexRepresentation != null)
            {
                existingIndexRepresentation.State = RowState.Selected;
                return existingIndexRepresentation;
            }

            var toAdd = new EditorTableBackgroundRow(itemIndex)
            {
                Y = itemIndex * ROW_HEIGHT,
                State = RowState.Selected
            };

            Add(toAdd);
            return toAdd;
        }

        protected override bool OnClick(ClickEvent e)
        {
            base.OnClick(e);
            float y = ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position).Y;
            int index = getItemIndexAt(y);

            if (index != -1)
            {
                Select(index);
                Selected?.Invoke(index);
            }

            return true;
        }

        private int getItemIndexAt(float y)
        {
            int index = (int)(y / ROW_HEIGHT);
            if (index >= 0 && index < RowCount)
                return index;

            return -1;
        }
    }

    public partial class EditorTableBackgroundRow : CompositeDrawable
    {
        private const int fade_duration = 100;
        private const int colour_duration = 450;

        private RowState state = RowState.Hovered;

        public RowState State
        {
            get => state;
            set
            {
                if (state == value)
                    return;

                state = value;

                if (!IsLoaded)
                    return;

                updateState(state);
            }
        }

        public override bool RemoveWhenNotAlive => true;

        protected override bool ShouldBeAlive => base.ShouldBeAlive && Alpha > 0;

        public int Index { get; }

        public EditorTableBackgroundRow(int index)
        {
            Index = index;
        }

        private Color4 colourHover;
        private Color4 colourSelected;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            RelativeSizeAxes = Axes.X;
            Height = EditorTableBackground.ROW_HEIGHT;
            Masking = true;
            CornerRadius = 3;
            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both
            };

            colourHover = colours.Background1;
            colourSelected = colours.Colour3;

            Colour = state == RowState.Hovered ? colourHover : colourSelected;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState(state);
        }

        private void updateState(RowState newState)
        {
            switch (newState)
            {
                case RowState.None:
                    this.FadeOut(fade_duration, Easing.OutQuint);
                    this.FadeColour(colourHover, colour_duration, Easing.OutQuint);
                    break;

                case RowState.Hovered:
                    this.FadeIn(fade_duration, Easing.OutQuint);
                    this.FadeColour(colourHover, colour_duration, Easing.OutQuint);
                    break;

                case RowState.Selected:
                    this.FadeIn(fade_duration, Easing.OutQuint);
                    this.FadeColour(colourSelected, colour_duration, Easing.OutQuint);
                    break;
            }
        }
    }

    public enum RowState
    {
        None,
        Hovered,
        Selected
    }
}
