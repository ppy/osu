// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Team.Header.Components
{
    public partial class TeamActionsButton : ProfileActionsButton
    {
        public readonly Bindable<TeamProfileData?> TeamData = new Bindable<TeamProfileData?>();

        private TeamReportPopoverTarget reportPopoverTarget = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            // This is a bit of a dirty hack. Because `ReportTeamPopover` is spawned from `TeamActionsPopover`,
            // and that they both share the same `PopoverContainer`, the former will get destroyed when the latter
            // is opened, causing it to get destroyed as well.
            //
            // This is worked around by having an additional dummy popover target on the actions button,
            // which is then passed to `TeamActionsPopover` and the user report action. This way the popover
            // can remain attached to it once the actions popover is destroyed.
            reportPopoverTarget = new TeamReportPopoverTarget
            {
                RelativeSizeAxes = Axes.Both,
            };
            Add(reportPopoverTarget);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            TeamData.BindValueChanged(_ =>
            {
                reportPopoverTarget.Team = TeamData.Value?.Team;
            });
        }

        public override Popover GetPopover() => new TeamActionPopover(reportPopoverTarget);

        private partial class TeamActionPopover : ProfileActionPopover
        {
            private readonly IHasPopover reportPopoverTarget;

            public TeamActionPopover(IHasPopover reportPopoverTarget)
            {
                this.reportPopoverTarget = reportPopoverTarget;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Actions = new[]
                {
                    new ProfilePopoverAction(FontAwesome.Solid.ExclamationTriangle, ReportStrings.TeamButton)
                    {
                        Action = () =>
                        {
                            this.HidePopover();
                            reportPopoverTarget.ShowPopover();
                        }
                    }
                };
            }
        }

        private partial class TeamReportPopoverTarget : Container, IHasPopover
        {
            public APITeam? Team;

            public Popover? GetPopover() => Team != null ? new ReportTeamPopover(Team) : null;
        }
    }
}
