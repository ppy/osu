// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;

#nullable enable

namespace osu.Game.Online
{
    public abstract class DownloadTracker<T> : Component
        where T : class
    {
        public readonly T TrackedItem;

        /// <summary>
        /// Holds the current download state of the download - whether is has already been downloaded, is in progress, or is not downloaded.
        /// </summary>
        public IBindable<DownloadState> State => state;

        private readonly Bindable<DownloadState> state = new Bindable<DownloadState>();

        /// <summary>
        /// The progress of an active download.
        /// </summary>
        public IBindableNumber<double> Progress => progress;

        private readonly BindableNumber<double> progress = new BindableNumber<double> { MinValue = 0, MaxValue = 1 };

        protected DownloadTracker(T trackedItem)
        {
            TrackedItem = trackedItem;
        }

        protected void UpdateState(DownloadState newState) => state.Value = newState;

        protected void UpdateProgress(double newProgress) => progress.Value = newProgress;
    }
}
