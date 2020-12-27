// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
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

        public BindableList<User> Friends { get; } = new BindableList<User>();

        public Bindable<UserActivity> Activity { get; } = new Bindable<UserActivity>();

        public string AccessToken => "token";

        public bool IsLoggedIn => State.Value == APIState.Online;

        public string ProvidedUsername => LocalUser.Value.Username;

        public string APIEndpointUrl => "http://localhost";

        public string WebsiteRootUrl => "http://localhost";

        /// <summary>
        /// Provide handling logic for an arbitrary API request.
        /// </summary>
        public Action<APIRequest> HandleRequest;

        private readonly Bindable<APIState> state = new Bindable<APIState>(APIState.Online);

        /// <summary>
        /// The current connectivity state of the API.
        /// </summary>
        public IBindable<APIState> State => state;

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
            HandleRequest?.Invoke(request);
        }

        public void Perform(APIRequest request) => HandleRequest?.Invoke(request);

        public Task PerformAsync(APIRequest request)
        {
            HandleRequest?.Invoke(request);
            return Task.CompletedTask;
        }

        public void Login(string username, string password)
        {
            LocalUser.Value = new User
            {
                Username = username,
                Id = 1001,
            };

            state.Value = APIState.Online;
        }

        public void Logout()
        {
            LocalUser.Value = new GuestUser();
            state.Value = APIState.Offline;
        }

        public RegistrationRequest.RegistrationRequestErrors CreateAccount(string email, string username, string password)
        {
            Thread.Sleep(200);
            return null;
        }

        public void SetState(APIState newState) => state.Value = newState;

        IBindable<User> IAPIProvider.LocalUser => LocalUser;
        IBindableList<User> IAPIProvider.Friends => Friends;
        IBindable<UserActivity> IAPIProvider.Activity => Activity;
    }
}
