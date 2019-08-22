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

            string userLinkTemplate() => $"[{historyItem.Giver?.Url} {historyItem.Giver?.Username}]";

            switch (historyItem.Action)
            {
                case KudosuAction.VoteGive:
                    linkFlowContainer.AddText(@"Received ");
                    addKudosuPart();
                    addMainPart(@" from obtaining votes in modding post of ");
                    addPostPart();
                    break;

                case KudosuAction.Give:
                    linkFlowContainer.AddText(@"Received ");
                    addKudosuPart();
                    addMainPart($@" from {userLinkTemplate()} for a post at ");
                    addPostPart();
                    break;

                case KudosuAction.Reset:
                    addMainPart($@"Kudosu reset by {userLinkTemplate()} for the post ");
                    addPostPart();
                    break;

                case KudosuAction.VoteReset:
                    linkFlowContainer.AddText(@"Lost ");
                    addKudosuPart();
                    addMainPart(@" from losing votes in modding post of ");
                    addPostPart();
                    break;

                case KudosuAction.DenyKudosuReset:
                    linkFlowContainer.AddText(@"Denied ");
                    addKudosuPart();
                    addMainPart(@" from modding post ");
                    addPostPart();
                    break;

                case KudosuAction.Revoke:
                    addMainPart($@"Denied kudosu by {userLinkTemplate()} for the post ");
                    addPostPart();
                    break;

                case KudosuAction.AllowKudosuGive:
                    linkFlowContainer.AddText(@"Received ");
                    addKudosuPart();
                    addMainPart(@" from kudosu deny repeal of modding post ");
                    addPostPart();
                    break;

                case KudosuAction.DeleteReset:
                    linkFlowContainer.AddText(@"Lost ");
                    addKudosuPart();
                    addMainPart(@" from modding post deletion of ");
                    addPostPart();
                    break;

                case KudosuAction.RestoreGive:
                    linkFlowContainer.AddText(@"Received ");
                    addKudosuPart();
                    addMainPart(@" from modding post restoration of ");
                    addPostPart();
                    break;

                case KudosuAction.RecalculateGive:
                    linkFlowContainer.AddText(@"Received ");
                    addKudosuPart();
                    addMainPart(@" from votes recalculation in modding post of ");
                    addPostPart();
                    break;

                case KudosuAction.RecalculateReset:
                    linkFlowContainer.AddText(@"Lost ");
                    addKudosuPart();
                    addMainPart(@" from votes recalculation in modding post of ");
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

        private void addMainPart(string text)
        {
            var formatted = MessageFormatter.FormatText(text);

            linkFlowContainer.AddLinks(formatted.Text, formatted.Links);
        }

        private void addPostPart() => linkFlowContainer.AddLink(historyItem.Post.Title, historyItem.Post.Url);
    }
}
