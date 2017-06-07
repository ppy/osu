// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;

namespace osu.Game.Users.Profile
{
    public abstract class ProfileSection : Container
    {
        protected readonly User User;
        public abstract string Title { get; }

        protected ProfileSection(User user)
        {
            User = user;
        }

        public override string ToString() => Title; //for tab control
    }
}
