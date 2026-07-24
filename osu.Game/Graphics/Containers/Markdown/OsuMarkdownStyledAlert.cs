// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Markdig.Extensions.CustomContainers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownStyledAlert : CompositeDrawable, IMarkdownTextFlowComponent
    {
        private readonly AlertType alertType;
        private readonly Box border;
        private readonly bool blockTitle;
        private bool firstBold = true;

        private const int border_width = 4;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public FillFlowContainer Content { get; }

        public OsuMarkdownStyledAlert(CustomContainer customContainer)
        {
            if (Enum.TryParse(customContainer.Info.Split("-")[1], true, out AlertType alertTypeParsed))
                alertType = alertTypeParsed;
            else
                alertType = AlertType.None;

            if (customContainer.First() is ParagraphBlock paragraphBlock &&
                paragraphBlock.Inline.FirstChild is EmphasisInline emphasisInline &&
                emphasisInline.NextSibling is LineBreakInline)
            {
                blockTitle = true;
            }

            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            InternalChildren = new Drawable[]
            {
                border = new Box
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = border_width,
                },
                Content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(20),
                    Margin = new MarginPadding { Left = border_width },
                    Padding = new MarginPadding { Vertical = 10, Horizontal = 15 }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            border.Colour = getAlertColour().Opacity(0.75f);
        }

        private Colour4 getAlertColour()
        {
            switch (alertType)
            {
                case AlertType.Note:
                    return colours.Blue2.Opacity(0.75f);

                case AlertType.Tip:
                    return colours.Lime2.Opacity(0.75f);

                case AlertType.Notice:
                    return colours.Pink2.Opacity(0.75f);

                case AlertType.Warning:
                    return colours.Red2.Opacity(0.75f);

                case AlertType.Caution:
                    return colours.Orange2.Opacity(0.75f);

                case AlertType.None:
                default:
                    return Colour4.White;
            }
        }

        private IconUsage? getAlertIcon()
        {
            switch (alertType)
            {
                case AlertType.Note:
                    return FontAwesome.Solid.InfoCircle;

                case AlertType.Tip:
                    return FontAwesome.Solid.Lightbulb;

                case AlertType.Notice:
                    return FontAwesome.Solid.Bullhorn;

                case AlertType.Warning:
                    return FontAwesome.Solid.Exclamation;

                case AlertType.Caution:
                    return FontAwesome.Solid.ExclamationTriangle;

                case AlertType.None:
                default:
                    return null;
            }
        }

        private bool checkFirstBold()
        {
            if (firstBold)
            {
                firstBold = false;
                return true;
            }

            return false;
        }

        MarkdownTextFlowContainer IMarkdownTextFlowComponent.CreateTextFlow() => new StyledAlertTextFlowContainer
        {
            AlertColour = getAlertColour(),
            AlertIcon = getAlertIcon(),
            BlockTitle = blockTitle,
            CheckFirstBold = checkFirstBold
        };

        private partial class StyledAlertTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            public Colour4 AlertColour { get; set; }
            public IconUsage? AlertIcon { get; set; }
            public bool BlockTitle { get; set; }
            public Func<bool>? CheckFirstBold { get; set; }

            protected override void AddEmphasis(string text, List<string> emphases)
            {
                bool hasBold = emphases.Any(s => s == "**" || s == "__");
                bool hasItalic = emphases.Any(s => s == "*" || s == "_");

                if (hasBold && CheckFirstBold?.Invoke() == true)
                {
                    if (AlertIcon is IconUsage icon)
                    {
                        AddDrawable(new SpriteIcon
                        {
                            Icon = icon,
                            Size = new Vector2(14),
                            Margin = new MarginPadding { Right = 5, Bottom = BlockTitle ? 5 : 0 },
                            Colour = AlertColour,
                        });
                    }

                    AddText(text, t =>
                    {
                        base.ApplyEmphasisedCreationParameters(t, hasBold, hasItalic);
                        t.Colour = AlertColour;
                        t.Margin = new MarginPadding { Bottom = BlockTitle ? 5 : 0 };
                    });
                }
                else
                    base.AddEmphasis(text, emphases);
            }
        }

        private enum AlertType
        {
            Note,
            Tip,
            Notice,
            Warning,
            Caution,
            None,
        }
    }
}
