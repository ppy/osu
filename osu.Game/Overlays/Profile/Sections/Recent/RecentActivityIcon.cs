// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public partial class RecentActivityIcon : Container
    {
        private readonly SpriteIcon icon;
        private readonly APIRecentActivity activity;

        public RecentActivityIcon(APIRecentActivity activity)
        {
            this.activity = activity;
            Child = icon = new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            // references:
            // https://github.com/ppy/osu-web/blob/659b371dcadf25b4f601a4c9895a813078301084/resources/assets/lib/profile-page/parse-event.tsx
            // https://github.com/ppy/osu-web/blob/master/resources/assets/less/bem/profile-extra-entries.less#L98-L128
            switch (activity.Type)
            {
                case RecentActivityType.BeatmapPlaycount:
                    icon.Icon = FontAwesome.Solid.Play;
                    icon.Colour = Color4.White;
                    break;

                case RecentActivityType.BeatmapsetApprove:
                    icon.Icon = FontAwesome.Solid.Check;
                    icon.Colour = getColorForApprovalType(activity.Approval);
                    break;

                case RecentActivityType.BeatmapsetDelete:
                    icon.Icon = FontAwesome.Solid.TrashAlt;
                    icon.Colour = colours.Red1;
                    break;

                case RecentActivityType.BeatmapsetRevive:
                    icon.Icon = FontAwesome.Solid.TrashRestore;
                    icon.Colour = Color4.White;
                    break;

                case RecentActivityType.BeatmapsetUpdate:
                    icon.Icon = FontAwesome.Solid.SyncAlt;
                    icon.Colour = colours.Green1;
                    break;

                case RecentActivityType.BeatmapsetUpload:
                    icon.Icon = FontAwesome.Solid.ArrowUp;
                    icon.Colour = colours.Orange1;
                    break;

                case RecentActivityType.RankLost:
                    icon.Icon = FontAwesome.Solid.AngleDoubleDown;
                    icon.Colour = Color4.White;
                    break;

                case RecentActivityType.UserSupportAgain:
                    icon.Icon = FontAwesome.Solid.Heart;
                    icon.Colour = colours.Pink;
                    break;

                case RecentActivityType.UserSupportFirst:
                    icon.Icon = FontAwesome.Solid.Heart;
                    icon.Colour = colours.Pink;
                    break;

                case RecentActivityType.UserSupportGift:
                    icon.Icon = FontAwesome.Solid.Gift;
                    icon.Colour = colours.Pink;
                    break;

                case RecentActivityType.UsernameChange:
                    icon.Icon = FontAwesome.Solid.Tag;
                    icon.Colour = Color4.White;
                    break;
            }
        }

        private Color4 getColorForApprovalType(BeatmapApproval approvalType)
        {
            switch (approvalType)
            {
                case BeatmapApproval.Approved:
                case BeatmapApproval.Ranked:
                    return colours.Lime1;

                case BeatmapApproval.Loved:
                    return colours.Pink1;

                case BeatmapApproval.Qualified:
                    return colours.Blue1;

                default:
                    throw new ArgumentOutOfRangeException($"Unsupported {nameof(BeatmapApproval)} type", approvalType, nameof(approvalType));
            }
        }
    }
}
