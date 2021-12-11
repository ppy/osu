// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public class DrawableRecentActivity : CompositeDrawable
    {
        private const int font_size = 14;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private IRulesetStore rulesets { get; set; }

        private readonly APIRecentActivity activity;

        private LinkFlowContainer content;

        public DrawableRecentActivity(APIRecentActivity activity)
        {
            this.activity = activity;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AddInternal(new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, size: 28),
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = createIcon().With(icon =>
                            {
                                icon.Anchor = Anchor.Centre;
                                icon.Origin = Anchor.Centre;
                            })
                        },
                        content = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: font_size))
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        },
                        new DrawableDate(activity.CreatedAt)
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Colour = colourProvider.Foreground1,
                            Font = OsuFont.GetFont(size: font_size),
                        }
                    }
                }
            });

            createMessage();
        }

        private Drawable createIcon()
        {
            switch (activity.Type)
            {
                case RecentActivityType.Rank:
                    return new UpdateableRank(activity.ScoreRank)
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 11,
                        FillMode = FillMode.Fit,
                        Margin = new MarginPadding { Top = 2 }
                    };

                case RecentActivityType.Achievement:
                    return new DelayedLoadWrapper(new MedalIcon(activity.Achievement.Slug)
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

        private void createMessage()
        {
            switch (activity.Type)
            {
                case RecentActivityType.Achievement:
                    addUserLink();
                    addText($" unlocked the \"{activity.Achievement.Name}\" medal!");
                    break;

                case RecentActivityType.BeatmapPlaycount:
                    addBeatmapLink();
                    addText($" has been played {activity.Count} times!");
                    break;

                case RecentActivityType.BeatmapsetApprove:
                    addBeatmapsetLink();
                    addText($" has been {activity.Approval.ToString().ToLowerInvariant()}!");
                    break;

                case RecentActivityType.BeatmapsetDelete:
                    addBeatmapsetLink();
                    addText(" has been deleted.");
                    break;

                case RecentActivityType.BeatmapsetRevive:
                    addBeatmapsetLink();
                    addText(" has been revived from eternal slumber by ");
                    addUserLink();
                    break;

                case RecentActivityType.BeatmapsetUpdate:
                    addUserLink();
                    addText(" has updated the beatmap ");
                    addBeatmapsetLink();
                    break;

                case RecentActivityType.BeatmapsetUpload:
                    addUserLink();
                    addText(" has submitted a new beatmap ");
                    addBeatmapsetLink();
                    break;

                case RecentActivityType.Medal:
                    // apparently this shouldn't exist look at achievement instead (https://github.com/ppy/osu-web/blob/master/resources/assets/coffee/react/profile-page/recent-activity.coffee#L111)
                    break;

                case RecentActivityType.Rank:
                    addUserLink();
                    addText($" achieved rank #{activity.Rank} on ");
                    addBeatmapLink();
                    addText($" ({getRulesetName()})");
                    break;

                case RecentActivityType.RankLost:
                    addUserLink();
                    addText(" has lost first place on ");
                    addBeatmapLink();
                    addText($" ({getRulesetName()})");
                    break;

                case RecentActivityType.UserSupportAgain:
                    addUserLink();
                    addText(" has once again chosen to support osu! - thanks for your generosity!");
                    break;

                case RecentActivityType.UserSupportFirst:
                    addUserLink();
                    addText(" has become an osu!supporter - thanks for your generosity!");
                    break;

                case RecentActivityType.UserSupportGift:
                    addUserLink();
                    addText(" has received the gift of osu!supporter!");
                    break;

                case RecentActivityType.UsernameChange:
                    addText($"{activity.User?.PreviousUsername} has changed their username to ");
                    addUserLink();
                    break;
            }
        }

        private string getRulesetName() =>
            rulesets.AvailableRulesets.FirstOrDefault(r => r.ShortName == activity.Mode)?.Name ?? activity.Mode;

        private void addUserLink()
            => content.AddLink(activity.User?.Username, LinkAction.OpenUserProfile, getLinkArgument(activity.User?.Url), creationParameters: t => t.Font = getLinkFont(FontWeight.Bold));

        private void addBeatmapLink()
            => content.AddLink(activity.Beatmap?.Title, LinkAction.OpenBeatmap, getLinkArgument(activity.Beatmap?.Url), creationParameters: t => t.Font = getLinkFont());

        private void addBeatmapsetLink()
            => content.AddLink(activity.Beatmapset?.Title, LinkAction.OpenBeatmapSet, getLinkArgument(activity.Beatmapset?.Url), creationParameters: t => t.Font = getLinkFont());

        private string getLinkArgument(string url) => MessageFormatter.GetLinkDetails($"{api.APIEndpointUrl}{url}").Argument.ToString();

        private FontUsage getLinkFont(FontWeight fontWeight = FontWeight.Regular)
            => OsuFont.GetFont(size: font_size, weight: fontWeight, italics: true);

        private void addText(string text)
            => content.AddText(text, t => t.Font = OsuFont.GetFont(size: font_size, weight: FontWeight.SemiBold));
    }
}
