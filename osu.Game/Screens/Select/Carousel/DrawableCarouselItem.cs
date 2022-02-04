// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Screens.Select.Carousel
{
    public abstract class DrawableCarouselItem : PoolableDrawable
    {
        public const float MAX_HEIGHT = 80;

        public override bool IsPresent => base.IsPresent || Item?.Visible == true;

        public readonly CarouselHeader Header;

        /// <summary>
        /// Optional content which sits below the header.
        /// </summary>
        protected readonly Container<Drawable> Content;

        protected readonly Container MovementContainer;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            Header.ReceivePositionalInputAt(screenSpacePos);

        private CarouselItem item;

        public CarouselItem Item
        {
            get => item;
            set
            {
                if (item == value)
                    return;

                if (item != null)
                {
                    item.Filtered.ValueChanged -= onStateChange;
                    item.State.ValueChanged -= onStateChange;

                    Header.State.UnbindFrom(item.State);

                    if (item is CarouselGroup group)
                    {
                        foreach (var c in group.Children)
                            c.Filtered.ValueChanged -= onChildrenChanged;
                    }
                }

                item = value;

                if (IsLoaded)
                    UpdateItem();
            }
        }

        protected DrawableCarouselItem(float headerHeight = MAX_HEIGHT)
        {
            RelativeSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                MovementContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        Header = new CarouselHeader
                        {
                            Height = headerHeight,
                        },
                        Content = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Y = headerHeight,
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            UpdateItem();
        }

        protected virtual void UpdateItem()
        {
            if (item == null)
                return;

            Scheduler.AddOnce(ApplyState);

            Item.Filtered.ValueChanged += onStateChange;
            Item.State.ValueChanged += onStateChange;

            Header.State.BindTo(Item.State);

            if (Item is CarouselGroup group)
            {
                foreach (var c in group.Children)
                    c.Filtered.ValueChanged += onChildrenChanged;
            }

            Scheduler.AddOnce(ApplyStateFromChildrenChange);
        }

        private void onStateChange(ValueChangedEvent<CarouselItemState> obj) => Scheduler.AddOnce(ApplyState);

        private void onStateChange(ValueChangedEvent<bool> _) => Scheduler.AddOnce(ApplyState);

        private void onChildrenChanged(ValueChangedEvent<bool> _) => Scheduler.AddOnce(ApplyStateFromChildrenChange);

        private CarouselItemState? lastAppliedState;

        /// <summary>
        /// If <see cref="Item"/> is a <see cref="CarouselGroup"/>, will fire whenever a child's <see cref="CarouselItem.Filtered"/> state changes.
        /// </summary>
        protected virtual void ApplyStateFromChildrenChange()
        {
        }

        /// <summary>
        /// Fired whenever the <see cref="Item"/>'s <see cref="CarouselItem.Filtered"/> or <see cref="CarouselItem.State"/> changes.
        /// </summary>
        protected virtual void ApplyState()
        {
            Debug.Assert(Item != null);

            if (lastAppliedState != Item.State.Value)
            {
                lastAppliedState = Item.State.Value;

                // Use the fact that we know the precise height of the item from the model to avoid the need for AutoSize overhead.
                // Additionally, AutoSize doesn't work well due to content starting off-screen and being masked away.
                Height = Item.TotalHeight;

                switch (lastAppliedState)
                {
                    case CarouselItemState.NotSelected:
                        Deselected();
                        break;

                    case CarouselItemState.Selected:
                        Selected();
                        break;
                }
            }

            if (!Item.Visible)
                Hide();
            else
                Show();
        }

        private bool isVisible = true;

        public override void Show()
        {
            if (isVisible)
                return;

            isVisible = true;
            this.FadeIn(250);
        }

        public override void Hide()
        {
            if (!isVisible)
                return;

            isVisible = false;
            this.FadeOut(300, Easing.OutQuint);
        }

        protected virtual void Selected()
        {
            Debug.Assert(Item != null);
        }

        protected virtual void Deselected()
        {
        }

        protected override bool OnClick(ClickEvent e)
        {
            Item.State.Value = CarouselItemState.Selected;
            return true;
        }
    }
}
