// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.API
{
    public abstract class ArchiveDownloadModelRequest<TModel> : APIDownloadRequest
        where TModel : class
    {
        public readonly TModel Info;

        public float Progress;

        public event Action<float> DownloadProgressed;

        protected ArchiveDownloadModelRequest(TModel model)
        {
            Info = model;

            Progressed += (current, total) => DownloadProgressed?.Invoke(Progress = (float)current / total);
        }
    }
}
