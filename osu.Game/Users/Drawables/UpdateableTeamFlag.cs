// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users.Drawables
{
    /// <summary>
    /// A team logo which can update to a new team when needed.
    /// </summary>
    public partial class UpdateableTeamFlag : ModelBackedDrawable<APITeam?>
    {
        public APITeam? Team
        {
            get => Model;
            set
            {
                Model = value;
                Invalidate(Invalidation.Presence);
            }
        }

        public new bool Masking
        {
            get => base.Masking;
            set => base.Masking = value;
        }

        public new float CornerRadius
        {
            get => base.CornerRadius;
            set => base.CornerRadius = value;
        }

        public new EdgeEffectParameters EdgeEffect
        {
            get => base.EdgeEffect;
            set => base.EdgeEffect = value;
        }

        /// <summary>
        /// Perform an action in addition to showing the team profile.
        /// This should be used to perform auxiliary tasks and not as a primary action for clicking a flag (to maintain a consistent UX).
        /// Ignored if `isInteractive` is false.
        /// </summary>
        public Action? Action;

        protected override double LoadDelay => 200;

        private readonly bool isInteractive;
        private readonly bool hideOnNull;
        private readonly bool showTooltipOnHover;

        /// <summary>
        /// Construct a new UpdateableTeamFlag.
        /// </summary>
        /// <param name="team">The initial team to display.</param>
        /// <param name="isInteractive">If set to true, hover/click sounds will play and clicking the flag will open the team's profile.</param>
        /// <param name="showTooltipOnHover">
        /// If set to true, the team's name is displayed in the tooltip.
        /// Only has an effect if <see cref="isInteractive"/> is true.
        /// </param>
        /// <param name="hideOnNull">Whether to hide the flag when the provided team is null.</param>
        public UpdateableTeamFlag(APITeam? team = null, bool isInteractive = true, bool hideOnNull = true, bool showTooltipOnHover = true)
        {
            this.isInteractive = isInteractive;
            this.hideOnNull = hideOnNull;
            this.showTooltipOnHover = showTooltipOnHover;

            Team = team;

            Masking = true;
        }

        protected override Drawable? CreateDrawable(APITeam? team)
        {
            if (team == null && hideOnNull)
                return Empty();

            if (isInteractive)
            {
                return new ClickableTeamFlag(team, showTooltipOnHover)
                {
                    RelativeSizeAxes = Axes.Both,
                    Action = Action,
                };
            }

            return new DrawableTeamFlag(team)
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        // Generally we just want team flags to disappear if the user doesn't have one.
        // This also handles fill flow cases and avoids spacing being added for non-displaying flags.
        public override bool IsPresent => base.IsPresent && (Team != null || !hideOnNull);
    }
}
