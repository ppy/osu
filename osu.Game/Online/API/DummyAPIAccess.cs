// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    public class DummyAPIAccess : IAPIProvider
    {
        public Bindable<User> LocalUser { get; } = new Bindable<User>(new User
        {
            Username = @"Dummy",
            Id = 1001,
        });

        public bool IsLoggedIn => true;

        public string ProvidedUsername => LocalUser.Value.Username;

        public string Endpoint => "http://localhost";

        public APIState State => LocalUser.Value.Id == 1 ? APIState.Offline : APIState.Online;

        public virtual void Queue(APIRequest request)
        {
        }

        public void Register(IOnlineComponent component)
        {
            // todo: add support
        }

        public void Unregister(IOnlineComponent component)
        {
            // todo: add support
        }

        public void Login(string username, string password)
        {
            LocalUser.Value = new User
            {
                Username = @"Dummy",
                Id = 1001,
            };
        }

        public void Logout()
        {
            LocalUser.Value = new GuestUser();
        }

        public RegistrationRequest.RegistrationRequestErrors CreateAccount(string email, string username, string password) => null;
    }
}
