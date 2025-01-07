// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Online.Friends
{
    public interface IFriendsClient
    {
        Task FriendConnected(int userId);
        Task FriendDisconnected(int userId);
    }
}
