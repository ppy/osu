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
    public class DrawableKudosuHistoryItem : CompositeDrawable
    {
        private const int height = 25;

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly APIKudosuHistory historyItem;
        private readonly LinkFlowContainer linkFlowContainer;
        private readonly DrawableDate date;

        public DrawableKudosuHistoryItem(APIKudosuHistory historyItem)
        {
            this.historyItem = historyItem;

            Height = height;
            RelativeSizeAxes = Axes.X;
            AddRangeInternal(new Drawable[]
            {
                linkFlowContainer = new LinkFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                },
                date = new DrawableDate(historyItem.CreatedAt)
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            date.Colour = colours.GreySeafoamLighter;

            switch (historyItem.Action)
            {
                case KudosuAction.VoteGive:
                case KudosuAction.Give:
                    linkFlowContainer.AddText(@"Received ");
                    addKudosuPart();
                    addMainPart();
                    addPostPart();
                    break;

                case KudosuAction.Reset:
                    addMainPart();
                    addPostPart();
                    break;

                case KudosuAction.VoteReset:
                    linkFlowContainer.AddText(@"Lost ");
                    addKudosuPart();
                    addMainPart();
                    addPostPart();
                    break;

                case KudosuAction.DenyKudosuReset:
                    linkFlowContainer.AddText(@"Denied ");
                    addKudosuPart();
                    addMainPart();
                    addPostPart();
                    break;

                case KudosuAction.Revoke:
                    addMainPart();
                    addPostPart();
                    break;
            }
        }

        private void addKudosuPart()
        {
            linkFlowContainer.AddText($@"{historyItem.Amount} kudosu", t =>
            {
                t.Font = t.Font.With(italics: true);
                t.Colour = colours.Blue;
            });
        }

        private void addMainPart()
        {
            var text = createMessage();

            linkFlowContainer.AddLinks(text.Text, text.Links);
        }

        private void addPostPart() => linkFlowContainer.AddLink(historyItem.Post.Title, historyItem.Post.Url);

        private MessageFormatter.MessageFormatterResult createMessage()
        {
            string userLinkTemplate() => $"[{historyItem.Giver?.Url} {historyItem.Giver?.Username}]";

            string message;

            switch (historyItem.Action)
            {
                case KudosuAction.Give:
                    message = $@" from {userLinkTemplate()} for a post at ";
                    break;

                case KudosuAction.VoteGive:
                    message = @" from obtaining votes in modding post of ";
                    break;

                case KudosuAction.Reset:
                    message = $@"Kudosu reset by {userLinkTemplate()} for the post ";
                    break;

                case KudosuAction.VoteReset:
                    message = @" from losing votes in modding post of ";
                    break;

                case KudosuAction.DenyKudosuReset:
                    message = @" from modding post ";
                    break;

                case KudosuAction.Revoke:
                    message = $@"Denied kudosu by {userLinkTemplate()} for the post ";
                    break;

                default:
                    message = string.Empty;
                    break;
            }

            return MessageFormatter.FormatText(message);
        }
    }
}
