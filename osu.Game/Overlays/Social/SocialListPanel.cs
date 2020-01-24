// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Users;

namespace osu.Game.Overlays.Social
{
    public class SocialListPanel : SocialPanel
    {
        public SocialListPanel(User user)
            : base(user)
        {
            RelativeSizeAxes = Axes.X;
        }
    }
}
