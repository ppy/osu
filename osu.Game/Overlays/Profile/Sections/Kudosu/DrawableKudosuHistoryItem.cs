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

            switch (item.Source)
            {
                case KudosuSource.AllowKudosu:
                    switch (item.Action)
                    {
                        case KudosuAction.Give:
                            return $"在讨论贴 {post} 中获得了足够的票数而获得了 {amount} kudosu";
                    }

                    break;

                case KudosuSource.DenyKudosu:
                    switch (item.Action)
                    {
                        case KudosuAction.Reset:
                            return $"拒绝了讨论帖 {post} 中的 {amount} kudosu";
                    }

                    break;

                case KudosuSource.Delete:
                    switch (item.Action)
                    {
                        case KudosuAction.Reset:
                            return $"因讨论贴 {post} 被删除而失去了 {amount} kudosu";
                    }

                    break;

                case KudosuSource.Restore:
                    switch (item.Action)
                    {
                        case KudosuAction.Give:
                            return $"因在讨论贴 {post} 中获得足够的票数而获得了 {amount} kudosu";
                    }

                    break;

                case KudosuSource.Vote:
                    switch (item.Action)
                    {
                        case KudosuAction.Give:
                            return $"因在讨论帖 {post} 中获得足够的票数而获得了 {amount} kudosu";

                        case KudosuAction.Reset:
                            return $"因在讨论贴 {post} 中失去票数而失去了 {amount} kudosu";
                    }

                    break;

                case KudosuSource.Recalculate:
                    switch (item.Action)
                    {
                        case KudosuAction.Give:
                            return $"因 {post} 的选票重新计算而获得了 {amount} kudosu";

                        case KudosuAction.Reset:
                            return $"因 {post} 的选票重新计算而失去了 {amount} kudosu";
                    }

                    break;

                case KudosuSource.Forum:

                    string giver = $"[{item.Giver?.Username}]({item.Giver?.Url})";

                    switch (historyItem.Action)
                    {
                        case KudosuAction.Give:
                            return $"收到了{giver} 在 {post} 中给的 {amount} kudosu";

                        case KudosuAction.Reset:
                            return $"被 {giver} 重置了在 {post} 中给的kudosu";

                        case KudosuAction.Revoke:
                            return $"拒绝了 {giver} 在 {post} 中给的 kudosu";
                    }

                    break;
            }

            return $"未知事件 ({amount} 改变)";
        }
    }
}
