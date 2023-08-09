// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Online.API;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update checker that fetches the latest release from the specified URL
    /// </summary>
    public abstract class GitHubUpdateChecker : IUpdateChecker
    {
        protected abstract string GitHubUrl { get; }

        public async Task<UpdateInfo?> CheckForUpdates()
        {
            var latest = await getLatestRelease().ConfigureAwait(false);

            if (latest == null)
                return null;

            return CreateUpdateInfo(latest);
        }

        protected abstract UpdateInfo? CreateUpdateInfo(GitHubRelease release);

        private async Task<GitHubRelease?> getLatestRelease()
        {
            try
            {
                var releases = new OsuJsonWebRequest<GitHubRelease>(GitHubUrl);

                await releases.PerformAsync().ConfigureAwait(false);

                return releases.ResponseObject;
            }
            catch
            {
                // we shouldn't crash on a web failure. or any failure for the matter.
                return null;
            }
        }
    }
}
