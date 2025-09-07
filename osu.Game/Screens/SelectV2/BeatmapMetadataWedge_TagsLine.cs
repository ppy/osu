// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMetadataWedge
    {
        private partial class TagsLine : FillFlowContainer
        {
            private readonly LayoutValue drawSizeLayout = new LayoutValue(Invalidation.DrawSize);

            private string[] tags = Array.Empty<string>();

            private TagsOverflowButton? overflowButton;

            public string[] Tags
            {
                get => tags;
                set
                {
                    tags = value;
                    updateTags();
                }
            }

            public Action<string>? PerformSearch { get; set; }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public TagsLine()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(4, 0);

                AddLayout(drawSizeLayout);
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                if (!drawSizeLayout.IsValid)
                {
                    updateLayout();
                    drawSizeLayout.Validate();
                }
            }

            private void updateLayout()
            {
                if (tags.Length == 0)
                    return;

                Debug.Assert(overflowButton != null);

                float limit = DrawWidth - overflowButton.DrawWidth - 5;
                bool showOverflow = false;

                foreach (var text in Children)
                {
                    if (text.X + text.DrawWidth < limit)
                        text.Show();
                    else
                    {
                        showOverflow = true;
                        text.AlwaysPresent = false;
                        text.Hide();
                    }
                }

                if (showOverflow)
                    overflowButton.Show();
                else
                    overflowButton.Hide();
            }

            private void updateTags()
            {
                ChildrenEnumerable = tags.Select(t => new OsuHoverContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Action = () => PerformSearch?.Invoke(t),
                    IdleColour = colourProvider.Light2,
                    AlwaysPresent = true,
                    Alpha = 0f,
                    Child = new OsuSpriteText
                    {
                        Text = t,
                        Font = OsuFont.Style.Caption1,
                    },
                });

                Add(overflowButton = new TagsOverflowButton(tags)
                {
                    Alpha = 0f,
                    PerformSearch = s => PerformSearch?.Invoke(s),
                });

                drawSizeLayout.Invalidate();
            }

            private partial class TagsOverflowButton : CompositeDrawable, IHasPopover, IHasLineBaseHeight
            {
                private readonly string[] tags;

                private Box box = null!;
                private OsuSpriteText text = null!;

                [Resolved]
                private OverlayColourProvider colourProvider { get; set; } = null!;

                public float LineBaseHeight => text.LineBaseHeight;

                public Action<string>? PerformSearch { get; init; }

                public TagsOverflowButton(string[] tags)
                {
                    this.tags = tags;
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    Size = new Vector2(OsuFont.Style.Caption1.Size);
                    CornerRadius = 1.5f;
                    Masking = true;

                    InternalChildren = new Drawable[]
                    {
                        box = new Box
                        {
                            Colour = colourProvider.Light1,
                            RelativeSizeAxes = Axes.Both,
                        },
                        text = new OsuSpriteText
                        {
                            Y = -2,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "...",
                            Colour = colourProvider.Background4,
                            Font = OsuFont.Style.Caption1.With(weight: FontWeight.Bold),
                        },
                        new HoverClickSounds(),
                    };
                }

                protected override bool OnHover(HoverEvent e)
                {
                    box.FadeColour(colourProvider.Content2, 300, Easing.OutQuint);
                    return base.OnHover(e);
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    box.FadeColour(colourProvider.Light1, 300, Easing.OutQuint);
                    base.OnHoverLost(e);
                }

                protected override bool OnClick(ClickEvent e)
                {
                    box.FlashColour(colourProvider.Content1, 300, Easing.OutQuint);
                    this.ShowPopover();
                    return true;
                }

                public Popover GetPopover() => new TagsOverflowPopover(tags, PerformSearch);
            }

            public partial class TagsOverflowPopover : OsuPopover
            {
                private readonly string[] tags;
                private readonly Action<string>? performSearch;

                public TagsOverflowPopover(string[] tags, Action<string>? performSearchAction)
                {
                    this.tags = tags;
                    performSearch = performSearchAction;
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    LinkFlowContainer textFlow;

                    Child = textFlow = new LinkFlowContainer(t => t.Font = OsuFont.Style.Caption1)
                    {
                        Width = 200,
                        AutoSizeAxes = Axes.Y,
                    };

                    foreach (string tag in tags)
                    {
                        textFlow.AddLink(tag, () => performSearch?.Invoke(tag));
                        textFlow.AddText(" ");
                    }
                }
            }
        }
    }
}
