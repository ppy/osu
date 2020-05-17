// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Online.API;

namespace osu.Game.Online
{
    /// <summary>
    /// A component which tracks a <typeparamref name="TModel"/> through potential download/import/deletion.
    /// </summary>
    public abstract class DownloadTrackingComposite<TModel, TModelManager> : CompositeDrawable
        where TModel : class, IEquatable<TModel>
        where TModelManager : class, IModelDownloader<TModel>
    {
        protected readonly Bindable<TModel> Model = new Bindable<TModel>();

        [Resolved(CanBeNull = true)]
        private TModelManager manager { get; set; }

        /// <summary>
        /// Holds the current download state of the <typeparamref name="TModel"/>, whether is has already been downloaded, is in progress, or is not downloaded.
        /// </summary>
        protected readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

        protected readonly BindableNumber<double> Progress = new BindableNumber<double> { MinValue = 0, MaxValue = 1 };

        protected DownloadTrackingComposite(TModel model = null)
        {
            Model.Value = model;
        }

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            Model.BindValueChanged(modelInfo =>
            {
                if (modelInfo.NewValue == null)
                    attachDownload(null);
                else if (manager.IsAvailableLocally(modelInfo.NewValue))
                    State.Value = DownloadState.LocallyAvailable;
                else
                    attachDownload(manager.GetExistingDownload(modelInfo.NewValue));
            }, true);

            manager.DownloadBegan += downloadBegan;
            manager.DownloadFailed += downloadFailed;
            manager.ItemAdded += itemAdded;
            manager.ItemRemoved += itemRemoved;
        }

        private void downloadBegan(ArchiveDownloadRequest<TModel> request) => Schedule(() =>
        {
            if (request.Model.Equals(Model.Value))
                attachDownload(request);
        });

        private void downloadFailed(ArchiveDownloadRequest<TModel> request) => Schedule(() =>
        {
            if (request.Model.Equals(Model.Value))
                attachDownload(null);
        });

        private ArchiveDownloadRequest<TModel> attachedRequest;

        private void attachDownload(ArchiveDownloadRequest<TModel> request)
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

        private void itemAdded(TModel s) => setDownloadStateFromManager(s, DownloadState.LocallyAvailable);

        private void itemRemoved(TModel s) => setDownloadStateFromManager(s, DownloadState.NotDownloaded);

        private void setDownloadStateFromManager(TModel s, DownloadState state) => Schedule(() =>
        {
            if (!s.Equals(Model.Value))
                return;

            State.Value = state;
        });

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (manager != null)
            {
                manager.DownloadBegan -= downloadBegan;
                manager.DownloadFailed -= downloadFailed;
                manager.ItemAdded -= itemAdded;
                manager.ItemRemoved -= itemRemoved;
            }

            State.UnbindAll();

            attachDownload(null);
        }

        #endregion
    }
}
