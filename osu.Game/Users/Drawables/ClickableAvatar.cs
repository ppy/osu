// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;

namespace osu.Game.Users.Drawables
{
    public class ClickableAvatar : Container
    {
        /// <summary>
        /// Whether to open the user's profile when clicked.
        /// </summary>
        public readonly BindableBool OpenOnClick = new BindableBool(true);

        private readonly User user;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        /// <summary>
        /// A clickable avatar for the specified user, with UI sounds included.
        /// If <see cref="OpenOnClick"/> is <c>true</c>, clicking will open the user's profile.
        /// </summary>
        /// <param name="user">The user. A null value will get a placeholder avatar.</param>
        public ClickableAvatar(User user = null)
        {
            this.user = user;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            ClickableArea clickableArea;
            Add(clickableArea = new ClickableArea
            {
                RelativeSizeAxes = Axes.Both,
                Action = openProfile
            });

            LoadComponentAsync(new DrawableAvatar(user), clickableArea.Add);

            clickableArea.Enabled.BindTo(OpenOnClick);
        }

        private void openProfile()
        {
            if (!OpenOnClick.Value)
                return;

            if (user?.Id > 1)
                game?.ShowUser(user.Id);
        }

        private class ClickableArea : OsuClickableContainer
        {
            public override string TooltipText => Enabled.Value ? @"view profile" : null;

            protected override bool OnClick(ClickEvent e)
            {
                if (!Enabled.Value)
                    return false;

                return base.OnClick(e);
            }
        }
    }
}
