// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Placeholders;

namespace osu.Game.Online
{
    /// <summary>
    /// A <see cref="Container"/> for dislaying online content who require a local user to be logged in.
    /// Shows its children only when the local user is logged in and supports displaying a placeholder if not.
    /// </summary>
    public abstract class OnlineViewContainer : Container, IOnlineComponent
    {
        private readonly Container placeholderContainer;
        private readonly Placeholder placeholder;
        protected readonly LoadingAnimation LoadingAnimation;

        private const int transform_time = 300;

        protected override Container<Drawable> Content { get; }

        [Resolved]
        protected IAPIProvider API { get; private set; }

        public OnlineViewContainer(string placeholderMessage)
        {
            InternalChildren = new Drawable[]
            {
                Content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                placeholderContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Child = placeholder = new LoginPlaceholder(placeholderMessage)
                },
                LoadingAnimation = new LoadingAnimation
                {
                    Alpha = 0,
                }
            };
        }

        public virtual void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                case APIState.Offline:
                    Content.FadeOut(150, Easing.OutQuint);
                    placeholder.ScaleTo(0.8f).Then().ScaleTo(1, 3 * transform_time, Easing.OutQuint);
                    placeholderContainer.FadeInFromZero(2 * transform_time, Easing.OutQuint);
                    LoadingAnimation.Hide();
                    break;

                case APIState.Online:
                    placeholderContainer.FadeOut(150, Easing.OutQuint);
                    Content.FadeIn(transform_time, Easing.OutQuint);
                    LoadingAnimation.Hide();
                    break;

                case APIState.Failing:
                case APIState.Connecting:
                    LoadingAnimation.Show();
                    placeholderContainer.FadeOut(150, Easing.OutQuint);
                    Content.FadeOut(150, Easing.OutQuint);
                    break;
            }
        }

        protected override void LoadComplete()
        {
            API?.Register(this);
            base.LoadComplete();
        }

        protected override void Dispose(bool isDisposing)
        {
            API?.Unregister(this);
            base.Dispose(isDisposing);
        }
    }
}
