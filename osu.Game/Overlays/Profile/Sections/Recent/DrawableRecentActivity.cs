// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        protected override string GetString()
        {
            switch (Item.Type)
            {
                case RecentActivityType.Achievement:
                    return ":user unlocked the \":achievement\" medal!";

                case RecentActivityType.BeatmapPlaycount:
                    return ":beatmap has been played :count times!";

                case RecentActivityType.BeatmapsetApprove:
                    return ":beatmapset has been :approval!";

                case RecentActivityType.BeatmapsetDelete:
                    return ":beatmapset has been deleted.";

                case RecentActivityType.BeatmapsetRevive:
                    return ":beatmapset has been revived from eternal slumber by :user.";

                case RecentActivityType.BeatmapsetUpdate:
                    return ":user has updated the beatmap \":beatmapset\"";

                case RecentActivityType.BeatmapsetUpload:
                    return ":user has submitted a new beatmap \":beatmapset\"";

                case RecentActivityType.Medal:
                    // apparently this shouldn't exist look at achievement instead (https://github.com/ppy/osu-web/blob/master/resources/assets/coffee/react/profile-page/recent-activity.coffee#L111)
                    break;

                case RecentActivityType.Rank:
                    return ":user achieved rank #:rank on :beatmap (:mode)";

                case RecentActivityType.RankLost:
                    return ":user has lost first place on :beatmap (:mode)";

                case RecentActivityType.UserSupportAgain:
                    return ":user has once again chosen to support osu! - thanks for your generosity!";

                case RecentActivityType.UserSupportFirst:
                    return ":user has become an osu!supporter - thanks for your generosity!";

                case RecentActivityType.UserSupportGift:
                    return ":user has received the gift of osu!supporter!";

                case RecentActivityType.UsernameChange:
                    return ":previousUsername has changed their username to :user";
            }

            return string.Empty;
        }

        protected override (string name, Action action)[] GetProperties() => new (string, Action)[]
        {
            ("achievement", () => AddText($"{Item.Achievement.Name}")),
            ("user", () => AddUserLink(Item.User?.Username, Item.User?.Url)),
            ("beatmapset", () => AddLink(Item.Beatmapset?.Title, LinkAction.OpenBeatmapSet, Item.Beatmapset?.Url)),
            ("beatmap", () => AddLink(Item.Beatmap?.Title, LinkAction.OpenBeatmap, Item.Beatmap?.Url)),
            ("previousUsername", () => AddText($"{Item.User?.PreviousUsername}")),
            ("rank", () => AddText($"{Item.Rank}")),
            ("count", () => AddText($"{Item.Count}")),
            ("mode", () => AddText($"{getRulesetName()}")),
            ("approval", () => AddText($"{Item.Approval.ToString().ToLowerInvariant()}"))
        };

        private string getRulesetName() =>
            rulesets.AvailableRulesets.FirstOrDefault(r => r.ShortName == Item.Mode)?.Name ?? Item.Mode;
    }
}
