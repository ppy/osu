// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A container that handles <see cref="Storyboard"/> loading, as well as applies user-specified visual settings to it.
    /// </summary>
    public partial class DimmableStoryboard : UserDimContainer
    {
        public Container OverlayLayerContainer { get; private set; }

        private readonly Storyboard storyboard;
        private readonly IReadOnlyList<Mod> mods;

        /// <summary>
        /// In certain circumstances, the storyboard cannot be hidden entirely even if it is fully dimmed. Such circumstances include:
        /// <list type="bullet">
        /// <item>
        /// cases where the storyboard has an overlay layer sprite, as it should continue to display fully dimmed
        /// <i>in front of</i> the playfield (https://github.com/ppy/osu/issues/29867),
        /// </item>
        /// <item>
        /// cases where the storyboard includes samples - as they are played back via drawable samples,
        /// they must be present for the playback to occur (https://github.com/ppy/osu/issues/9315).
        /// </item>
        /// </list>
        /// </summary>
        private readonly Lazy<bool> storyboardMustAlwaysBePresent;

        private DrawableStoryboard drawableStoryboard;

        /// <summary>
        /// Whether the storyboard is considered finished.
        /// </summary>
        /// <remarks>
        /// This is true by default in here, until an actual drawable storyboard is loaded, in which case it'll bind to it.
        /// </remarks>
        public IBindable<bool> HasStoryboardEnded = new BindableBool(true);

        public DimmableStoryboard(Storyboard storyboard, IReadOnlyList<Mod> mods)
        {
            this.storyboard = storyboard;
            this.mods = mods;

            storyboardMustAlwaysBePresent = new Lazy<bool>(() => storyboard.GetLayer(@"Overlay").Elements.Any() || storyboard.Layers.Any(l => l.Elements.OfType<StoryboardSampleInfo>().Any()));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(OverlayLayerContainer = new Container());

            initializeStoryboard(false);
        }

        protected override void LoadComplete()
        {
            ShowStoryboard.BindValueChanged(show =>
            {
                initializeStoryboard(true);

                if (drawableStoryboard != null)
                {
                    // Regardless of user dim setting, for the time being we need to ensure storyboards are still updated in the background (even if not displayed).
                    // If we don't do this, an intensive storyboard will have a lot of catch-up work to do at the start of a break, causing a huge stutter.
                    //
                    // This can be reconsidered when https://github.com/ppy/osu-framework/issues/6491 is resolved.
                    bool alwaysPresent = show.NewValue;

                    Content.AlwaysPresent = alwaysPresent;
                    drawableStoryboard.AlwaysPresent = alwaysPresent;
                }
            }, true);
            base.LoadComplete();
        }

        protected override bool ShowDimContent => IgnoreUserSettings.Value || (ShowStoryboard.Value && (DimLevel < 1 || storyboardMustAlwaysBePresent.Value));

        private void initializeStoryboard(bool async)
        {
            if (drawableStoryboard != null)
                return;

            if (!ShowStoryboard.Value && !IgnoreUserSettings.Value)
                return;

            drawableStoryboard = storyboard.CreateDrawable(mods);
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
