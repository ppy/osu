// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that applies user-configured visual settings to its contents.
    /// This container specifies behavior that applies to both Storyboards and Backgrounds.
    /// </summary>
    public class VisualSettingsContainer : Container
    {
        private const float background_fade_duration = 800;

        private Bindable<double> dimLevel { get; set; }

        private Bindable<double> blurLevel { get; set; }

        private Bindable<bool> showStoryboard { get; set; }

        /// <summary>
        /// Whether or not user-configured dim levels should be applied to the container.
        /// </summary>
        public readonly Bindable<bool> EnableVisualSettings = new Bindable<bool>();

        /// <summary>
        /// Whether or not the storyboard loaded should completely hide the background behind it.
        /// </summary>
        public readonly Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

        protected Container LocalContainer { get; }

        protected override Container<Drawable> Content => LocalContainer;

        private readonly bool isStoryboard;

        public Bindable<float> AddedBlur = new Bindable<float>();

        public Vector2 BlurTarget => EnableVisualSettings.Value
            ? new Vector2(AddedBlur.Value + (float)blurLevel.Value * 25)
            : new Vector2(AddedBlur.Value);

        /// <summary>
        /// Creates a new <see cref="VisualSettingsContainer"/>.
        /// </summary>
        /// <param name="isStoryboard"> Whether or not this instance contains a storyboard.
        /// <remarks>
        /// While both backgrounds and storyboards allow user dim levels to be applied, storyboards can be toggled via <see cref="showStoryboard"/>
        /// and can cause backgrounds to become hidden via <see cref="StoryboardReplacesBackground"/>. Storyboards are also currently unable to be blurred.
        /// </remarks>
        /// </param>
        public VisualSettingsContainer(bool isStoryboard = false)
        {
            this.isStoryboard = isStoryboard;
            AddInternal(LocalContainer = new Container { RelativeSizeAxes = Axes.Both });
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            dimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            blurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
            showStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
            EnableVisualSettings.ValueChanged += _ => UpdateVisuals();
            dimLevel.ValueChanged += _ => UpdateVisuals();
            blurLevel.ValueChanged += _ => UpdateVisuals();
            showStoryboard.ValueChanged += _ => UpdateVisuals();
            StoryboardReplacesBackground.ValueChanged += _ => UpdateVisuals();
            AddedBlur.ValueChanged += _ => UpdateVisuals();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (isStoryboard)
            {
                LocalContainer.FadeTo(!showStoryboard.Value || dimLevel.Value == 1 ? 0 : 1, background_fade_duration, Easing.OutQuint);
            }
            else
            {
                // The background needs to be hidden in the case of it being replaced by the storyboard
                LocalContainer.FadeTo(showStoryboard.Value && StoryboardReplacesBackground.Value ? 0 : 1, background_fade_duration, Easing.OutQuint);

                foreach (Drawable c in LocalContainer)
                {
                    // Only blur if this container contains a background
                    // We can't blur the container like we did with the dim because buffered containers add considerable draw overhead.
                    // As a result, this blurs the background directly.
                    ((Background)c)?.BlurTo(BlurTarget, background_fade_duration, Easing.OutQuint);
                }
            }

            LocalContainer.FadeColour(EnableVisualSettings.Value ? OsuColour.Gray(1 - (float)dimLevel.Value) : Color4.White, background_fade_duration, Easing.OutQuint);
        }
    }
}
