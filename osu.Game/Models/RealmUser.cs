// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;
using Realms;

namespace osu.Game.Models
{
    public class RealmUser : EmbeddedObject, IUser
    {
        public int OnlineID { get; set; } = 1;

        public string Username { get; set; }

        public bool IsBot => false;
    }
}
