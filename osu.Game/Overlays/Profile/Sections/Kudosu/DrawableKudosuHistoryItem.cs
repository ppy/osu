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
            linkFlowContainer.AddLink(historyItem.Post.Title, historyItem.Post.Url);
        }

        private string getString(APIKudosuHistory item)
        {
            string amount = $"{Math.Abs(item.Amount)} kudosu";

            switch (item.Source)
            {
                case KudosuSource.AllowKudosu:
                    switch (item.Action)
                    {
                        case KudosuAction.Give:
                            return $"Received {amount} from kudosu deny repeal of modding post ";
                    }

                    break;

                case KudosuSource.DenyKudosu:
                    switch (item.Action)
                    {
                        case KudosuAction.Reset:
                            return $"Denied {amount} from modding post ";
                    }

                    break;

                case KudosuSource.Delete:
                    switch (item.Action)
                    {
                        case KudosuAction.Reset:
                            return $"Lost {amount} from modding post deletion of ";
                    }

                    break;

                case KudosuSource.Restore:
                    switch (item.Action)
                    {
                        case KudosuAction.Give:
                            return $"Received {amount} from modding post restoration of ";
                    }

                    break;

                case KudosuSource.Vote:
                    switch (item.Action)
                    {
                        case KudosuAction.Give:
                            return $"Received {amount} from obtaining votes in modding post of ";

                        case KudosuAction.Reset:
                            return $"Lost {amount} from losing votes in modding post of ";
                    }

                    break;

                case KudosuSource.Recalculate:
                    switch (item.Action)
                    {
                        case KudosuAction.Give:
                            return $"Received {amount} from votes recalculation in modding post of ";

                        case KudosuAction.Reset:
                            return $"Lost {amount} from votes recalculation in modding post of ";
                    }

                    break;

                case KudosuSource.Forum:

                    string giver = $"[{item.Giver?.Username}]({item.Giver?.Url})";

                    switch (historyItem.Action)
                    {
                        case KudosuAction.Give:
                            return $"Received {amount} from {giver} for a post at ";

                        case KudosuAction.Reset:
                            return $"Kudosu reset by {giver} for the post ";

                        case KudosuAction.Revoke:
                            return $"Denied kudosu by {giver} for the post ";
                    }

                    break;
            }

            return $"Unknown event ({amount} change)";
        }
    }
}
