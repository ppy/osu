using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Notifications;
using osu.Game.Tests.Visual;
using osu.Game.Users;

#nullable disable

namespace osu.Game.Tournament.Screens.Game
{
    public partial class GameScreen : TournamentScreen
    {
        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        private OsuGameTestScene.TestOsuGame nestedGame;

        public override void Show()
        {
            this.FadeIn();

            nestedGame = new OsuGameTestScene.TestOsuGame(host.Storage, new ForwardingAPIAccess(api))
            {
                Masking = true
            };

            nestedGame.SetHost(host);

            AddInternal(nestedGame);
        }

        public override void Hide()
        {
            if (nestedGame != null) RemoveInternal(nestedGame, true);

            base.Hide();
        }
    }

    public partial class ForwardingAPIAccess : IAPIProvider
    {
        private readonly IAPIProvider api;

        public ForwardingAPIAccess(IAPIProvider api)
        {
            this.api = api;
        }

        public IBindable<APIUser> LocalUser => api.LocalUser;
        public IBindableList<APIUser> Friends => api.Friends;
        public IBindable<UserActivity> Activity => api.Activity;
        public string AccessToken => api.AccessToken;
        public bool IsLoggedIn => api.IsLoggedIn;
        public string ProvidedUsername => api.ProvidedUsername;
        public string APIEndpointUrl => api.APIEndpointUrl;
        public string WebsiteRootUrl => api.WebsiteRootUrl;
        public int APIVersion => api.APIVersion;
        public Exception LastLoginError => api.LastLoginError;
        public IBindable<APIState> State => api.State;

        public void Queue(APIRequest request)
        {
            api.Queue(request);
        }

        public void Perform(APIRequest request)
        {
            api.Perform(request);
        }

        public Task PerformAsync(APIRequest request)
        {
            return api.PerformAsync(request);
        }

        public void Login(string username, string password)
        {
            api.Login(username, password);
        }

        public void Logout()
        {
            api.Logout();
        }

        public IHubClientConnector GetHubConnector(string clientName, string endpoint, bool preferMessagePack = true)
        {
            return api.GetHubConnector(clientName, endpoint, preferMessagePack);
        }

        public NotificationsClientConnector GetNotificationsConnector()
        {
            return api.GetNotificationsConnector();
        }

        public RegistrationRequest.RegistrationRequestErrors CreateAccount(string email, string username, string password)
        {
            return api.CreateAccount(email, username, password);
        }
    }
}
