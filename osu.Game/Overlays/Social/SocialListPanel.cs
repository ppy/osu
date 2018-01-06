// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Users;

namespace osu.Game.Overlays.Social
{
    public class SocialListPanel : SocialPanel
    {
        public SocialListPanel(User user) : base(user)
        {
            RelativeSizeAxes = Axes.X;
        }
    }
}
