// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users.Drawables
{
    public partial class ClickableTeamFlag : OsuClickableContainer
    {
        private readonly APITeam? team;

        [Resolved]
        private OsuGame? game { get; set; }

        /// <summary>
        /// Perform an action in addition to showing the team profile.
        /// This should be used to perform auxiliary tasks and not as a primary action for clicking a flag (to maintain a consistent UX).
        /// </summary>
        public new Action? Action;

        /// <summary>
        /// A clickable flag component for the specified team, with UI sounds and a tooltip.
        /// </summary>
        /// <param name="team">The team. A null value will show a placeholder background.</param>
        /// <param name="showTooltipOnHover">If set to true, the team's name is displayed in the tooltip.</param>
        public ClickableTeamFlag(APITeam? team, bool showTooltipOnHover = true)
        {
            this.team = team;

            if (team == null)
                return;

            base.Action = () =>
            {
                openProfile();
                Action?.Invoke();
            };

            if (showTooltipOnHover)
                TooltipText = team.Name;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LoadComponentAsync(new DrawableTeamFlag(team) { RelativeSizeAxes = Axes.Both }, Add);
        }

        private void openProfile()
        {
            if (team != null)
                game?.ShowTeam(team);
        }
    }
}
