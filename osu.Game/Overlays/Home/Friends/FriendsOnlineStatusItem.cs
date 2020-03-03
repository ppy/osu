// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Overlays.Home.Friends
{
    public class FriendsOnlineStatusItem : OverlayUpdateStreamItem<FriendsBundle>
    {
        public FriendsOnlineStatusItem(FriendsBundle value)
            : base(value)
        {
        }

        protected override string GetMainText() => Value.Status.ToString();

        protected override string GetAdditionalText() => Value.Amount.ToString();

        protected override Color4 GetBarColour() => Value.Colour;
    }
}
