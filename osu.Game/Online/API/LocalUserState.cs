// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    public partial class LocalUserState : Component, ILocalUserState
    {
        public IBindable<APIUser> User => localUser;
        public IBindableList<APIRelation> Friends => friends;
        public IBindableList<APIRelation> Blocks => blocks;
        public IBindableList<int> FavouriteBeatmapSets => favouriteBeatmapSets;

        private readonly IAPIProvider api;

        private readonly Bindable<APIUser> localUser = new Bindable<APIUser>(createGuestUser());
        private readonly BindableList<APIRelation> friends = new BindableList<APIRelation>();
        private readonly BindableList<APIRelation> blocks = new BindableList<APIRelation>();
        private readonly BindableList<int> favouriteBeatmapSets = new BindableList<int>();

        private readonly Bindable<UserStatus> configStatus = new Bindable<UserStatus>();
        private readonly Bindable<bool> configSupporter = new Bindable<bool>();

        public LocalUserState(IAPIProvider api, OsuConfigManager config)
        {
            this.api = api;

            config.BindWith(OsuSetting.UserOnlineStatus, configStatus);
            config.BindWith(OsuSetting.WasSupporter, configSupporter);
        }

        #region Logging in / out

        private static APIUser createGuestUser() => new GuestUser();

        /// <summary>
        /// Show a placeholder user if saved credentials are available.
        /// This is useful for storing local scores and showing a placeholder username after starting the game,
        /// until a valid connection has been established.
        /// </summary>
        public void SetPlaceholderLocalUser(string username)
        {
            if (!localUser.IsDefault)
                return;

            localUser.Value = new APIUser
            {
                Username = username,
                IsSupporter = configSupporter.Value,
            };
        }

        public void SetLocalUser(APIMe me)
        {
            localUser.Value = me;
            configSupporter.Value = me.IsSupporter;

            UpdateFriends();
            UpdateBlocks();
            UpdateFavouriteBeatmapSets();
        }

        public void ClearLocalUser()
        {
            // Reset the status to be broadcast on the next login, in case multiple players share the same system.
            configStatus.Value = UserStatus.Online;

            // Scheduled prior to state change such that the state changed event is invoked with the correct user and their friends present
            Schedule(() =>
            {
                localUser.Value = createGuestUser();
                configSupporter.Value = false;
                friends.Clear();
                blocks.Clear();
                favouriteBeatmapSets.Clear();
            });
        }

        #endregion

        public void UpdateFriends()
        {
            if (!api.IsLoggedIn)
                return;

            var friendsReq = new GetFriendsRequest();
            friendsReq.Success += res =>
            {
                var existingFriends = friends.Select(f => f.TargetID).ToHashSet();
                var updatedFriends = res.Select(f => f.TargetID).ToHashSet();

                // Add new friends into local list.
                friends.AddRange(res.Where(r => !existingFriends.Contains(r.TargetID)));

                // Remove non-friends from local list.
                friends.RemoveAll(f => !updatedFriends.Contains(f.TargetID));
            };

            api.Queue(friendsReq);
        }

        public void UpdateBlocks()
        {
            if (!api.IsLoggedIn)
                return;

            var blocksReq = new GetBlocksRequest();
            blocksReq.Success += res =>
            {
                var existingBlocks = blocks.Select(f => f.TargetID).ToHashSet();
                var updatedBlocks = res.Select(f => f.TargetID).ToHashSet();

                // Add new blocked users to local list.
                blocks.AddRange(res.Where(r => !existingBlocks.Contains(r.TargetID)));

                // Remove non-blocked users from local list.
                blocks.RemoveAll(b => !updatedBlocks.Contains(b.TargetID));

                // Remove friends who got blocked since last check.
                friends.RemoveAll(f => updatedBlocks.Contains(f.TargetID));
            };

            api.Queue(blocksReq);
        }

        public void UpdateFavouriteBeatmapSets()
        {
            if (!api.IsLoggedIn)
                return;

            var favouritesReq = new GetMyFavouriteBeatmapSetsRequest();
            favouritesReq.Success += res =>
            {
                var existingBeatmapSets = favouriteBeatmapSets.ToHashSet();
                var updatedBeatmapSets = res.BeatmapSetIds.ToHashSet();

                favouriteBeatmapSets.AddRange(updatedBeatmapSets.Except(existingBeatmapSets));
                favouriteBeatmapSets.RemoveAll(b => !updatedBeatmapSets.Contains(b));
            };

            api.Queue(favouritesReq);
        }
    }
}
