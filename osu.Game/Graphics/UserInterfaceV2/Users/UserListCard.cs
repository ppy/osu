// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;
using osu.Framework.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2.Users
{
    public class UserListCard : UserCard
    {
        public UserListCard(User user)
            : base(user)
        {
            RelativeSizeAxes = Axes.X;
            Height = 40;
            CornerRadius = 6;
        }
    }
}
