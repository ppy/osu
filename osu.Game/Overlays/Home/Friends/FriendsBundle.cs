// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK.Graphics;

namespace osu.Game.Overlays.Home.Friends
{
    public class FriendsBundle
    {
        public FriendsOnlineStatus Status { get; }

        public int Amount { get; }

        public Color4 Colour => getColour();

        public FriendsBundle(FriendsOnlineStatus status, int amount)
        {
            Status = status;
            Amount = amount;
        }

        private Color4 getColour()
        {
            switch (Status)
            {
                default:
                    throw new ArgumentException($@"{Status} status does not provide a colour in {nameof(getColour)}.");

                case FriendsOnlineStatus.All:
                    return Color4.White;

                case FriendsOnlineStatus.Online:
                    return Color4.Lime;

                case FriendsOnlineStatus.Offline:
                    return Color4.Black;
            }
        }
    }

    public enum FriendsOnlineStatus
    {
        All,
        Online,
        Offline
    }
}
