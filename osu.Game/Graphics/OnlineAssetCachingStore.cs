// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Online;

namespace osu.Game.Graphics
{
    /// <summary>
    /// <para>
    /// Store for retrieval and caching of assets (background, avatars, covers) retrieved from the web to disk.
    /// </para>
    /// <para>
    /// This store assumes relies on the uniqueness of the URL of retrieved assets to determine identity.
    /// Therefore, this store <b>MUST</b> only be used with URLs that are content-addressed in some way
    /// (by containing a content-based hash in the filename, or a cache-busting query string based on time of last update).
    /// </para>
    /// <para>
    /// This store <b>MUST NOT</b> be used with URLs containing naive cache-busting strings (e.g. <c>test.jpg?TIMESTAMP</c>)
    /// as it both makes the caching ineffective <b>AND</b> trashes the cache with entries that will never be used again.
    /// </para>
    /// </summary>
    public class OnlineAssetCachingStore : IDisposable
    {
        // ReSharper disable NotAccessedField.Local
        private readonly RealmAccess realmAccess;
        private readonly OnlineStore onlineStore;
        private readonly RealmFileStore fileStore;
        private readonly LargeTextureStore largeTextureStore;

        public OnlineAssetCachingStore(GameHost host, RealmAccess realmAccess)
        {
            this.realmAccess = realmAccess;
            onlineStore = new TrustedDomainOnlineStore();
            fileStore = new RealmFileStore(realmAccess, host.Storage);
            // largeTextureStore = new LargeTextureStore(host.Renderer, host.CreateTextureLoaderStore(new StorageBackedResourceStore(fileStore.Storage)));
            largeTextureStore = new LargeTextureStore(host.Renderer, host.CreateTextureLoaderStore(onlineStore));
        }

        public Texture? Get(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            return largeTextureStore.Get(url);

            // TODO: logic below temporarily disabled as it causes unacceptable performance on devices with slow I/O due to realm abuse
            // see https://github.com/ppy/osu/issues/38651#issuecomment-5356443643 for details

            // string? path = realmAccess.Write(r =>
            // {
            //     var a = r.All<RealmOnlineAsset>().Filter($@"{nameof(RealmOnlineAsset.File)}.{nameof(RealmNamedFileUsage.Filename)} == $0", url).FirstOrDefault();
            //     if (a != null)
            //         a.LastAccessed = DateTimeOffset.Now;
            //     return a?.File.File.GetStoragePath();
            // });

            // if (path == null)
            // {
            //     var onlineStream = onlineStore.GetStream(url);

            //     if (onlineStream == null)
            //         return null;

            //     path = realmAccess.Write(r =>
            //     {
            //         var file = fileStore.Add(onlineStream, r);
            //         r.Add(new RealmOnlineAsset(file, url));
            //         return file.GetStoragePath();
            //     });
            // }
            // else
            // {
            //     Logger.Log($"Online asset {url} retrieved from {nameof(OnlineAssetCachingStore)}.", LoggingTarget.Network);
            // }

            // return largeTextureStore.Get(path);
        }

        public void Dispose()
        {
            onlineStore.Dispose();
            largeTextureStore.Dispose();
        }
    }
}
