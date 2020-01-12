using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.Placeholders;

namespace osu.Game.Online
{
    /// <summary>
    /// A <see cref="Container"/> for dislaying online content who require a local user to be logged in.
    /// Shows its children only when the local user is logged in and supports displaying a placeholder if not.
    /// </summary>
    public class OnlineViewContainer : Container, IOnlineComponent
    {
        private readonly Container content;
        private readonly Container placeholderContainer;
        private readonly Placeholder placeholder;

        private const int transform_time = 300;

        protected override Container<Drawable> Content => content;

        public OnlineViewContainer(string placeholder_message)
        {
            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                placeholderContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Child = placeholder = new LoginPlaceholder(placeholder_message)
                },
            };
        }

        public void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                case APIState.Offline:
                case APIState.Connecting:
                    Schedule(() =>updatePlaceholderVisibility(true));
                    break;

                default:
                    Schedule(() => updatePlaceholderVisibility(false));
                    break;
            }
        }

        private void updatePlaceholderVisibility(bool show_placeholder)
        {
            if (show_placeholder)
            {
                    content.FadeOut(transform_time / 2, Easing.OutQuint);
                    placeholder.ScaleTo(0.8f).Then().ScaleTo(1, 3 * transform_time, Easing.OutQuint);
                    placeholderContainer.FadeInFromZero(2 * transform_time, Easing.OutQuint);
            }
            else
            {
                    placeholderContainer.FadeOut(transform_time / 2, Easing.OutQuint);
                    content.FadeIn(transform_time, Easing.OutQuint);
            }
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            api.Register(this);
        }
    }
}
