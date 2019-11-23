// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    public class DummyAPIAccess : Component, IAPIProvider
    {
        public Bindable<User> LocalUser { get; } = new Bindable<User>(new User
        {
            Username = @"Dummy",
            Id = 1001,
        });

        public Bindable<UserActivity> Activity { get; } = new Bindable<UserActivity>();

        public bool IsLoggedIn => true;

        public string ProvidedUsername => LocalUser.Value.Username;

        public string Endpoint => "http://localhost";

        private APIState state = APIState.Online;

        private readonly List<IOnlineComponent> components = new List<IOnlineComponent>();

        public APIState State
        {
            get => state;
            private set
            {
                if (state == value)
                    return;

                state = value;

                Scheduler.Add(() => components.ForEach(c => c.APIStateChanged(this, value)));
            }
        }

        public DummyAPIAccess()
        {
            LocalUser.BindValueChanged(u =>
            {
                u.OldValue?.Activity.UnbindFrom(Activity);
                u.NewValue.Activity.BindTo(Activity);
            }, true);
        }

        public virtual void Queue(APIRequest request)
        {
        }

        public void Register(IOnlineComponent component)
        {
            Scheduler.Add(delegate { components.Add(component); });
            component.APIStateChanged(this, state);
        }

        public void Unregister(IOnlineComponent component)
        {
            Scheduler.Add(delegate { components.Remove(component); });
        }

        public void Login(string username, string password)
        {
            LocalUser.Value = new User
            {
                Username = username,
                Id = 1001,
            };

            State = APIState.Online;
        }

        public void Logout()
        {
            LocalUser.Value = new GuestUser();
            State = APIState.Offline;
        }

        public RegistrationRequest.RegistrationRequestErrors CreateAccount(string email, string username, string password)
        {
            Thread.Sleep(200);
            return null;
        }
    }
}
