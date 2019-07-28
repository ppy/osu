// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Leaderboards;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public class DrawableRecentActivity : DrawableProfileRow
    {
        private IAPIProvider api;

        private readonly APIRecentActivity activity;

        private LinkFlowContainer content;

        public DrawableRecentActivity(APIRecentActivity activity)
        {
            this.activity = activity;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            this.api = api;

            LeftFlowContainer.Padding = new MarginPadding { Left = 10, Right = 160 };

            LeftFlowContainer.Add(content = new LinkFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            });

            RightFlowContainer.Add(new DrawableDate(activity.CreatedAt)
            {
                Font = OsuFont.GetFont(size: 13),
                Colour = OsuColour.Gray(0xAA),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            });

            var formatted = createMessage();

            content.AddLinks(formatted.Text, formatted.Links);
        }

        protected override Drawable CreateLeftVisual()
        {
            switch (activity.Type)
            {
                case RecentActivityType.Rank:
                    return new UpdateableRank(activity.ScoreRank)
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

        private MessageFormatter.MessageFormatterResult createMessage()
        {
            string userLinkTemplate() => $"[{toAbsoluteUrl(activity.User?.Url)} {activity.User?.Username}]";
            string beatmapLinkTemplate() => $"[{toAbsoluteUrl(activity.Beatmap?.Url)} {activity.Beatmap?.Title}]";
            string beatmapsetLinkTemplate() => $"[{toAbsoluteUrl(activity.Beatmapset?.Url)} {activity.Beatmapset?.Title}]";

            string message;

            switch (activity.Type)
            {
                case RecentActivityType.Achievement:
                    message = $"{userLinkTemplate()} unlocked the {activity.Achievement.Name} medal!";
                    break;

                case RecentActivityType.BeatmapPlaycount:
                    message = $"{beatmapLinkTemplate()} has been played {activity.Count} times!";
                    break;

                case RecentActivityType.BeatmapsetApprove:
                    message = $"{beatmapsetLinkTemplate()} has been {activity.Approval.ToString().ToLowerInvariant()}!";
                    break;

                case RecentActivityType.BeatmapsetDelete:
                    message = $"{beatmapsetLinkTemplate()} has been deleted.";
                    break;

                case RecentActivityType.BeatmapsetRevive:
                    message = $"{beatmapsetLinkTemplate()} has been revived from eternal slumber by {userLinkTemplate()}.";
                    break;

                case RecentActivityType.BeatmapsetUpdate:
                    message = $"{userLinkTemplate()} has updated the beatmap {beatmapsetLinkTemplate()}!";
                    break;

                case RecentActivityType.BeatmapsetUpload:
                    message = $"{userLinkTemplate()} has submitted a new beatmap {beatmapsetLinkTemplate()}!";
                    break;

                case RecentActivityType.Medal:
                    // apparently this shouldn't exist look at achievement instead (https://github.com/ppy/osu-web/blob/master/resources/assets/coffee/react/profile-page/recent-activity.coffee#L111)
                    message = string.Empty;
                    break;

                case RecentActivityType.Rank:
                    message = $"{userLinkTemplate()} achieved rank #{activity.Rank} on {beatmapLinkTemplate()} ({activity.Mode}!)";
                    break;

                case RecentActivityType.RankLost:
                    message = $"{userLinkTemplate()} has lost first place on {beatmapLinkTemplate()} ({activity.Mode}!)";
                    break;

                case RecentActivityType.UserSupportAgain:
                    message = $"{userLinkTemplate()} has once again chosen to support osu! - thanks for your generosity!";
                    break;

                case RecentActivityType.UserSupportFirst:
                    message = $"{userLinkTemplate()} has become an osu!supporter - thanks for your generosity!";
                    break;

                case RecentActivityType.UserSupportGift:
                    message = $"{userLinkTemplate()} has received the gift of osu!supporter!";
                    break;

                case RecentActivityType.UsernameChange:
                    message = $"{activity.User?.PreviousUsername} has changed their username to {userLinkTemplate()}!";
                    break;

                default:
                    message = string.Empty;
                    break;
            }

            return MessageFormatter.FormatText(message);
        }
    }
}
