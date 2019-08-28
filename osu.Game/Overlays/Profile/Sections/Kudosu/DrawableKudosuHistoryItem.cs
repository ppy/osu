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

            string prefix = getPrefix(historyItem);
            var formattedSource = MessageFormatter.FormatText(getSource(historyItem));

            if (!string.IsNullOrEmpty(prefix))
            {
                linkFlowContainer.AddText(prefix);
                linkFlowContainer.AddText($@" {historyItem.Amount} kudosu", t =>
                {
                    t.Font = t.Font.With(italics: true);
                    t.Colour = colours.Blue;
                });
            }

            linkFlowContainer.AddLinks(formattedSource.Text + " ", formattedSource.Links);
            linkFlowContainer.AddLink(historyItem.Post.Title, historyItem.Post.Url);
        }

        private string getSource(APIKudosuHistory historyItem)
        {
            string userLink() => $"[{historyItem.Giver?.Url} {historyItem.Giver?.Username}]";

            switch (historyItem.Action)
            {
                case KudosuAction.VoteGive:
                    return @" from obtaining votes in modding post of";

                case KudosuAction.Give:
                    return $@" from {userLink()} for a post at";

                case KudosuAction.Reset:
                    return $@"Kudosu reset by {userLink()} for the post";

                case KudosuAction.VoteReset:
                    return @" from losing votes in modding post of";

                case KudosuAction.DenyKudosuReset:
                    return @" from modding post";

                case KudosuAction.Revoke:
                    return $@"Denied kudosu by {userLink()} for the post";

                case KudosuAction.AllowKudosuGive:
                    return @" from kudosu deny repeal of modding post";

                case KudosuAction.DeleteReset:
                    return @" from modding post deletion of";

                case KudosuAction.RestoreGive:
                    return @" from modding post restoration of";

                case KudosuAction.RecalculateGive:
                    return @" from votes recalculation in modding post of";

                case KudosuAction.RecalculateReset:
                    return @" from votes recalculation in modding post of";

                default:
                    return @" from unknown event ";
            }
        }

        private string getPrefix(APIKudosuHistory historyItem)
        {
            switch (historyItem.Action)
            {
                case KudosuAction.VoteGive:
                    return @"Received";

                case KudosuAction.Give:
                    return @"Received";

                case KudosuAction.VoteReset:
                    return @"Lost";

                case KudosuAction.DenyKudosuReset:
                    return @"Denied";

                case KudosuAction.AllowKudosuGive:
                    return @"Received";

                case KudosuAction.DeleteReset:
                    return @"Lost";

                case KudosuAction.RestoreGive:
                    return @"Received";

                case KudosuAction.RecalculateGive:
                    return @"Received";

                case KudosuAction.RecalculateReset:
                    return @"Lost";

                default:
                    return null;
            }
        }
    }
}
