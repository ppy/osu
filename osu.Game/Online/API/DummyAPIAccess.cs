// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Notifications;
using osu.Game.Tests;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    public partial class DummyAPIAccess : Component, IAPIProvider
    {
        public const int DUMMY_USER_ID = 1001;

        public Bindable<APIUser> LocalUser { get; } = new Bindable<APIUser>(new APIUser
        {
            Username = @"Local user",
            Id = DUMMY_USER_ID,
        });

        public BindableList<APIUser> Friends { get; } = new BindableList<APIUser>();

        public Bindable<UserActivity> Activity { get; } = new Bindable<UserActivity>();

        public Language Language => Language.en;

        public string AccessToken => "token";

        /// <seealso cref="APIAccess.IsLoggedIn"/>
        public bool IsLoggedIn => State.Value > APIState.Offline;

        public string ProvidedUsername => LocalUser.Value.Username;

        public string APIEndpointUrl => "http://localhost";

        public string WebsiteRootUrl => "http://localhost";

        public int APIVersion => int.Parse(DateTime.Now.ToString("yyyyMMdd"));

        public Exception? LastLoginError { get; private set; }

        /// <summary>
        /// Provide handling logic for an arbitrary API request.
        /// Should return true is a request was handled. If null or false return, the request will be failed with a <see cref="NotSupportedException"/>.
        /// </summary>
        public Func<APIRequest, bool>? HandleRequest;

        private readonly Bindable<APIState> state = new Bindable<APIState>(APIState.Online);

        private bool shouldFailNextLogin;
        private bool stayConnectingNextLogin;

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
            Schedule(() =>
            {
                if (HandleRequest?.Invoke(request) != true)
                {
                    request.Fail(new InvalidOperationException($@"{nameof(DummyAPIAccess)} cannot process this request."));
                }
            });
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

            if (stayConnectingNextLogin)
            {
                stayConnectingNextLogin = false;
                return;
            }

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
                Id = DUMMY_USER_ID,
            };

            state.Value = APIState.Online;
        }

        public void Logout()
        {
            state.Value = APIState.Offline;
            // must happen after `state.Value` is changed such that subscribers to that bindable's value changes see the correct user.
            // compare: `APIAccess.Logout()`.
            LocalUser.Value = new GuestUser();
        }

        public IHubClientConnector? GetHubConnector(string clientName, string endpoint, bool preferMessagePack) => null;

        public NotificationsClientConnector GetNotificationsConnector() => new PollingNotificationsClientConnector(this);

        public RegistrationRequest.RegistrationRequestErrors? CreateAccount(string email, string username, string password)
        {
            Thread.Sleep(200);
            return null;
        }

        public void SetState(APIState newState) => state.Value = newState;

        IBindable<APIUser> IAPIProvider.LocalUser => LocalUser;
        IBindableList<APIUser> IAPIProvider.Friends => Friends;
        IBindable<UserActivity> IAPIProvider.Activity => Activity;

        /// <summary>
        /// During the next simulated login, the process will fail immediately.
        /// </summary>
        public void FailNextLogin() => shouldFailNextLogin = true;

        /// <summary>
        /// During the next simulated login, the process will pause indefinitely at "connecting".
        /// </summary>
        public void PauseOnConnectingNextLogin() => stayConnectingNextLogin = true;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            // Ensure (as much as we can) that any pending tasks are run.
            Scheduler.Update();
        }
    }
}
