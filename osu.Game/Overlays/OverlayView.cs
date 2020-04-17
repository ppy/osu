// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;

namespace osu.Game.Overlays
{
    /// <summary>
    /// Drawable which used to represent online content in <see cref="FullscreenOverlay"/>.
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    public abstract class OverlayView<T> : Container, IOnlineComponent
        where T : class
    {
        [Resolved]
        protected IAPIProvider API { get; private set; }

        protected override Container<Drawable> Content => content;

        private readonly FillFlowContainer content;

        protected OverlayView()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddInternal(content = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            API.Register(this);
        }

        private APIRequest<T> request;

        protected abstract APIRequest<T> CreateRequest();

        protected abstract void OnSuccess(T response);

        public virtual void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                case APIState.Online:
                    request = CreateRequest();
                    request.Success += response => Schedule(() => OnSuccess(response));
                    api.Queue(request);
                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            API?.Unregister(this);
            base.Dispose(isDisposing);
        }
    }
}