// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics.Containers;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A container that handles <see cref="Storyboard"/> loading, as well as applies user-specified visual settings to it.
    /// </summary>
    public class DimmableStoryboard : UserDimContainer
    {
        private readonly Storyboard storyboard;
        private DrawableStoryboard drawableStoryboard;

        public DimmableStoryboard(Storyboard storyboard)
        {
            this.storyboard = storyboard;
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

        protected override bool ShowDimContent => ShowStoryboard.Value && DimLevel < 1;

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
