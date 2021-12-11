// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    public class DummyAPIAccess : Component, IAPIProvider
    {
        public Bindable<APIUser> LocalUser { get; } = new Bindable<APIUser>(new APIUser
        {
            Username = @"Dummy",
            Id = 1001,
        });

        public BindableList<APIUser> Friends { get; } = new BindableList<APIUser>();

        public Bindable<UserActivity> Activity { get; } = new Bindable<UserActivity>();

        public string AccessToken => "token";

        public bool IsLoggedIn => State.Value == APIState.Online;

        public string ProvidedUsername => LocalUser.Value.Username;

        public string APIEndpointUrl => "http://localhost";

        public string WebsiteRootUrl => "http://localhost";

        public Exception LastLoginError { get; private set; }

        /// <summary>
        /// Provide handling logic for an arbitrary API request.
        /// Should return true is a request was handled. If null or false return, the request will be failed with a <see cref="NotSupportedException"/>.
        /// </summary>
        public Func<APIRequest, bool> HandleRequest;

        private readonly Bindable<APIState> state = new Bindable<APIState>(APIState.Online);

        private bool shouldFailNextLogin;

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
            if (HandleRequest?.Invoke(request) != true)
            {
                // this will fail due to not receiving an APIAccess, and trigger a failure on the request.
                // this is intended - any request in testing that needs non-failures should use HandleRequest.
                request.Perform(this);
            }
        }

        public void Perform(APIRequest request) => HandleRequest?.Invoke(request);

        public Task PerformAsync(APIRequest request)
        {
            HandleRequest?.Invoke(request);
            return Task.CompletedTask;
        }

        public void Login(string username, string password)
        {
            state.Value = APIState.Connecting;

            if (shouldFailNextLogin)
            {
                LastLoginError = new APIException("Not powerful enough to login.", new ArgumentException(nameof(shouldFailNextLogin)));

                state.Value = APIState.Offline;
                shouldFailNextLogin = false;
                return;
            }

            LastLoginError = null;
            LocalUser.Value = new APIUser
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

        public IHubClientConnector GetHubConnector(string clientName, string endpoint, bool preferMessagePack) => null;

        public RegistrationRequest.RegistrationRequestErrors CreateAccount(string email, string username, string password)
        {
            Thread.Sleep(200);
            return null;
        }

        public void SetState(APIState newState) => state.Value = newState;

        IBindable<APIUser> IAPIProvider.LocalUser => LocalUser;
        IBindableList<APIUser> IAPIProvider.Friends => Friends;
        IBindable<UserActivity> IAPIProvider.Activity => Activity;

        public void FailNextLogin() => shouldFailNextLogin = true;
    }
}
