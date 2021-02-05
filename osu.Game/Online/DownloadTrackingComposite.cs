// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
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
        protected TModelManager Manager { get; private set; }

        /// <summary>
        /// Holds the current download state of the <typeparamref name="TModel"/>, whether is has already been downloaded, is in progress, or is not downloaded.
        /// </summary>
        protected readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

        protected readonly BindableNumber<double> Progress = new BindableNumber<double> { MinValue = 0, MaxValue = 1 };

        protected DownloadTrackingComposite(TModel model = null)
        {
            Model.Value = model;
        }

        private IBindable<WeakReference<TModel>> managedUpdated;
        private IBindable<WeakReference<TModel>> managerRemoved;
        private IBindable<WeakReference<ArchiveDownloadRequest<TModel>>> managerDownloadBegan;
        private IBindable<WeakReference<ArchiveDownloadRequest<TModel>>> managerDownloadFailed;

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            Model.BindValueChanged(modelInfo =>
            {
                if (modelInfo.NewValue == null)
                    attachDownload(null);
                else if (IsModelAvailableLocally())
                    State.Value = DownloadState.LocallyAvailable;
                else
                    attachDownload(Manager?.GetExistingDownload(modelInfo.NewValue));
            }, true);

            if (Manager == null)
                return;

            managerDownloadBegan = Manager.DownloadBegan.GetBoundCopy();
            managerDownloadBegan.BindValueChanged(downloadBegan);
            managerDownloadFailed = Manager.DownloadFailed.GetBoundCopy();
            managerDownloadFailed.BindValueChanged(downloadFailed);
            managedUpdated = Manager.ItemUpdated.GetBoundCopy();
            managedUpdated.BindValueChanged(itemUpdated);
            managerRemoved = Manager.ItemRemoved.GetBoundCopy();
            managerRemoved.BindValueChanged(itemRemoved);
        }

        /// <summary>
        /// Checks that a database model matches the one expected to be downloaded.
        /// </summary>
        /// <example>
        /// For online play, this could be used to check that the databased model matches the online beatmap.
        /// </example>
        /// <param name="databasedModel">The model in database.</param>
        protected virtual bool VerifyDatabasedModel([NotNull] TModel databasedModel) => true;

        /// <summary>
        /// Whether the given model is available in the database.
        /// By default, this calls <see cref="IModelDownloader{TModel}.IsAvailableLocally"/>,
        /// but can be overriden to add additional checks for verifying the model in database.
        /// </summary>
        protected virtual bool IsModelAvailableLocally() => Manager?.IsAvailableLocally(Model.Value) == true;

        private void downloadBegan(ValueChangedEvent<WeakReference<ArchiveDownloadRequest<TModel>>> weakRequest)
        {
            if (weakRequest.NewValue.TryGetTarget(out var request))
            {
                Schedule(() =>
                {
                    if (request.Model.Equals(Model.Value))
                        attachDownload(request);
                });
            }
        }

        private void downloadFailed(ValueChangedEvent<WeakReference<ArchiveDownloadRequest<TModel>>> weakRequest)
        {
            if (weakRequest.NewValue.TryGetTarget(out var request))
            {
                Schedule(() =>
                {
                    if (request.Model.Equals(Model.Value))
                        attachDownload(null);
                });
            }
        }

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
                    State.Value = DownloadState.Importing;
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

        private void onRequestSuccess(string _) => Schedule(() => State.Value = DownloadState.Importing);

        private void onRequestProgress(float progress) => Schedule(() => Progress.Value = progress);

        private void onRequestFailure(Exception e) => Schedule(() => attachDownload(null));

        private void itemUpdated(ValueChangedEvent<WeakReference<TModel>> weakItem)
        {
            if (weakItem.NewValue.TryGetTarget(out var item))
            {
                Schedule(() =>
                {
                    if (!item.Equals(Model.Value))
                        return;

                    if (!VerifyDatabasedModel(item))
                    {
                        State.Value = DownloadState.NotDownloaded;
                        return;
                    }

                    State.Value = DownloadState.LocallyAvailable;
                });
            }
        }

        private void itemRemoved(ValueChangedEvent<WeakReference<TModel>> weakItem)
        {
            if (weakItem.NewValue.TryGetTarget(out var item))
            {
                Schedule(() =>
                {
                    if (item.Equals(Model.Value))
                        State.Value = DownloadState.NotDownloaded;
                });
            }
        }

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            State.UnbindAll();

            attachDownload(null);
        }

        #endregion
    }
}
