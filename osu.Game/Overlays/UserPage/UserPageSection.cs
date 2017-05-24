// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Users;

namespace osu.Game.Overlays.UserPage
{
    public abstract class UserPageSection : Container
    {
        protected readonly User User;
        public abstract string Title { get; }

        protected UserPageSection(User user)
        {
            User = user;
        }
    }
}
