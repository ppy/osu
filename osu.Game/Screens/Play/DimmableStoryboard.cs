// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A container that handles <see cref="Storyboard"/> loading, as well as applies user-specified visual settings to it.
    /// </summary>
    public partial class DimmableStoryboard : UserDimContainer, IColouredDimmable
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

        private Bindable<double> userDimColour { get; set; }

        public DimmableStoryboard(Storyboard storyboard, IReadOnlyList<Mod> mods)
        {
            this.storyboard = storyboard;
            this.mods = mods;

            storyboardMustAlwaysBePresent = new Lazy<bool>(() => storyboard.GetLayer(@"Overlay").Elements.Any() || storyboard.Layers.Any(l => l.Elements.OfType<StoryboardSampleInfo>().Any()));

            sharedData = new ColouredDimmableSharedData();
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userDimColour = config.GetBindable<double>(OsuSetting.DimColour);

            Add(OverlayLayerContainer = new Container());

            initializeStoryboard(false);
        }

        protected override void LoadComplete()
        {
            userDimColour.ValueChanged += _ => UpdateVisuals();
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

        protected override DrawNode CreateDrawNode() => new DimmableStoryboardDrawNode(this, sharedData);

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

        protected virtual Colour4 DimColour
        {
            get
            {
                // This assumes that DimColour shouldn't be affected by DimWhenUserSettingsIgnored
                // (as it's used to dim things in song select only)
                if (IgnoreUserSettings.Value)
                    return Colour4.Black;

                float grayscaleDimColour = (float)userDimColour.Value;
                return new Colour4(grayscaleDimColour, grayscaleDimColour, grayscaleDimColour, 1.0f);
            }
        }


        // See `DimmableBackground` class for explanation of this atrocity.
        private Colour4 invertedDimLevel => new Colour4(
            1f - DimLevel,
            1f - DimLevel,
            1f - DimLevel,
            1f
        );

        private Colour4 animatedInvertedDimLevel = Colour4.White;

        protected Colour4 AnimatedInvertedDimLevel
        {
            get => animatedInvertedDimLevel;
            set
            {
                if (value.Equals(animatedInvertedDimLevel))
                    return;

                animatedInvertedDimLevel = value;
                Invalidate(Invalidation.Colour);
            }
        }

        private Colour4 animatedDimColour;

        protected Colour4 AnimatedDimColour
        {
            get => animatedDimColour;
            set
            {
                if (value.Equals(animatedDimColour))
                    return;

                animatedDimColour = value;
                Invalidate(Invalidation.Colour);
            }
        }

        protected Colour4 AnimatedDimLevel => new Colour4(
            1f - AnimatedInvertedDimLevel.R,
            1f - AnimatedInvertedDimLevel.G,
            1f - AnimatedInvertedDimLevel.B,
            1f
        );

        protected Colour4 ColourOffset => AnimatedDimColour * AnimatedDimLevel;

        private ColouredDimmableSharedData sharedData;

        public IColouredDimmable DrawColourOffsetSource => this;

        public Colour4 DrawColourOffset => ColourOffset * DrawColourInfo.Colour;

        public ColouredDimmableSharedData ColouredDimmableSharedData => sharedData;

        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();

            this.TransformTo(nameof(AnimatedInvertedDimLevel), invertedDimLevel, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            this.TransformTo(nameof(AnimatedDimColour), DimColour, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            sharedData.Dispose();
        }

        protected class DimmableStoryboardDrawNode : CompositeDrawableDrawNode
        {
            public new DimmableStoryboard Source => (DimmableStoryboard)base.Source;

            private ColouredDimmableSharedData sharedData;

            private Colour4 drawColourOffset;

            public DimmableStoryboardDrawNode(DimmableStoryboard source, ColouredDimmableSharedData sharedData)
                : base(source)
            {
                this.sharedData = sharedData;
            }

            public override void ApplyState()
            {
                base.ApplyState();

                drawColourOffset = Source.DrawColourOffset;
            }

            protected override void Draw(IRenderer renderer)
            {
                // Update shared data before drawing children that rely on it
                if (!sharedData.IsInitialised)
                {
                    sharedData.Initialize(renderer);
                }

                sharedData.SetDrawColourOffset(drawColourOffset);

                base.Draw(renderer);
            }
        }
    }
}
