// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Users;

namespace osu.Game.Overlays.Social
{
    public class SocialGridPanel : SocialPanel
    {
        public SocialGridPanel(User user) : base(user)
        {
            Width = 300;
        }
    }
}
