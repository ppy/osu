using System;
using System.Collections.Generic;
using System.Text;

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
