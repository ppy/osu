// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.RegularExpressions;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users.Drawables
{
    public class AvatarStore
    {
        private readonly UserLookupCache userCache;
        private readonly OnlineStore onlineStore;
        private readonly Storage avatarStorage;
        private readonly LargeTextureStore largeTextureStore;

        private static readonly Regex avatar_url_regex =
            new Regex(@"^https:\/\/a\.ppy\.sh\/(?<userId>\d+)\?(?<filename>\d+\.(gif|jpg|jpeg|png))$", RegexOptions.Compiled);

        private static readonly Regex custom_cover_url_regex =
            new Regex(@"^https:\/\/assets\.ppy\.sh\/user-profile-covers\/(?<id>\d+)\/(?<filename>[0-9a-fA-F]+\.(gif|jpg|jpeg|png))$", RegexOptions.Compiled);

        private static readonly Regex preset_cover_url_regex =
            new Regex(@"^https:\/\/assets\.ppy\.sh\/user-cover-presets\/(?<id>\d+)/(?<filename>[0-9a-fA-F]+\.(gif|jpg|jpeg|png))$", RegexOptions.Compiled);

        private static readonly Regex team_flag_url_regex =
            new Regex(@"^https:\/\/assets\.ppy\.sh\/teams\/flag\/(?<teamId>\d+)/(?<filename>[0-9a-fA-F]+\.(gif|jpg|jpeg|png))$", RegexOptions.Compiled);

        private const string user_avatar_storage = @"user-avatars";
        private const string custom_cover_storage = @"custom-covers";
        private const string preset_cover_storage = @"preset-covers";
        private const string team_flag_storage = @"team-flags";

        public AvatarStore(GameHost host, UserLookupCache userCache)
        {
            this.userCache = userCache;
            onlineStore = new TrustedDomainOnlineStore();
            avatarStorage = host.CacheStorage.GetStorageForDirectory(@"avatars");

            largeTextureStore = new LargeTextureStore(host.Renderer, host.CreateTextureLoaderStore(new StorageBackedResourceStore(avatarStorage)));
            largeTextureStore.AddTextureSource(host.CreateTextureLoaderStore(onlineStore));
        }

        public Texture? GetUserAvatar(int userId)
        {
            var user = userCache.GetUserAsync(userId).GetResultSafely();
            return user == null ? null : GetUserAvatar(user);
        }

        public Texture? GetUserAvatar(APIUser user)
        {
            try
            {
                string avatarUrl = user.AvatarUrl;
                if (string.IsNullOrEmpty(avatarUrl))
                    return null;

                var match = avatar_url_regex.Match(avatarUrl);

                if (!match.Success)
                {
                    Logger.Log($@"Unrecognised user avatar URL format ({avatarUrl}), serving online version.", LoggingTarget.Network);
                    return largeTextureStore.Get(avatarUrl);
                }

                string userId = match.Groups[@"userId"].Value;
                string filename = match.Groups[@"filename"].Value;
                ensureDownloadedToLocalCache(avatarStorage.GetStorageForDirectory(user_avatar_storage).GetStorageForDirectory(userId), filename, avatarUrl);

                string lookupKey = $@"{user_avatar_storage}/{userId}/{filename}";
                Logger.Log($@"Serving user avatar from local cache ({lookupKey}).", LoggingTarget.Network);
                return largeTextureStore.Get(lookupKey);
            }
            catch (Exception e)
            {
                Logger.Log($@"Error when retrieving user avatar: {e}", LoggingTarget.Network);
                return null;
            }
        }

        public Texture? GetUserCover(APIUser user)
        {
            try
            {
                string? coverUrl = user.CoverUrl;
                if (string.IsNullOrEmpty(coverUrl))
                    return null;

                string? coverStorageName = null;
                Match? match = null;

                if (custom_cover_url_regex.IsMatch(coverUrl))
                {
                    coverStorageName = custom_cover_storage;
                    match = custom_cover_url_regex.Match(coverUrl);
                }

                if (preset_cover_url_regex.IsMatch(coverUrl))
                {
                    coverStorageName = preset_cover_storage;
                    match = preset_cover_url_regex.Match(coverUrl);
                }

                if (match?.Success != true)
                {
                    Logger.Log(@$"Unrecognised user cover URL format ({coverUrl}), serving online version.", LoggingTarget.Network);
                    return largeTextureStore.Get(coverUrl);
                }

                string presetOrUserId = match.Groups[@"id"].Value;
                string filename = match.Groups[@"filename"].Value;
                ensureDownloadedToLocalCache(avatarStorage.GetStorageForDirectory(coverStorageName).GetStorageForDirectory(presetOrUserId), filename, coverUrl);

                string lookupKey = $@"{coverStorageName}/{presetOrUserId}/{filename}";
                return largeTextureStore.Get(lookupKey);
            }
            catch (Exception e)
            {
                Logger.Log($@"Error when retrieving user cover: {e}", LoggingTarget.Network);
                return null;
            }
        }

        public Texture? GetTeamAvatar(APITeam team)
        {
            try
            {
                string flagUrl = team.FlagUrl;
                if (string.IsNullOrEmpty(flagUrl))
                    return null;

                var match = team_flag_url_regex.Match(flagUrl);

                if (!match.Success)
                {
                    Logger.Log($@"Unrecognised team avatar URL format ({flagUrl}), serving online version.", LoggingTarget.Network);
                    return largeTextureStore.Get(flagUrl);
                }

                string teamId = match.Groups[@"teamId"].Value;
                string filename = match.Groups[@"filename"].Value;
                ensureDownloadedToLocalCache(avatarStorage.GetStorageForDirectory(team_flag_storage).GetStorageForDirectory(teamId), filename, flagUrl);

                string lookupKey = $@"{team_flag_storage}/{teamId}/{filename}";
                Logger.Log($@"Serving team avatar from local cache ({lookupKey}).", LoggingTarget.Network);
                return largeTextureStore.Get(lookupKey);
            }
            catch (Exception e)
            {
                Logger.Log($@"Error when retrieving team avatar: {e}", LoggingTarget.Network);
                return null;
            }
        }

        private void ensureDownloadedToLocalCache(Storage scopedStorage, string localKey, string remoteUrl)
        {
            if (scopedStorage.Exists(localKey))
                return;

            try
            {
                foreach (string file in scopedStorage.GetFiles(string.Empty))
                {
                    Logger.Log($@"Purging stale cached asset {scopedStorage.GetFullPath(file)}...", LoggingTarget.Network);
                    scopedStorage.Delete(file);
                }
            }
            catch (Exception e)
            {
                Logger.Log($@"Purging stale cached assets under {scopedStorage.GetFullPath(string.Empty)} failed: {e}", LoggingTarget.Network);
            }

            byte[] onlineResult = onlineStore.Get(remoteUrl);

            using (var stream = scopedStorage.CreateFileSafely(localKey))
                stream.Write(onlineResult, 0, onlineResult.Length);
        }
    }
}
