// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Game.Database;
using Realms;

namespace osu.Game.Models
{
    /// <summary>
    /// Describes an online asset (background image, user avatar, cover...) which is persisted to disk
    /// to reduce the number of online requests and shorten retrieval time.
    /// </summary>
    public class RealmOnlineAsset : RealmObject
    {
        /// <summary>
        /// Contains information about the original URL of the file and its location on disk.
        /// </summary>
        public RealmNamedFileUsage File { get; set; } = null!;

        /// <summary>
        /// Contains the last time of access of this asset.
        /// </summary>
        /// <remarks>
        /// Assets that have not been accessed for over a month are purged
        /// (<see cref="RealmAccess.cleanupPendingDeletions"/>).
        /// </remarks>
        public DateTimeOffset LastAccessed { get; set; } = DateTimeOffset.Now;

        [UsedImplicitly]
        private RealmOnlineAsset()
        {
        }

        public RealmOnlineAsset(RealmFile localFile, string remoteUrl)
        {
            File = new RealmNamedFileUsage(localFile, remoteUrl);
        }
    }
}
