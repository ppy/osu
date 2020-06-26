// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public class DrawableRecentActivity : DrawableHistoryItem<APIRecentActivity>
    {
        [Resolved]
        private RulesetStore rulesets { get; set; }

        public DrawableRecentActivity(APIRecentActivity activity)
            : base(activity)
        {
        }

        protected override DateTimeOffset GetDate() => Item.CreatedAt;

        protected override Drawable CreateLeftContent() => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Child = createIcon().With(icon =>
            {
                icon.Anchor = Anchor.Centre;
                icon.Origin = Anchor.Centre;
            })
        };

        protected override float LeftContentSize() => 28;

        private Drawable createIcon()
        {
            switch (Item.Type)
            {
                case RecentActivityType.Rank:
                    return new UpdateableRank(Item.ScoreRank)
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 11,
                        FillMode = FillMode.Fit,
                        Margin = new MarginPadding { Top = 2 }
                    };

                case RecentActivityType.Achievement:
                    return new DelayedLoadWrapper(new MedalIcon(Item.Achievement.Slug)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                    })
                    {
                        RelativeSizeAxes = Axes.X,
                        Width = 0.5f,
                        Height = 18
                    };

                default:
                    return Empty();
            }
        }

        protected override void CreateMessage()
        {
            switch (Item.Type)
            {
                case RecentActivityType.Achievement:
                    addUserLink();
                    AddText($" unlocked the \"{Item.Achievement.Name}\" medal!");
                    return;

                case RecentActivityType.BeatmapPlaycount:
                    addBeatmapLink();
                    AddText($" has been played {Item.Count} times!");
                    return;

                case RecentActivityType.BeatmapsetApprove:
                    addBeatmapsetLink();
                    AddText($" has been {Item.Approval.ToString().ToLowerInvariant()}!");
                    return;

                case RecentActivityType.BeatmapsetDelete:
                    addBeatmapsetLink();
                    AddText(" has been deleted.");
                    return;

                case RecentActivityType.BeatmapsetRevive:
                    addBeatmapsetLink();
                    AddText(" has been revived from eternal slumber by ");
                    addUserLink();
                    return;

                case RecentActivityType.BeatmapsetUpdate:
                    addUserLink();
                    AddText(" has updated the beatmap ");
                    addBeatmapsetLink();
                    return;

                case RecentActivityType.BeatmapsetUpload:
                    addUserLink();
                    AddText(" has submitted a new beatmap ");
                    addBeatmapsetLink();
                    return;

                case RecentActivityType.Medal:
                    // apparently this shouldn't exist look at achievement instead (https://github.com/ppy/osu-web/blob/master/resources/assets/coffee/react/profile-page/recent-activity.coffee#L111)
                    return;

                case RecentActivityType.Rank:
                    addUserLink();
                    AddText($" achieved rank #{Item.Rank} on ");
                    addBeatmapLink();
                    AddText($" ({getRulesetName()})");
                    return;

                case RecentActivityType.RankLost:
                    addUserLink();
                    AddText(" has lost first place on ");
                    addBeatmapLink();
                    AddText($" ({getRulesetName()})");
                    return;

                case RecentActivityType.UserSupportAgain:
                    addUserLink();
                    AddText(" has once again chosen to support osu! - thanks for your generosity!");
                    return;

                case RecentActivityType.UserSupportFirst:
                    addUserLink();
                    AddText(" has become an osu!supporter - thanks for your generosity!");
                    return;

                case RecentActivityType.UserSupportGift:
                    addUserLink();
                    AddText(" has received the gift of osu!supporter!");
                    return;

                case RecentActivityType.UsernameChange:
                    AddText($"{Item.User?.PreviousUsername} has changed their username to ");
                    addUserLink();
                    return;
            }
        }

        private string getRulesetName() =>
            rulesets.AvailableRulesets.FirstOrDefault(r => r.ShortName == Item.Mode)?.Name ?? Item.Mode;

        private void addUserLink() => AddUserLink(Item.User?.Username, Item.User?.Url);

        private void addBeatmapLink()
            => AddLink(Item.Beatmap?.Title, LinkAction.OpenBeatmap, Item.Beatmap?.Url);

        private void addBeatmapsetLink()
            => AddLink(Item.Beatmapset?.Title, LinkAction.OpenBeatmapSet, Item.Beatmapset?.Url);
    }
}
