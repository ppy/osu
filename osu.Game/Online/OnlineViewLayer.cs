// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for  full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Placeholders;
using osuTK.Graphics;

namespace osu.Game.Online
{
    /// <summary>
    /// A layer which displays on top of content which require a local user to be logged in.
    /// Blocks input to the underlying content, dims the optional target content and displays a login placeholder when the user isn't logged in.
    /// </summary>
    public class OnlineViewLayer : VisibilityContainer, IOnlineComponent
    {
        protected LoadingSpinner LoadingSpinner;

        private readonly Drawable viewTarget;

        private readonly string placeholderMessage;

        private Placeholder placeholder;

        private const double transform_duration = 500;

        [Resolved]
        protected IAPIProvider API { get; private set; }

        public OnlineViewLayer(string placeholderMessage, Drawable viewTarget)
        {
            this.placeholderMessage = placeholderMessage;
            this.viewTarget = viewTarget;

            RelativeSizeAxes = Axes.Both;
        }

        public override bool HandleNonPositionalInput => false;

        public override bool HandlePositionalInput => false;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                placeholder = new LoginPlaceholder(placeholderMessage)
                {
                    Alpha = 0,
                },
                LoadingSpinner = new LoadingSpinner(true)
                {
                    Alpha = 0,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            API.Register(this);
        }

        public void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                case APIState.Offline:
                    placeholder.ScaleTo(0.8f).Then().ScaleTo(1, transform_duration, Easing.OutQuint);
                    placeholder.FadeInFromZero(2 * transform_duration, Easing.OutQuint);
                    LoadingSpinner.Hide();
                    Show();
                    break;

                case APIState.Online:
                    placeholder.FadeOut(transform_duration, Easing.OutQuint);
                    LoadingSpinner.Hide();
                    Hide();
                    break;

                case APIState.Failing:
                case APIState.Connecting:
                    LoadingSpinner.Show();
                    placeholder.FadeOut(transform_duration, Easing.OutQuint);
                    Show();
                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            API?.Unregister(this);
            base.Dispose(isDisposing);

            if (State.Value == Visibility.Visible)
            {
                // ensure we don't leave the view in a bad state.
                viewTarget?.FadeColour(Color4.White, transform_duration, Easing.OutQuint);
            }
        }

        protected override void PopIn() => viewTarget?.FadeColour(OsuColour.Gray(0.5f), transform_duration, Easing.OutQuint);

        protected override void PopOut() => viewTarget?.FadeColour(Color4.White, transform_duration, Easing.OutQuint);
    }
}
