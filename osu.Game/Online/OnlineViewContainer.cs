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
    /// A <see cref="Container"/> for displaying online content which require a local user to be logged in.
    /// Shows its children only when the local user is logged in and supports displaying a placeholder if not.
    /// </summary>
    public abstract class OnlineViewContainer : Container, IOnlineComponent
    {
        private readonly Placeholder placeholder;
        protected readonly LoadingAnimation LoadingAnimation;

        protected const double TRANSFORM_TIME = 300.0;

        private readonly Container viewContent;
        protected override Container<Drawable> Content => viewContent;

        [Resolved]
        protected IAPIProvider API { get; private set; }

        protected OnlineViewContainer(string placeholderMessage)
        {
            InternalChildren = new Drawable[]
            {
                viewContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                placeholder = new LoginPlaceholder(placeholderMessage),
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
                    PopContentOut(viewContent);
                    placeholder.ScaleTo(0.8f).Then().ScaleTo(1, 3 * TRANSFORM_TIME, Easing.OutQuint);
                    placeholder.FadeInFromZero(2 * TRANSFORM_TIME, Easing.OutQuint);
                    LoadingAnimation.Hide();
                    break;

                case APIState.Online:
                    PopContentIn(viewContent);
                    placeholder.FadeOut(TRANSFORM_TIME / 2, Easing.OutQuint);
                    LoadingAnimation.Hide();
                    break;

                case APIState.Failing:
                case APIState.Connecting:
                    PopContentOut(viewContent);
                    LoadingAnimation.Show();
                    placeholder.FadeOut(TRANSFORM_TIME / 2, Easing.OutQuint);
                    break;
            }
        }

        protected virtual void PopContentOut(Drawable content) => content.FadeOut(TRANSFORM_TIME / 2, Easing.OutQuint);

        protected virtual void PopContentIn(Drawable content) => content.FadeIn(TRANSFORM_TIME, Easing.OutQuint);

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
