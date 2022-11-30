// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Containers
{
    public abstract partial class OsuRearrangeableListItem<TModel> : RearrangeableListItem<TModel>
    {
        public const float FADE_DURATION = 100;

        /// <summary>
        /// Whether any item is currently being dragged. Used to hide other items' drag handles.
        /// </summary>
        public readonly BindableBool DragActive = new BindableBool();

        private Color4 handleColour = Color4.White;

        /// <summary>
        /// The colour of the drag handle.
        /// </summary>
        protected Color4 HandleColour
        {
            get => handleColour;
            set
            {
                if (handleColour == value)
                    return;

                handleColour = value;

                if (handle != null)
                    handle.Colour = value;
            }
        }

        /// <summary>
        /// Whether the drag handle should be shown.
        /// </summary>
        protected readonly Bindable<bool> ShowDragHandle = new Bindable<bool>(true);

        private Container handleContainer;
        private PlaylistItemHandle handle;

        protected OsuRearrangeableListItem(TModel item)
            : base(item)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Content = new[]
                    {
                        new[]
                        {
                            handleContainer = new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Horizontal = 5 },
                                Child = handle = new PlaylistItemHandle
                                {
                                    Size = new Vector2(12),
                                    Colour = HandleColour,
                                    AlwaysPresent = true,
                                    Alpha = 0
                                }
                            },
                            CreateContent()
                        }
                    },
                    ColumnDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) }
                },
                new HoverClickSounds()
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ShowDragHandle.BindValueChanged(show => handleContainer.Alpha = show.NewValue ? 1 : 0, true);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (!base.OnDragStart(e))
                return false;

            DragActive.Value = true;
            return true;
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            DragActive.Value = false;
            base.OnDragEnd(e);
        }

        protected override bool IsDraggableAt(Vector2 screenSpacePos) => handle.HandlingDrag;

        protected override bool OnHover(HoverEvent e)
        {
            handle.UpdateHoverState(IsDragged || !DragActive.Value);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e) => handle.UpdateHoverState(false);

        protected abstract Drawable CreateContent();

        public partial class PlaylistItemHandle : SpriteIcon
        {
            public bool HandlingDrag { get; private set; }
            private bool isHovering;

            public PlaylistItemHandle()
            {
                Icon = FontAwesome.Solid.Bars;
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                base.OnMouseDown(e);

                HandlingDrag = true;
                UpdateHoverState(isHovering);

                return false;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                base.OnMouseUp(e);

                HandlingDrag = false;
                UpdateHoverState(isHovering);
            }

            public void UpdateHoverState(bool hovering)
            {
                isHovering = hovering;

                if (isHovering || HandlingDrag)
                    this.FadeIn(FADE_DURATION);
                else
                    this.FadeOut(FADE_DURATION);
            }
        }
    }
}
