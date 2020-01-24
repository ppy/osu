// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;

namespace osu.Game.Overlays.Social
{
    public class SocialGridPanel : SocialPanel
    {
        public SocialGridPanel(User user)
            : base(user)
        {
            Width = 300;
        }
    }
}
