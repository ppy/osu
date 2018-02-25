using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public class DrawableRecentActivity : DrawableProfileRow
    {
        private RecentActivity activity;
        private User user;

        public DrawableRecentActivity(RecentActivity activity, User user)
        {
            this.activity = activity;
            this.user = user;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LeftFlowContainer.Add(new OsuSpriteText
            {
                Text = activityToString(),
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

        private string activityToString()
        {
            switch (activity.Type)
            {
                case RecentActivityType.Achievement:
                    return $"{activity.User.Username} unlocked the {activity.AchivementName} achievement!";

                case RecentActivityType.BeatmapPlaycount:
                    return $"{activity.Beatmap.Title} has been played {activity.Count} times!";

                case RecentActivityType.BeatmapsetDelete:
                    return $"{activity.Beatmap.Title} has been deleted.";

                case RecentActivityType.BeatmapsetRevive:
                    return $"{activity.Beatmap.Title} has been revived from eternal slumber by ${activity.User.Username}";

                case RecentActivityType.BeatmapsetUpdate:
                    return $"{activity.User.Username} has updated the beatmap ${activity.Beatmap.Title}";

                case RecentActivityType.BeatmapsetUpload:
                    return $"{activity.User.Username} has submitted a new beatmap ${activity.Beatmap.Title}";

                case RecentActivityType.Medal:
                    return $"{activity.User.Username} has unlocked the {activity.AchivementName} medal!";

                case RecentActivityType.Rank:
                    return $"{activity.User.Username} achieved rank #{activity.Rank} on {activity.Beatmap?.Title}";

                case RecentActivityType.RankLost:
                    return $"{activity.User.Username} has lost first place on {activity.Beatmap.Title}!";

                case RecentActivityType.UserSupportAgain:
                    return $"{activity.User.Username} has once again chosen to support osu! - thanks for your generosity!";

                case RecentActivityType.UserSupportFirst:
                    return $"{activity.User.Username} has become an osu! supporter - thanks for your generosity!";

                case RecentActivityType.UsernameChange:
                    return $"{activity.User.PreviousUsername} has changed their username to {activity.User.Username}";

                case RecentActivityType.UserSupportGift:
                    return $"{activity.User.Username} has received the gift of osu! supporter!";

                default:
                    return string.Empty;
            }
        }
    }
}
