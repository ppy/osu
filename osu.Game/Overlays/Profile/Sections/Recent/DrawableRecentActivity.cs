// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public class DrawableRecentActivity : DrawableProfileRow
    {
        private APIAccess api;

        private readonly RecentActivity activity;

        private string userLinkTemplate;
        private string beatmapLinkTemplate;
        private string beatmapsetLinkTemplate;

        private LinkFlowContainer content;

        public DrawableRecentActivity(RecentActivity activity)
        {
            this.activity = activity;
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;

            userLinkTemplate = $"[{toAbsoluteUrl(activity.User?.Url)} {activity.User?.Username}]";
            beatmapLinkTemplate = $"[{toAbsoluteUrl(activity.Beatmap?.Url)} {activity.Beatmap?.Title}]";
            beatmapsetLinkTemplate = $"[{toAbsoluteUrl(activity.Beatmapset?.Url)} {activity.Beatmapset?.Title}]";

            LeftFlowContainer.Padding = new MarginPadding { Left = 10, Right = 160 };

            LeftFlowContainer.Add(content = new LinkFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            });

            RightFlowContainer.Add(new OsuSpriteText
            {
                Text = activity.CreatedAt.LocalDateTime.ToShortDateString(),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Font = "Exo2.0-RegularItalic",
                TextSize = 12,
                Colour = OsuColour.Gray(0xAA),
            });

            var formatted = MessageFormatter.FormatText(activityToString());

            content.AddLinks(formatted.Text, formatted.Links);
        }

        protected override Drawable CreateLeftVisual()
        {
            switch (activity.Type)
            {
                case RecentActivityType.Rank:
                    return new DrawableRank(activity.ScoreRank)
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 60,
                        FillMode = FillMode.Fit,
                    };

                case RecentActivityType.Achievement:
                    return new MedalIcon(activity.Achievement.Slug)
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 60,
                        FillMode = FillMode.Fit,
                    };

                default:
                    return new Container
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 60,
                        FillMode = FillMode.Fit,
                    };
            }
        }

        private string toAbsoluteUrl(string url) => $"{api.Endpoint}{url}";

        private string activityToString()
        {
            switch (activity.Type)
            {
                case RecentActivityType.Achievement:
                    return $"{userLinkTemplate} unlocked the {activity.Achievement.Name} medal!";

                case RecentActivityType.BeatmapPlaycount:
                    return $"{beatmapLinkTemplate} has been played {activity.Count} times!";

                case RecentActivityType.BeatmapsetApprove:
                    return $"{beatmapsetLinkTemplate} has been {activity.Approval.ToString().ToLowerInvariant()}!";

                case RecentActivityType.BeatmapsetDelete:
                    return $"{beatmapsetLinkTemplate} has been deleted.";

                case RecentActivityType.BeatmapsetRevive:
                    return $"{beatmapsetLinkTemplate} has been revived from eternal slumber by {userLinkTemplate}.";

                case RecentActivityType.BeatmapsetUpdate:
                    return $"{userLinkTemplate} has updated the beatmap {beatmapsetLinkTemplate}!";

                case RecentActivityType.BeatmapsetUpload:
                    return $"{userLinkTemplate} has submitted a new beatmap {beatmapsetLinkTemplate}!";

                case RecentActivityType.Medal:
                    // apparently this shouldn't exist look at achievement instead (https://github.com/ppy/osu-web/blob/master/resources/assets/coffee/react/profile-page/recent-activity.coffee#L111)
                    return string.Empty;

                case RecentActivityType.Rank:
                    return $"{userLinkTemplate} achieved rank #{activity.Rank} on {beatmapLinkTemplate} ({activity.Mode}!)";

                case RecentActivityType.RankLost:
                    return $"{userLinkTemplate} has lost first place on {beatmapLinkTemplate} ({activity.Mode}!)";

                case RecentActivityType.UserSupportAgain:
                    return $"{userLinkTemplate} has once again chosen to support osu! - thanks for your generosity!";

                case RecentActivityType.UserSupportFirst:
                    return $"{userLinkTemplate} has become an osu! supporter - thanks for your generosity!";

                case RecentActivityType.UserSupportGift:
                    return $"{userLinkTemplate} has received the gift of osu! supporter!";

                case RecentActivityType.UsernameChange:
                    return $"{activity.User.PreviousUsername} has changed their username to {userLinkTemplate}!";

                default:
                    return string.Empty;
            }
        }
    }
}
