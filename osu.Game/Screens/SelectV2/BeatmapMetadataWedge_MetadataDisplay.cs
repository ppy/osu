// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
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
        private partial class MetadataDisplay : FillFlowContainer
        {
            private (LocalisableString value, Action? linkAction)? data;

            public (LocalisableString value, Action? linkAction)? Data
            {
                get => data;
                set
                {
                    data = value;

                    valueText.Clear();

                    if (value.HasValue)
                    {
                        string valueString = value.Value.value.ToString();
                        // todo: this logic is not perfect but we don't have a way to truncate text in TextFlowContainer yet.
                        string truncatedValueString = valueString.Truncate(24);

                        if (value.Value.linkAction != null)
                            valueText.AddLink(truncatedValueString, value.Value.linkAction, valueString != truncatedValueString ? valueString : null);
                        else
                            valueText.AddText(truncatedValueString);
                    }
                    else
                    {
                        valueText.AddArbitraryDrawable(new LoadingSpinner
                        {
                            Size = new Vector2(10),
                            Margin = new MarginPadding { Top = 3f },
                            State = { Value = Visibility.Visible },
                        });
                    }
                }
            }

            public DateTimeOffset? Date
            {
                set
                {
                    valueText.Clear();

                    if (value != null)
                        valueText.AddArbitraryDrawable(new DrawableDate(value.Value, textSize: OsuFont.Style.Caption1.Size, italic: false));
                    else
                        valueText.AddText("-");
                }
            }

            public (string[] tags, Action<string> linkAction) Tags
            {
                set
                {
                    valueText.Clear();
                    int total = 0;

                    foreach (string tag in value.tags)
                    {
                        total += tag.Length + 1;

                        // todo: this logic is not perfect but we don't have a way to truncate text in TextFlowContainer yet.
                        if (total > 80)
                        {
                            valueText.AddArbitraryDrawable(new TagsOverflowButton(value.tags));
                            break;
                        }

                        valueText.AddLink(tag, () => value.linkAction(tag));
                        valueText.AddText(" ");
                    }
                }
            }

            private readonly OsuSpriteText labelText;
            private readonly LinkFlowContainer valueText;

            public MetadataDisplay(LocalisableString label)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    labelText = new OsuSpriteText
                    {
                        Text = label,
                        Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                    },
                    valueText = new LinkFlowContainer(t => t.Font = OsuFont.Style.Caption1)
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = OsuFont.Style.Caption1.Size,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                labelText.Colour = colourProvider.Content1;
                valueText.Colour = colourProvider.Content2;
            }

            private partial class TagsOverflowButton : CompositeDrawable, IHasPopover, IHasLineBaseHeight
            {
                private readonly string[] tags;

                private Box box = null!;
                private OsuSpriteText text = null!;

                [Resolved]
                private OverlayColourProvider colourProvider { get; set; } = null!;

                [Resolved]
                private SongSelect? songSelect { get; set; }

                public float LineBaseHeight => text.LineBaseHeight;

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
                        }
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

                public Popover GetPopover() => new TagsOverflowPopover(tags, songSelect);
            }

            public partial class TagsOverflowPopover : OsuPopover
            {
                private readonly string[] tags;
                private readonly SongSelect? songSelect;

                public TagsOverflowPopover(string[] tags, SongSelect? songSelect)
                {
                    this.tags = tags;
                    this.songSelect = songSelect;
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
                        textFlow.AddLink(tag, () => songSelect?.Search(tag));
                        textFlow.AddText(" ");
                    }
                }
            }
        }
    }
}
