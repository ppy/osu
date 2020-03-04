// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2.Users
{
    public class UserGridCard : UserCard
    {
        public UserGridCard(User user)
            : base(user)
        {
            Size = new Vector2(290, 120);
            CornerRadius = 10;
        }
    }
}
