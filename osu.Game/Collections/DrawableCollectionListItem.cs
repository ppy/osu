// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Collections
{
    /// <summary>
    /// Visualises a <see cref="BeatmapCollection"/> inside a <see cref="DrawableCollectionList"/>.
    /// </summary>
    public partial class DrawableCollectionListItem : OsuRearrangeableListItem<Live<BeatmapCollection>>
    {
        private const float item_height = 45;
        private const float button_width = item_height * 0.75f;

        /// <summary>
        /// Creates a new <see cref="DrawableCollectionListItem"/>.
        /// </summary>
        /// <param name="item">The <see cref="BeatmapCollection"/>.</param>
        /// <param name="isCreated">Whether <paramref name="item"/> currently exists inside realm.</param>
        public DrawableCollectionListItem(Live<BeatmapCollection> item, bool isCreated)
            : base(item)
        {
            // For now we don't support rearranging and always use alphabetical sort.
            // Change this to:
            //
            // ShowDragHandle.Value = item.IsManaged;
            //
            // if we want to support user sorting (but changes will need to be made to realm to persist).
            ShowDragHandle.Value = false;
        }

        protected override Drawable CreateContent() => new ItemContent(Model);

        /// <summary>
        /// The main content of the <see cref="DrawableCollectionListItem"/>.
        /// </summary>
        private partial class ItemContent : CircularContainer
        {
            private readonly Live<BeatmapCollection> collection;

            private ItemTextBox textBox = null!;

            [Resolved]
            private RealmAccess realm { get; set; } = null!;

            public ItemContent(Live<BeatmapCollection> collection)
            {
                this.collection = collection;

                RelativeSizeAxes = Axes.X;
                Height = item_height;
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new[]
                {
                    collection.IsManaged
                        ? new DeleteButton(collection)
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            IsTextBoxHovered = v => textBox.ReceivePositionalInputAt(v)
                        }
                        : Empty(),
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Right = collection.IsManaged ? button_width : 0 },
                        Children = new Drawable[]
                        {
                            textBox = new ItemTextBox(collection)
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = item_height,
                                CommitOnFocusLost = true,
                            },
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                // Bind late, as the collection name may change externally while still loading.
                textBox.Current.Value = collection.PerformRead(c => c.IsValid ? c.Name : string.Empty);
                textBox.OnCommit += onCommit;
            }

            private void onCommit(TextBox sender, bool newText)
            {
                if (collection.IsManaged)
                    collection.PerformWrite(c => c.Name = textBox.Current.Value);
                else if (!string.IsNullOrEmpty(textBox.Current.Value))
                    realm.Write(r => r.Add(new BeatmapCollection(textBox.Current.Value)));

                textBox.Text = string.Empty;
            }
        }

        private partial class ItemTextBox : OsuTextBox
        {
            protected override float LeftRightPadding => item_height / 2;

            private const float count_text_size = 12;

            [Resolved]
            private RealmAccess realm { get; set; } = null!;

            private readonly Live<BeatmapCollection> collection;

            private OsuSpriteText countText = null!;

            private IDisposable? itemCountSubscription;

            public ItemTextBox(Live<BeatmapCollection> collection)
            {
                this.collection = collection;

                CornerRadius = item_height / 2;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundUnfocused = colours.GreySeaFoamDarker.Darken(0.5f);
                BackgroundFocused = colours.GreySeaFoam;

                if (collection.IsManaged)
                {
                    TextContainer.Height *= (Height - count_text_size) / Height;
                    TextContainer.Margin = new MarginPadding { Bottom = count_text_size };

                    TextContainer.Add(countText = new OsuSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.TopLeft,
                        Depth = float.MinValue,
                        Font = OsuFont.Default.With(size: count_text_size, weight: FontWeight.SemiBold),
                        Margin = new MarginPadding { Top = 2, Left = 2 },
                        Colour = colours.Yellow
                    });

                    itemCountSubscription = realm.SubscribeToPropertyChanged(r => r.Find<BeatmapCollection>(collection.ID), c => c.BeatmapMD5Hashes, _ =>
                        Scheduler.AddOnce(() =>
                        {
                            int count = collection.PerformRead(c => c.BeatmapMD5Hashes.Count);

                            countText.Text = count == 1
                                // Intentionally not localised until we have proper support for this (see https://github.com/ppy/osu-framework/pull/4918
                                // but also in this case we want support for formatting a number within a string).
                                ? $"{count:#,0} beatmap"
                                : $"{count:#,0} beatmaps";
                        }));
                }
                else
                {
                    PlaceholderText = "Create a new collection";
                }
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                itemCountSubscription?.Dispose();
            }
        }

        public partial class DeleteButton : CompositeDrawable
        {
            public Func<Vector2, bool> IsTextBoxHovered = null!;

            [Resolved]
            private IDialogOverlay? dialogOverlay { get; set; }

            private readonly Live<BeatmapCollection> collection;

            private Drawable fadeContainer = null!;
            private Drawable background = null!;

            public DeleteButton(Live<BeatmapCollection> collection)
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

                if (collection.PerformRead(c => c.BeatmapMD5Hashes.Count) == 0)
                    deleteCollection();
                else
                    dialogOverlay?.Push(new DeleteCollectionDialog(collection, deleteCollection));

                return true;
            }

            private void deleteCollection() => collection.PerformWrite(c => c.Realm!.Remove(c));
        }
    }
}
