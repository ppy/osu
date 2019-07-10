// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that handles <see cref="Storyboard"/> loading, as well as applies user-specified visual settings to it.
    /// </summary>
    public class StoryboardContainer : UserDimContainer
    {
        private readonly Storyboard storyboard;
        private DrawableStoryboard drawableStoryboard;

        public StoryboardContainer(Storyboard storyboard)
        {
            this.storyboard = storyboard;
            EnableUserDim.Default = true;
            EnableUserDim.Value = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            initializeStoryboard(false);
        }

        protected override void LoadComplete()
        {
            ShowStoryboard.BindValueChanged(_ => initializeStoryboard(true), true);
            base.LoadComplete();
        }

        protected override void ApplyFade()
        {
            DimContainer.FadeTo(!ShowStoryboard.Value || UserDimLevel.Value == 1 ? 0 : 1, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }

        private void initializeStoryboard(bool async)
        {
            if (drawableStoryboard != null)
                return;

            if (!ShowStoryboard.Value)
                return;

            drawableStoryboard = storyboard.CreateDrawable();
            drawableStoryboard.Masking = true;

            if (async)
                LoadComponentAsync(drawableStoryboard, Add);
            else
                Add(drawableStoryboard);
        }
    }
}
