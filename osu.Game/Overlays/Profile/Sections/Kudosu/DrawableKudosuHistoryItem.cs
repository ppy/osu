// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using System;
using osuTK;

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
                    Spacing = new Vector2(0, 3),
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
            var formattedSource = MessageFormatter.FormatText(getString(historyItem));
            linkFlowContainer.AddLinks(formattedSource.Text, formattedSource.Links);
        }

        private string getString(APIKudosuHistory item)
        {
            string amount = $"{Math.Abs(item.Amount)} kudosu";
            string post = $"[{item.Post.Title}]({item.Post.Url})";
            string giver = $"[{item.Giver?.Username}]({item.Giver?.Url})";

            return (item.Source, item.Action) switch
            {
                (KudosuSource.AllowKudosu, KudosuAction.Give) => $"Received {amount} from kudosu deny repeal of modding post {post}",
                (KudosuSource.DenyKudosu, KudosuAction.Reset) => $"Denied {amount} from modding post {post}",
                (KudosuSource.Delete, KudosuAction.Reset) => $"Lost {amount} from modding post deletion of {post}",
                (KudosuSource.Restore, KudosuAction.Give) => $"Received {amount} from modding post restoration of {post}",
                (KudosuSource.Vote, KudosuAction.Give) => $"Received {amount} from obtaining votes in modding post of {post}",
                (KudosuSource.Vote, KudosuAction.Reset) => $"Lost {amount} from losing votes in modding post of {post}",
                (KudosuSource.Recalculate, KudosuAction.Give) => $"Received {amount} from votes recalculation in modding post of {post}",
                (KudosuSource.Recalculate, KudosuAction.Reset) => $"Lost {amount} from votes recalculation in modding post of {post}",
                (KudosuSource.Forum, KudosuAction.Give) => $"Received {amount} from {giver} for a post at {post}",
                (KudosuSource.Forum, KudosuAction.Reset) => $"Kudosu reset by {giver} for the post {post}",
                (KudosuSource.Forum, KudosuAction.Revoke) => $"Denied kudosu by {giver} for the post {post}",
                _ => $"Unknown event ({amount} change)",
            };
        }
    }
}
