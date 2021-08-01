// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osuTK;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A container that handles <see cref="Storyboard"/> loading, as well as applies user-specified visual settings to it.
    /// </summary>
    public class DimmableStoryboard : UserDimContainer
    {
        public Container OverlayLayerContainer { get; private set; }

        private readonly Storyboard storyboard;
        private DrawableStoryboard drawableStoryboard;

        /// <summary>
        /// Whether the storyboard is considered finished.
        /// </summary>
        /// <remarks>
        /// This is true by default in here, until an actual drawable storyboard is loaded, in which case it'll bind to it.
        /// </remarks>
        public IBindable<bool> HasStoryboardEnded = new BindableBool(true);

        public DimmableStoryboard(Storyboard storyboard)
        {
            this.storyboard = storyboard;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(OverlayLayerContainer = new Container());

            initializeStoryboard(false);
        }

        protected override void LoadComplete()
        {
            ShowStoryboard.BindValueChanged(_ => initializeStoryboard(true), true);
            base.LoadComplete();
        }

        protected override bool ShowDimContent => IgnoreUserSettings.Value || (ShowStoryboard.Value && DimLevel < 1);

        private void initializeStoryboard(bool async)
        {
            if (drawableStoryboard != null)
                return;

            if (!ShowStoryboard.Value && !IgnoreUserSettings.Value)
                return;

            double storyboardLastestEventTime = storyboard.LatestEventTime ?? 0;
            if (!storyboard.ReplacesBackground && storyboardLastestEventTime != 0)
            {
                var sprite = new StoryboardSprite(storyboard.BeatmapInfo.Metadata.BackgroundFile, Anchor.Centre, new Vector2(320, 240));
                sprite.TimelineGroup.Alpha.Add(Easing.None, 0, storyboardLastestEventTime, 1, 1);
                storyboard.GetLayer("Background").Elements.Insert(0, sprite);
            }

            drawableStoryboard = storyboard.CreateDrawable();
            HasStoryboardEnded.BindTo(drawableStoryboard.HasStoryboardEnded);

            if (async)
                LoadComponentAsync(drawableStoryboard, onStoryboardCreated);
            else
                onStoryboardCreated(drawableStoryboard);
        }

        private void onStoryboardCreated(DrawableStoryboard storyboard)
        {
            Add(storyboard);
            OverlayLayerContainer.Add(storyboard.OverlayLayer.CreateProxy());
        }
    }
}
