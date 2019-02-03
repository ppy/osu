// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    public class DummyAPIAccess : IAPIProvider
    {
        public Bindable<User> LocalUser { get; } = new Bindable<User>(new User
        {
            Username = @"Dummy",
            Id = 1,
        });

        public bool IsLoggedIn => true;

        public void Update()
        {
        }

        public virtual void Queue(APIRequest request)
        {
        }

        public void Register(IOnlineComponent component)
        {
        }
    }
}
