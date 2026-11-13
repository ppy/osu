// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        private bool alertTitleFound;

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

            if (customContainer.FirstOrDefault() is ParagraphBlock paragraphBlock &&
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
            border.Colour = getAlertColour();
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

        /// <remarks>
        /// <para>
        /// This ugly construction is done to ensure that only the first emphasis block
        /// of this entire alert gets its special colour applied to it.
        /// The field cannot live inside <see cref="StyledAlertTextFlowContainer"/>
        /// as one <see cref="StyledAlertTextFlowContainer"/> exists for every paragraph of the alert content,
        /// so the first emphasis <i>of every paragraph</i> would get the special colour.
        /// </para>
        /// <para>
        /// Web uses a similarly repulsive CSS selector construction of
        /// <c>&amp;:first-child > strong:first-of-type</c>
        /// (https://github.com/ppy/osu-web/blob/ad92032fd45eaf9bef748fd37ebde2eeb0a17370/resources/css/bem/osu-md.less#L398-L401)
        /// and not much more can be done about that.
        /// </para>
        /// </remarks>
        private bool foundAlertTitle()
        {
            if (!alertTitleFound)
            {
                alertTitleFound = true;
                return false;
            }

            return true;
        }

        MarkdownTextFlowContainer IMarkdownTextFlowComponent.CreateTextFlow() => new StyledAlertTextFlowContainer
        {
            AlertColour = getAlertColour(),
            AlertIcon = getAlertIcon(),
            BlockTitle = blockTitle,
            FoundAlertTitle = foundAlertTitle
        };

        private partial class StyledAlertTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            public Colour4 AlertColour { get; set; }
            public IconUsage? AlertIcon { get; set; }
            public bool BlockTitle { get; set; }
            public Func<bool>? FoundAlertTitle { get; set; }

            protected override void AddEmphasis(string text, bool hasBold, bool hasItalic)
            {
                if (hasBold && FoundAlertTitle?.Invoke() == false)
                {
                    int marginBottom = BlockTitle ? 5 : 0;

                    if (AlertIcon is IconUsage icon)
                    {
                        AddDrawable(new SpriteIcon
                        {
                            Icon = icon,
                            Size = new Vector2(14),
                            Margin = new MarginPadding { Right = 5, Bottom = marginBottom },
                            Colour = AlertColour,
                        });
                    }

                    AddText(text, t =>
                    {
                        base.ApplyEmphasisedCreationParameters(t, hasBold, hasItalic);
                        t.Colour = AlertColour;
                        t.Margin = new MarginPadding { Bottom = marginBottom };
                    });
                }
                else
                    base.AddEmphasis(text, hasBold, hasItalic);
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
