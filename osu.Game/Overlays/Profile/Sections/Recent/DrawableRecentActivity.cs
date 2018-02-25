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
        private readonly string userLinkTemplate;
        private readonly string beatmapLinkTemplate;

        private LinkFlowContainer content;

        public DrawableRecentActivity(RecentActivity activity)
        {
            this.activity = activity;

            userLinkTemplate = $"[{urlToAbsolute(activity.User?.Url)} {activity.User?.Username}]";
            beatmapLinkTemplate = $"[{urlToAbsolute(activity.Beatmap?.Url)} {activity.Beatmap?.Title}]";
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;

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

                default:
                    return new Container
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 60,
                        FillMode = FillMode.Fit,
                    };
            }
        }

        private string urlToAbsolute(string url) => $"{api?.Endpoint ?? @"https://osu.ppy.sh"}{url}";

        private string activityToString()
        {
            switch (activity.Type)
            {
                case RecentActivityType.Achievement:
                    return $"{userLinkTemplate} unlocked the {activity.AchivementName} achievement!";

                case RecentActivityType.BeatmapPlaycount:
                    return $"{beatmapLinkTemplate} has been played {activity.Count} times!";

                case RecentActivityType.BeatmapsetDelete:
                    return $"{beatmapLinkTemplate} has been deleted.";

                case RecentActivityType.BeatmapsetRevive:
                    return $"{beatmapLinkTemplate} has been revived from eternal slumber by ${userLinkTemplate}";

                case RecentActivityType.BeatmapsetUpdate:
                    return $"{userLinkTemplate} has updated the beatmap ${beatmapLinkTemplate}";

                case RecentActivityType.BeatmapsetUpload:
                    return $"{userLinkTemplate} has submitted a new beatmap ${beatmapLinkTemplate}";

                case RecentActivityType.Medal:
                    return $"{userLinkTemplate} has unlocked the {activity.AchivementName} medal!";

                case RecentActivityType.Rank:
                    return $"{userLinkTemplate} achieved rank #{activity.Rank} on {beatmapLinkTemplate}";

                case RecentActivityType.RankLost:
                    return $"{userLinkTemplate} has lost first place on {beatmapLinkTemplate}!";

                case RecentActivityType.UserSupportAgain:
                    return $"{userLinkTemplate} has once again chosen to support osu! - thanks for your generosity!";

                case RecentActivityType.UserSupportFirst:
                    return $"{userLinkTemplate} has become an osu! supporter - thanks for your generosity!";

                case RecentActivityType.UsernameChange:
                    return $"{activity.User.PreviousUsername} has changed their username to {userLinkTemplate}";

                case RecentActivityType.UserSupportGift:
                    return $"{userLinkTemplate} has received the gift of osu! supporter!";

                default:
                    return string.Empty;
            }
        }
    }
}
