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
                    addKudosuPart(@"Received");
                    addMainPart(@" from obtaining votes in modding post of ");
                    break;

                case KudosuAction.Give:
                    addKudosuPart(@"Received");
                    addMainPart($@" from {userLinkTemplate()} for a post at ");
                    break;

                case KudosuAction.Reset:
                    addMainPart($@"Kudosu reset by {userLinkTemplate()} for the post ");
                    break;

                case KudosuAction.VoteReset:
                    addKudosuPart(@"Lost");
                    addMainPart(@" from losing votes in modding post of ");
                    break;

                case KudosuAction.DenyKudosuReset:
                    addKudosuPart(@"Denied");
                    addMainPart(@" from modding post ");
                    break;

                case KudosuAction.Revoke:
                    addMainPart($@"Denied kudosu by {userLinkTemplate()} for the post ");
                    break;

                case KudosuAction.AllowKudosuGive:
                    addKudosuPart(@"Received");
                    addMainPart(@" from kudosu deny repeal of modding post ");
                    break;

                case KudosuAction.DeleteReset:
                    addKudosuPart(@"Lost");
                    addMainPart(@" from modding post deletion of ");
                    break;

                case KudosuAction.RestoreGive:
                    addKudosuPart(@"Received");
                    addMainPart(@" from modding post restoration of ");
                    break;

                case KudosuAction.RecalculateGive:
                    addKudosuPart(@"Received");
                    addMainPart(@" from votes recalculation in modding post of ");
                    break;

                case KudosuAction.RecalculateReset:
                    addKudosuPart(@"Lost");
                    addMainPart(@" from votes recalculation in modding post of ");
                    break;
            }

            addPostPart();
        }

        private void addKudosuPart(string prefix)
        {
            linkFlowContainer.AddText(prefix);

            linkFlowContainer.AddText($@" {historyItem.Amount} kudosu", t =>
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
