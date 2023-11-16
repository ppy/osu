// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
    public partial class OnlineViewContainer : Container
    {
        protected LoadingSpinner LoadingSpinner { get; private set; }

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private readonly string placeholderMessage;

        private Drawable placeholder;

        private const double transform_duration = 300;

        [Resolved]
        protected IAPIProvider API { get; private set; }

        /// <summary>
        /// Construct a new instance of an online view container.
        /// </summary>
        /// <param name="placeholderMessage">The message to display when not logged in. If empty, no button will display.</param>
        public OnlineViewContainer(string placeholderMessage)
        {
            this.placeholderMessage = placeholderMessage;
        }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            InternalChildren = new[]
            {
                Content,
                placeholder = string.IsNullOrEmpty(placeholderMessage) ? Empty() : new LoginPlaceholder(placeholderMessage),
                LoadingSpinner = new LoadingSpinner
                {
                    Alpha = 0,
                }
            };

            apiState.BindTo(api.State);
            apiState.BindValueChanged(onlineStateChanged, true);
        }

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            switch (state.NewValue)
            {
                case APIState.Offline:
                    PopContentOut(Content);
                    placeholder.ScaleTo(0.8f).Then().ScaleTo(1, 3 * transform_duration, Easing.OutQuint);
                    placeholder.FadeInFromZero(2 * transform_duration, Easing.OutQuint);
                    LoadingSpinner.Hide();
                    break;

                case APIState.Online:
                    PopContentIn(Content);
                    placeholder.FadeOut(transform_duration / 2, Easing.OutQuint);
                    LoadingSpinner.Hide();
                    break;

                case APIState.Failing:
                case APIState.Connecting:
                case APIState.RequiresSecondFactorAuth:
                    PopContentOut(Content);
                    LoadingSpinner.Show();
                    placeholder.FadeOut(transform_duration / 2, Easing.OutQuint);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        });

        /// <summary>
        /// Applies a transform to the online content to make it hidden.
        /// </summary>
        protected virtual void PopContentOut(Drawable content) => content.FadeOut(transform_duration / 2, Easing.OutQuint);

        /// <summary>
        /// Applies a transform to the online content to make it visible.
        /// </summary>
        protected virtual void PopContentIn(Drawable content) => content.FadeIn(transform_duration, Easing.OutQuint);
    }
}
