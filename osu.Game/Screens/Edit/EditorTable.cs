// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    public abstract partial class EditorTable : TableContainer
    {
        public event Action<Drawable>? OnRowSelected;

        private const float horizontal_inset = 20;

        protected const float ROW_HEIGHT = 25;

        public const int TEXT_SIZE = 14;

        protected readonly FillFlowContainer<RowBackground> BackgroundFlow;

        // We can avoid potentially thousands of objects being added to the input sub-tree since item selection is being handled by the BackgroundFlow
        // and no items in the underlying table are clickable.
        protected override bool ShouldBeConsideredForInput(Drawable child) => base.ShouldBeConsideredForInput(child) && child == BackgroundFlow;

        protected EditorTable()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Horizontal = horizontal_inset };
            RowSize = new Dimension(GridSizeMode.Absolute, ROW_HEIGHT);

            AddInternal(BackgroundFlow = new FillFlowContainer<RowBackground>
            {
                RelativeSizeAxes = Axes.Both,
                Depth = 1f,
                Padding = new MarginPadding { Horizontal = -horizontal_inset },
                Margin = new MarginPadding { Top = ROW_HEIGHT }
            });
        }

        protected int GetIndexForObject(object? item)
        {
            for (int i = 0; i < BackgroundFlow.Count; i++)
            {
                if (BackgroundFlow[i].Item == item)
                    return i;
            }

            return -1;
        }

        protected virtual bool SetSelectedRow(object? item)
        {
            bool foundSelection = false;

            foreach (var b in BackgroundFlow)
            {
                b.Selected = ReferenceEquals(b.Item, item);

                if (b.Selected)
                {
                    Debug.Assert(!foundSelection);
                    OnRowSelected?.Invoke(b);
                    foundSelection = true;
                }
            }

            return foundSelection;
        }

        protected object? GetObjectAtIndex(int index)
        {
            if (index < 0 || index > BackgroundFlow.Count - 1)
                return null;

            return BackgroundFlow[index].Item;
        }

        protected override Drawable CreateHeader(int index, TableColumn? column) => new HeaderText(column?.Header ?? default);

        private partial class HeaderText : OsuSpriteText
        {
            public HeaderText(LocalisableString text)
            {
                Text = text.ToUpper();
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold);
            }
        }

        public partial class RowBackground : OsuClickableContainer
        {
            public readonly object Item;

            private const int fade_duration = 100;

            private readonly Box hoveredBackground;

            public RowBackground(object item)
            {
                Item = item;

                RelativeSizeAxes = Axes.X;
                Height = 25;

                AlwaysPresent = true;

                CornerRadius = 3;
                Masking = true;

                Children = new Drawable[]
                {
                    hoveredBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    },
                };
            }

            private Color4 colourHover;
            private Color4 colourSelected;

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colours)
            {
                colourHover = colours.Background1;
                colourSelected = colours.Colour3;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
                FinishTransforms(true);
            }

            private bool selected;

            public bool Selected
            {
                get => selected;
                set
                {
                    if (value == selected)
                        return;

                    selected = value;
                    updateState();
                }
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

            private void updateState()
            {
                hoveredBackground.FadeColour(selected ? colourSelected : colourHover, 450, Easing.OutQuint);

                if (selected || IsHovered)
                    hoveredBackground.FadeIn(fade_duration, Easing.OutQuint);
                else
                    hoveredBackground.FadeOut(fade_duration, Easing.OutQuint);
            }
        }
    }
}
