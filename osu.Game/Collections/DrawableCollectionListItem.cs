// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Collections
{
    /// <summary>
    /// Visualises a <see cref="BeatmapCollection"/> inside a <see cref="DrawableCollectionList"/>.
    /// </summary>
    public class DrawableCollectionListItem : OsuRearrangeableListItem<BeatmapCollection>
    {
        private const float item_height = 35;
        private const float button_width = item_height * 0.75f;

        /// <summary>
        /// Whether the <see cref="BeatmapCollection"/> currently exists inside the <see cref="CollectionManager"/>.
        /// </summary>
        public IBindable<bool> IsCreated => isCreated;

        private readonly Bindable<bool> isCreated = new Bindable<bool>();

        /// <summary>
        /// Creates a new <see cref="DrawableCollectionListItem"/>.
        /// </summary>
        /// <param name="item">The <see cref="BeatmapCollection"/>.</param>
        /// <param name="isCreated">Whether <paramref name="item"/> currently exists inside the <see cref="CollectionManager"/>.</param>
        public DrawableCollectionListItem(BeatmapCollection item, bool isCreated)
            : base(item)
        {
            this.isCreated.Value = isCreated;

            ShowDragHandle.BindTo(this.isCreated);
        }

        protected override Drawable CreateContent() => new ItemContent(Model)
        {
            IsCreated = { BindTarget = isCreated }
        };

        /// <summary>
        /// The main content of the <see cref="DrawableCollectionListItem"/>.
        /// </summary>
        private class ItemContent : CircularContainer
        {
            public readonly Bindable<bool> IsCreated = new Bindable<bool>();

            private readonly IBindable<string> collectionName;
            private readonly BeatmapCollection collection;

            [Resolved(CanBeNull = true)]
            private CollectionManager collectionManager { get; set; }

            private Container textBoxPaddingContainer;
            private ItemTextBox textBox;

            public ItemContent(BeatmapCollection collection)
            {
                this.collection = collection;

                RelativeSizeAxes = Axes.X;
                Height = item_height;
                Masking = true;

                collectionName = collection.Name.GetBoundCopy();
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Children = new Drawable[]
                {
                    new DeleteButton(collection)
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        IsCreated = { BindTarget = IsCreated },
                        IsTextBoxHovered = v => textBox.ReceivePositionalInputAt(v)
                    },
                    textBoxPaddingContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Right = button_width },
                        Children = new Drawable[]
                        {
                            textBox = new ItemTextBox
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = Vector2.One,
                                CornerRadius = item_height / 2,
                                PlaceholderText = IsCreated.Value ? string.Empty : "Create a new collection"
                            },
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                // Bind late, as the collection name may change externally while still loading.
                textBox.Current = collection.Name;

                collectionName.BindValueChanged(_ => createNewCollection(), true);
                IsCreated.BindValueChanged(created => textBoxPaddingContainer.Padding = new MarginPadding { Right = created.NewValue ? button_width : 0 }, true);
            }

            private void createNewCollection()
            {
                if (IsCreated.Value)
                    return;

                if (string.IsNullOrEmpty(collectionName.Value))
                    return;

                // Add the new collection and disable our placeholder. If all text is removed, the placeholder should not show back again.
                collectionManager?.Collections.Add(collection);
                textBox.PlaceholderText = string.Empty;

                // When this item changes from placeholder to non-placeholder (via changing containers), its textbox will lose focus, so it needs to be re-focused.
                Schedule(() => GetContainingInputManager().ChangeFocus(textBox));

                IsCreated.Value = true;
            }
        }

        private class ItemTextBox : OsuTextBox
        {
            protected override float LeftRightPadding => item_height / 2;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundUnfocused = colours.GreySeaFoamDarker.Darken(0.5f);
                BackgroundFocused = colours.GreySeaFoam;
            }
        }

        public class DeleteButton : CompositeDrawable
        {
            public readonly IBindable<bool> IsCreated = new Bindable<bool>();

            public Func<Vector2, bool> IsTextBoxHovered;

            [Resolved(CanBeNull = true)]
            private DialogOverlay dialogOverlay { get; set; }

            [Resolved(CanBeNull = true)]
            private CollectionManager collectionManager { get; set; }

            private readonly BeatmapCollection collection;

            private Drawable fadeContainer;
            private Drawable background;

            public DeleteButton(BeatmapCollection collection)
            {
                this.collection = collection;
                RelativeSizeAxes = Axes.Y;

                Width = button_width + item_height / 2; // add corner radius to cover with fill
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChild = fadeContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.1f,
                    Children = new[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Red
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.Centre,
                            X = -button_width * 0.6f,
                            Size = new Vector2(10),
                            Icon = FontAwesome.Solid.Trash
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                IsCreated.BindValueChanged(created => Alpha = created.NewValue ? 1 : 0, true);
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => base.ReceivePositionalInputAt(screenSpacePos) && !IsTextBoxHovered(screenSpacePos);

            protected override bool OnHover(HoverEvent e)
            {
                fadeContainer.FadeTo(1f, 100, Easing.Out);
                return false;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                fadeContainer.FadeTo(0.1f, 100);
            }

            protected override bool OnClick(ClickEvent e)
            {
                background.FlashColour(Color4.White, 150);

                if (collection.Beatmaps.Count == 0)
                    deleteCollection();
                else
                    dialogOverlay?.Push(new DeleteCollectionDialog(collection, deleteCollection));

                return true;
            }

            private void deleteCollection() => collectionManager?.Collections.Remove(collection);
        }
    }
}
