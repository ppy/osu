// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osuTK.Graphics;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that applies user-configured dim levels to its contents.
    /// This container specifies behavior that applies to both Storyboards and Backgrounds.
    /// </summary>
    public class UserDimContainer : Container
    {
        private const float background_fade_duration = 800;

        private Bindable<double> dimLevel { get; set; }

        private Bindable<bool> showStoryboard { get; set; }

        /// <summary>
        /// Whether or not user-configured dim levels should be applied to the container.
        /// </summary>
        public readonly Bindable<bool> EnableUserDim = new Bindable<bool>();

        /// <summary>
        /// Whether or not the storyboard loaded should completely hide the background behind it.
        /// </summary>
        public readonly Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

        protected Container DimContainer { get; }

        protected override Container<Drawable> Content => DimContainer;

        private readonly bool isStoryboard;

        /// <summary>
        /// Creates a new <see cref="UserDimContainer"/>.
        /// </summary>
        /// <param name="isStoryboard"> Whether or not this instance of UserDimContainer contains a storyboard.
        /// <remarks>
        /// While both backgrounds and storyboards allow user dim levels to be applied, storyboards can be toggled via <see cref="showStoryboard"/>
        /// and can cause backgrounds to become hidden via <see cref="StoryboardReplacesBackground"/>.
        /// </remarks>
        /// </param>
        public UserDimContainer(bool isStoryboard = false)
        {
            this.isStoryboard = isStoryboard;
            AddInternal(DimContainer = new Container { RelativeSizeAxes = Axes.Both });
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            dimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            showStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
            EnableUserDim.ValueChanged += _ => updateBackgroundDim();
            dimLevel.ValueChanged += _ => updateBackgroundDim();
            showStoryboard.ValueChanged += _ => updateBackgroundDim();
            StoryboardReplacesBackground.ValueChanged += _ => updateBackgroundDim();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateBackgroundDim();
        }

        private void updateBackgroundDim()
        {
            if (isStoryboard)
            {
                DimContainer.FadeTo(!showStoryboard.Value || dimLevel.Value == 1 ? 0 : 1, background_fade_duration, Easing.OutQuint);
            }
            else
            {
                // The background needs to be hidden in the case of it being replaced by the storyboard
                DimContainer.FadeTo(showStoryboard.Value && StoryboardReplacesBackground.Value ? 0 : 1, background_fade_duration, Easing.OutQuint);
            }

            DimContainer.FadeColour(EnableUserDim.Value ? OsuColour.Gray(1 - (float)dimLevel.Value) : Color4.White, background_fade_duration, Easing.OutQuint);
        }
    }
}
