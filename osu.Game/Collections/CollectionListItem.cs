// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
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
    public class CollectionListItem : OsuRearrangeableListItem<BeatmapCollection>
    {
        private const float item_height = 35;

        public CollectionListItem(BeatmapCollection item)
            : base(item)
        {
            Padding = new MarginPadding { Right = 20 };
        }

        protected override Drawable CreateContent() => new ItemContent(Model);

        private class ItemContent : CircularContainer
        {
            private readonly BeatmapCollection collection;

            private ItemTextBox textBox;

            public ItemContent(BeatmapCollection collection)
            {
                this.collection = collection;

                RelativeSizeAxes = Axes.X;
                Height = item_height;
                Masking = true;
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
                        IsTextBoxHovered = v => textBox.ReceivePositionalInputAt(v)
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Right = item_height / 2 },
                        Children = new Drawable[]
                        {
                            textBox = new ItemTextBox
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = Vector2.One,
                                CornerRadius = item_height / 2,
                                Text = collection.Name
                            },
                        }
                    },
                };
            }
        }

        private class ItemTextBox : OsuTextBox
        {
            protected override float LeftRightPadding => item_height / 2;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundUnfocused = colours.GreySeafoamDarker.Darken(0.5f);
                BackgroundFocused = colours.GreySeafoam;
            }
        }

        private class DeleteButton : CompositeDrawable
        {
            public Func<Vector2, bool> IsTextBoxHovered;

            [Resolved(CanBeNull = true)]
            private DialogOverlay dialogOverlay { get; set; }

            private readonly BeatmapCollection collection;

            private Drawable background;

            public DeleteButton(BeatmapCollection collection)
            {
                this.collection = collection;
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fit;

                Alpha = 0.1f;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChildren = new[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Red
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        X = -6,
                        Size = new Vector2(10),
                        Icon = FontAwesome.Solid.Trash
                    }
                };
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => base.ReceivePositionalInputAt(screenSpacePos) && !IsTextBoxHovered(screenSpacePos);

            protected override bool OnHover(HoverEvent e)
            {
                this.FadeTo(1f, 100, Easing.Out);
                return false;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.FadeTo(0.1f, 100);
            }

            protected override bool OnClick(ClickEvent e)
            {
                background.FlashColour(Color4.White, 150);
                dialogOverlay?.Push(new DeleteCollectionDialog(collection));
                return true;
            }
        }
    }
}
