// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Colour;
using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;

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

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.Width = 0.5f;
            Background.Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(1), Color4.White);
        }
    }
}
