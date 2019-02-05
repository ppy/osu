// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using System.Collections.Generic;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public abstract class ClickableUserContainer : Container
    {
        private UserProfileOverlay profile;

        private User user;
        public User User
        {
            get { return user; }
            set
            {
                if (user == value) return;
                user = value;

                OnUserChange(user);
            }
        }

        public ClickableUserContainer()
        {
            AutoSizeAxes = Axes.Both;
        }

        protected abstract void OnUserChange(User user);

        [BackgroundDependencyLoader(true)]
        private void load(UserProfileOverlay profile)
        {
            this.profile = profile;
        }

        protected override bool OnClick(ClickEvent e)
        {
            profile?.ShowUser(user);
            return true;
        }
    }
}
