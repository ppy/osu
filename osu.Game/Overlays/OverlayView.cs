// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Placeholders;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    /// <summary>
    /// A subview containing online content, to be displayed inside a <see cref="FullscreenOverlay"/>.
    /// </summary>
    /// <remarks>
    /// Automatically performs a data fetch on load.
    /// </remarks>
    /// <typeparam name="T">The type of the API response.</typeparam>
    public abstract class OverlayView<T> : Container, IOnlineComponent
        where T : class
    {
        [Resolved]
        protected IAPIProvider API { get; private set; }

        protected LoadingSpinner LoadingSpinner { get; private set; }

        protected override Container<Drawable> Content { get; } = new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
        };

        private Placeholder currentPlaceholder;

        private BlockingBox blockingBox;

        private APIRequest<T> request;

        protected OverlayView()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                Content,
                blockingBox = new BlockingBox
                {
                    RelativeSizeAxes = Axes.Both,
                },
                currentPlaceholder = new LoginPlaceholder("Please login to view content!")
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
                LoadingSpinner = new LoadingSpinner
                {
                    Alpha = 0,
                    Size = new osuTK.Vector2(40),
                    Margin = new MarginPadding { Top = 25 }
                }
}           ;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            API.Register(this);
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
            request.Success += onSuccess;

            LoadingSpinner.Show();

            API.Queue(request);
        }

        public virtual void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                case APIState.Offline:
                    blockingBox.FadeIn(300, Easing.OutQuint);
                    currentPlaceholder.ScaleTo(0.8f).Then().ScaleTo(1, 600, Easing.OutQuint);
                    currentPlaceholder.FadeInFromZero(2 * 300, Easing.OutQuint);
                    LoadingSpinner.Hide();
                    break;

                case APIState.Online:
                    blockingBox.FadeOut(300, Easing.OutQuint);
                    currentPlaceholder.FadeOut(300, Easing.OutQuint);
                    LoadingSpinner.Hide();
                    PerformFetch();
                    break;

                case APIState.Failing:
                case APIState.Connecting:
                    LoadingSpinner.Show();
                    blockingBox.FadeIn(300, Easing.OutQuint);
                    currentPlaceholder.FadeOut(300, Easing.OutQuint);
                    break;
            }
        }

        private void onSuccess(T content)
        {
            LoadingSpinner.Hide();
            Schedule(() => OnSuccess(content));
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            API?.Unregister(this);
            base.Dispose(isDisposing);
        }

        private class BlockingBox : Box
        {
            public BlockingBox()
            {
                Colour = Color4.Black.Opacity(0.7f);
            }

            public override bool HandleNonPositionalInput => false;

            protected override bool Handle(UIEvent e)
            {
                switch (e)
                {
                    case ScrollEvent _:
                        return false;
                }

                return true;
            }
        }
    }
}
