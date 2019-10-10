// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Comments
{
    public class CommentsHeader : CompositeDrawable
    {
        private const int height = 40;
        private const int spacing = 10;
        private const int padding = 50;
        private const int text_size = 14;

        public readonly Bindable<SortCommentsBy> Sort = new Bindable<SortCommentsBy>();
        public readonly BindableBool ShowDeleted = new BindableBool();

        private readonly Box background;

        public CommentsHeader()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;
            AddRangeInternal(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = padding },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(spacing, 0),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.GetFont(size: text_size),
                                    Text = @"Sort by"
                                },
                                new SortSelector
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Current = Sort
                                }
                            }
                        },
                        new ShowDeletedButton
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Checked = { BindTarget = ShowDeleted }
                        }
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray3;
        }

        private class ShowDeletedButton : HeaderButton
        {
            private const int spacing = 5;

            public readonly BindableBool Checked = new BindableBool();

            private readonly SpriteIcon checkboxIcon;

            public ShowDeletedButton()
            {
                Add(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(spacing, 0),
                    Children = new Drawable[]
                    {
                        checkboxIcon = new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(10),
                        },
                        new SpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(size: text_size),
                            Text = @"Show deleted"
                        }
                    },
                });
            }

            protected override void LoadComplete()
            {
                Checked.BindValueChanged(onCheckedChanged, true);
                base.LoadComplete();
            }

            private void onCheckedChanged(ValueChangedEvent<bool> isChecked)
            {
                checkboxIcon.Icon = isChecked.NewValue ? FontAwesome.Solid.CheckSquare : FontAwesome.Regular.Square;
            }

            protected override bool OnClick(ClickEvent e)
            {
                Checked.Value = !Checked.Value;
                return base.OnClick(e);
            }
        }
    }
}
