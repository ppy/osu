// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;

namespace osu.Game.Online
{
    /// <summary>
    /// A component which tracks a beatmap through potential download/import/deletion.
    /// </summary>
    public abstract class DownloadTrackingComposite<TModel, TModelManager> : CompositeDrawable
        where TModel : class
        where TModelManager : class, IDownloadModelManager<TModel>
    {
        public readonly Bindable<TModel> ModelInfo = new Bindable<TModel>();

        private TModelManager manager;

        /// <summary>
        /// Holds the current download state of the beatmap, whether is has already been downloaded, is in progress, or is not downloaded.
        /// </summary>
        protected readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

        protected readonly Bindable<double> Progress = new Bindable<double>();

        protected DownloadTrackingComposite(TModel model = null)
        {
            ModelInfo.Value = model;
        }

        [BackgroundDependencyLoader(true)]
        private void load(TModelManager manager)
        {
            this.manager = manager;

            ModelInfo.BindValueChanged(modelInfo =>
            {
                if (modelInfo.NewValue == null)
                    attachDownload(null);
                else if (manager.IsAvailableLocally(modelInfo.NewValue))
                    State.Value = DownloadState.LocallyAvailable;
                else
                    attachDownload(manager.GetExistingDownload(modelInfo.NewValue));
            }, true);

            manager.DownloadBegan += download =>
            {
                if (download.Info.Equals(ModelInfo.Value))
                    attachDownload(download);
            };

            manager.ItemAdded += itemAdded;
            manager.ItemRemoved += itemRemoved;
        }

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (manager != null)
            {
                manager.DownloadBegan -= attachDownload;
                manager.ItemAdded -= itemAdded;
            }

            State.UnbindAll();

            attachDownload(null);
        }

        #endregion

        private ArchiveDownloadModelRequest<TModel> attachedRequest;

        private void attachDownload(ArchiveDownloadModelRequest<TModel> request)
        {
            if (attachedRequest != null)
            {
                attachedRequest.Failure -= onRequestFailure;
                attachedRequest.DownloadProgressed -= onRequestProgress;
                attachedRequest.Success -= onRequestSuccess;
            }

            attachedRequest = request;

            if (attachedRequest != null)
            {
                if (attachedRequest.Progress == 1)
                {
                    State.Value = DownloadState.Downloaded;
                    Progress.Value = 1;
                }
                else
                {
                    State.Value = DownloadState.Downloading;
                    Progress.Value = attachedRequest.Progress;

                    attachedRequest.Failure += onRequestFailure;
                    attachedRequest.DownloadProgressed += onRequestProgress;
                    attachedRequest.Success += onRequestSuccess;
                }
            }
            else
            {
                State.Value = DownloadState.NotDownloaded;
            }
        }

        private void onRequestSuccess(string _) => Schedule(() => State.Value = DownloadState.Downloaded);

        private void onRequestProgress(float progress) => Schedule(() => Progress.Value = progress);

        private void onRequestFailure(Exception e) => Schedule(() => attachDownload(null));

        private void itemAdded(TModel s, bool existing) => setDownloadStateFromManager(s, DownloadState.LocallyAvailable);

        private void itemRemoved(TModel s) => setDownloadStateFromManager(s, DownloadState.NotDownloaded);

        private void setDownloadStateFromManager(TModel s, DownloadState state) => Schedule(() =>
        {
            if (s.Equals(ModelInfo.Value))
                return;

            State.Value = state;
        });
    }
}
