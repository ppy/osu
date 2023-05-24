// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;

namespace osu.Game.Overlays
{
    /// <summary>
    /// A subview containing online content, to be displayed inside a <see cref="FullscreenOverlay{T}"/>.
    /// </summary>
    /// <remarks>
    /// Automatically performs a data fetch on load.
    /// </remarks>
    /// <typeparam name="T">The type of the API response.</typeparam>
    public abstract partial class OverlayView<T> : CompositeDrawable
        where T : class
    {
        [Resolved]
        protected IAPIProvider API { get; private set; }

        private APIRequest<T> request;

        protected OverlayView()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [BackgroundDependencyLoader]
        private void load()
        {
            apiState.BindTo(API.State);
            apiState.BindValueChanged(onlineStateChanged, true);
        }

        /// <summary>
        /// Create the API request for fetching data.
        /// </summary>
        protected abstract APIRequest<T> CreateRequest();

        /// <summary>
        /// Fired when results arrive from the main API request.
        /// </summary>
        /// <param name="response"></param>
        protected abstract void OnSuccess(T response);

        /// <summary>
        /// Force a re-request for data from the API.
        /// </summary>
        protected void PerformFetch()
        {
            request?.Cancel();

            request = CreateRequest();
            request.Success += response => Schedule(() => OnSuccess(response));

            API.Queue(request);
        }

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            switch (state.NewValue)
            {
                case APIState.Online:
                    PerformFetch();
                    break;
            }
        });

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
