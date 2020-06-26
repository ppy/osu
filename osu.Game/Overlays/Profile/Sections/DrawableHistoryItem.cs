// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class DrawableHistoryItem<T> : CompositeDrawable
    {
        protected const int FONT_SIZE = 14;

        [Resolved]
        protected IAPIProvider API { get; private set; }

        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; }

        protected readonly T Item;

        protected LinkFlowContainer Content { get; private set; }

        protected DrawableHistoryItem(T item)
        {
            Item = item;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AddInternal(new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, size: LeftContentSize()),
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new[]
                    {
                        CreateLeftContent(),
                        Content = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: FONT_SIZE))
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        },
                        new DrawableDate(GetDate())
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Colour = ColourProvider.Foreground1,
                            Font = OsuFont.GetFont(size: FONT_SIZE),
                        }
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CreateMessage();
        }

        protected abstract DateTimeOffset GetDate();

        protected abstract void CreateMessage();

        protected virtual float LeftContentSize() => 0;

        protected virtual Drawable CreateLeftContent() => Empty();

        private void addText(string text, Color4 colour) => Content.AddText(text, t =>
        {
            t.Font = OsuFont.GetFont(size: FONT_SIZE, weight: FontWeight.SemiBold);
            t.Colour = colour;
        });

        protected void AddText(string text) => addText(text, Color4.White);

        protected void AddColoredText(string text, Color4 colour) => addText(text, colour);

        protected void AddLink(string title, LinkAction action, string url, FontWeight fontWeight = FontWeight.Regular)
            => Content.AddLink(title, action, getLinkArgument(url), creationParameters: t => t.Font = getLinkFont(fontWeight));

        protected void AddUserLink(string username, string url)
            => AddLink(username, LinkAction.OpenUserProfile, url, FontWeight.Bold);

        private string getLinkArgument(string url) => MessageFormatter.GetLinkDetails($"{API.Endpoint}{url}").Argument;

        private FontUsage getLinkFont(FontWeight fontWeight = FontWeight.Regular)
            => OsuFont.GetFont(size: FONT_SIZE, weight: fontWeight, italics: true);
    }
}
