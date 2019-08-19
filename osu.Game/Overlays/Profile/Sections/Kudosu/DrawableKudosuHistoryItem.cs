// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Profile.Sections.Kudosu
{
    public class DrawableKudosuHistoryItem : DrawableProfileRow
    {
        private readonly APIKudosuHistory historyItem;
        private LinkFlowContainer content;

        public DrawableKudosuHistoryItem(APIKudosuHistory historyItem)
        {
            this.historyItem = historyItem;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LeftFlowContainer.Padding = new MarginPadding { Left = 10 };

            LeftFlowContainer.Add(content = new LinkFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            });

            RightFlowContainer.Add(new DrawableDate(historyItem.CreatedAt)
            {
                Font = OsuFont.GetFont(size: 13),
                Colour = OsuColour.Gray(0xAA),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            });

            var formatted = createMessage();

            content.AddLinks(formatted.Text, formatted.Links);
        }

        protected override Drawable CreateLeftVisual() => new Container
        {
            RelativeSizeAxes = Axes.Y,
            AutoSizeAxes = Axes.X,
        };

        private MessageFormatter.MessageFormatterResult createMessage()
        {
            string postLinkTemplate() => $"({historyItem.Post.Title})[{historyItem.Post.Url}]";
            string userLinkTemplate() => $"[{historyItem.Giver?.Url} {historyItem.Giver?.Username}]";

            string message;

            switch (historyItem.Action)
            {
                case KudosuAction.Give:
                    message = $"Received {historyItem.Amount} kudosu from {userLinkTemplate()} for a post at {postLinkTemplate()}";
                    break;

                case KudosuAction.VoteGive:
                    message = $"Received {historyItem.Amount} kudosu from obtaining votes in modding post of {postLinkTemplate()}";
                    break;

                case KudosuAction.Reset:
                    message = $"Kudosu reset by {userLinkTemplate()} for the post {postLinkTemplate()}";
                    break;

                case KudosuAction.VoteReset:
                    message = $"Lost {historyItem.Amount} kudosu from losing votes in modding post of {postLinkTemplate()}";
                    break;

                case KudosuAction.DenyKudosuReset:
                    message = $"Denied {historyItem.Amount} kudosu from modding post {postLinkTemplate()}";
                    break;

                case KudosuAction.Revoke:
                    message = $"Denied kudosu by {userLinkTemplate()} for the post {postLinkTemplate()}";
                    break;

                default:
                    message = string.Empty;
                    break;
            }

            return MessageFormatter.FormatText(message);
        }
    }
}
