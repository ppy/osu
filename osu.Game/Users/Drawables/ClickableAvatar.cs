// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users.Drawables
{
    public class ClickableAvatar : Container
    {
        private const string default_tooltip_text = "view profile";

        /// <summary>
        /// Whether to open the user's profile when clicked.
        /// </summary>
        public bool OpenOnClick
        {
            set => clickableArea.Enabled.Value = value;
        }

        /// <summary>
        /// By default, the tooltip will show "view profile" as avatars are usually displayed next to a username.
        /// Setting this to <c>true</c> exposes the username via tooltip for special cases where this is not true.
        /// </summary>
        public bool ShowUsernameTooltip
        {
            set => clickableArea.TooltipText = value ? (user?.Username ?? string.Empty) : default_tooltip_text;
        }

        private readonly APIUser user;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        private readonly ClickableArea clickableArea;

        /// <summary>
        /// A clickable avatar for the specified user, with UI sounds included.
        /// If <see cref="OpenOnClick"/> is <c>true</c>, clicking will open the user's profile.
        /// </summary>
        /// <param name="user">The user. A null value will get a placeholder avatar.</param>
        public ClickableAvatar(APIUser user = null)
        {
            this.user = user;

            Add(clickableArea = new ClickableArea
            {
                RelativeSizeAxes = Axes.Both,
                Action = openProfile
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LoadComponentAsync(new DrawableAvatar(user), clickableArea.Add);
        }

        private void openProfile()
        {
            if (user?.Id > 1)
                game?.ShowUser(user);
        }

        private class ClickableArea : OsuClickableContainer
        {
            private LocalisableString tooltip = default_tooltip_text;

            public ClickableArea()
                : base(HoverSampleSet.Submit)
            {
            }

            public override LocalisableString TooltipText
            {
                get => Enabled.Value ? tooltip : default;
                set => tooltip = value;
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (!Enabled.Value)
                    return false;

                return base.OnClick(e);
            }
        }
    }
}
