// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.API
{
    public abstract class ArchiveDownloadRequest<TModel> : APIDownloadRequest
        where TModel : class
    {
        public readonly TModel Model;

        public double Progress { get; private set; }

        public event Action<double> DownloadProgressed;

        protected ArchiveDownloadRequest(TModel model)
        {
            Model = model;

            Progressed += (current, total) => SetProgress((double)current / total);
        }

        protected void SetProgress(double progress)
        {
            Progress = progress;
            DownloadProgressed?.Invoke(progress);
        }
    }
}
