using System;
using osu.Framework.Bindables;
using osu.Game.Online.API;

namespace osu.Game.Database.Sayo
{
    public interface ISayoModelDownloader<TModel>
        where TModel : class
    {
        IBindable<WeakReference<ArchiveDownloadRequest<TModel>>> SayoDownloadBegan { get; }

        IBindable<WeakReference<ArchiveDownloadRequest<TModel>>> SayoDownloadFailed { get; }

        /// <summary>
        /// 从sayobot下载请求的 <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="model">目标 <typeparamref name="TModel"/></param>
        /// <param name="noVideo">不带视频</param>
        /// <param name="mini">Mini</param>
        /// <returns>下载是否开始</returns>
        /// <remarks>noVideo和mini不能同时为true</remarks>
        bool SayoDownload(TModel model, bool noVideo, bool mini);
    }
}
