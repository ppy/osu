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
            => activity.Type switch
            {
                RecentActivityType.Rank => new UpdateableRank(activity.ScoreRank)
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 60,
                    FillMode = FillMode.Fit,
                },

                RecentActivityType.Achievement => new DelayedLoadWrapper(new MedalIcon(activity.Achievement.Slug)
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                })
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 60,
                },

                _ => new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 60,
                    FillMode = FillMode.Fit,
                },
            };

        private string toAbsoluteUrl(string url) => $"{api.Endpoint}{url}";

        private MessageFormatter.MessageFormatterResult createMessage()
        {
            string userLinkTemplate() => $"[{toAbsoluteUrl(activity.User?.Url)} {activity.User?.Username}]";
            string beatmapLinkTemplate() => $"[{toAbsoluteUrl(activity.Beatmap?.Url)} {activity.Beatmap?.Title}]";
            string beatmapsetLinkTemplate() => $"[{toAbsoluteUrl(activity.Beatmapset?.Url)} {activity.Beatmapset?.Title}]";

            var message = activity.Type switch
            {
                RecentActivityType.Achievement => $"{userLinkTemplate()} unlocked the {activity.Achievement.Name} medal!",
                RecentActivityType.BeatmapPlaycount => $"{beatmapLinkTemplate()} has been played {activity.Count} times!",
                RecentActivityType.BeatmapsetApprove => $"{beatmapsetLinkTemplate()} has been {activity.Approval.ToString().ToLowerInvariant()}!",
                RecentActivityType.BeatmapsetDelete => $"{beatmapsetLinkTemplate()} has been deleted.",
                RecentActivityType.BeatmapsetRevive => $"{beatmapsetLinkTemplate()} has been revived from eternal slumber by {userLinkTemplate()}.",
                RecentActivityType.BeatmapsetUpdate => $"{userLinkTemplate()} has updated the beatmap {beatmapsetLinkTemplate()}!",
                RecentActivityType.BeatmapsetUpload => $"{userLinkTemplate()} has submitted a new beatmap {beatmapsetLinkTemplate()}!",
                RecentActivityType.Medal => string.Empty,
                RecentActivityType.Rank => $"{userLinkTemplate()} achieved rank #{activity.Rank} on {beatmapLinkTemplate()} ({activity.Mode}!)",
                RecentActivityType.RankLost => $"{userLinkTemplate()} has lost first place on {beatmapLinkTemplate()} ({activity.Mode}!)",
                RecentActivityType.UserSupportAgain => $"{userLinkTemplate()} has once again chosen to support osu! - thanks for your generosity!",
                RecentActivityType.UserSupportFirst => $"{userLinkTemplate()} has become an osu!supporter - thanks for your generosity!",
                RecentActivityType.UserSupportGift => $"{userLinkTemplate()} has received the gift of osu!supporter!",
                RecentActivityType.UsernameChange => $"{activity.User?.PreviousUsername} has changed their username to {userLinkTemplate()}!",
                _ => string.Empty,
            };
            return MessageFormatter.FormatText(message);
        }
    }
}
