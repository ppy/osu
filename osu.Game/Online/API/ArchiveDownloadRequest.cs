// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.API
{
    public abstract class ArchiveDownloadRequest<TModel> : APIDownloadRequest
        where TModel : class
    {
        public readonly TModel Model;

        public float Progress { get; private set; }

        public event Action<float> DownloadProgressed;

        protected ArchiveDownloadRequest(TModel model)
        {
            Model = model;

            Progressed += (current, total) => SetProgress((float)current / total);
        }

        protected void SetProgress(float progress)
        {
            Progress = progress;
            DownloadProgressed?.Invoke(progress);
        }
    }
}
