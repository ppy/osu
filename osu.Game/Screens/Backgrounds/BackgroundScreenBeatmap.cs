// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Play;
using osuTK;
using osu.Framework.Graphics.Rendering;

namespace osu.Game.Screens.Backgrounds
{
    public partial class BackgroundScreenBeatmap : BackgroundScreen
    {
        /// <summary>
        /// The amount of blur to apply when full user blur is requested.
        /// </summary>
        public const float USER_BLUR_FACTOR = 25;

        protected Background Background;

        private WorkingBeatmap beatmap;

        /// <summary>
        /// Whether or not user-configured settings relating to brightness of elements should be ignored.
        /// </summary>
        /// <remarks>
        /// Beatmap background screens should not apply user settings by default.
        /// </remarks>
        public readonly Bindable<bool> IgnoreUserSettings = new Bindable<bool>(true);

        /// <summary>
        /// Whether or not the storyboard loaded should completely hide the background behind it.
        /// </summary>
        public readonly Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

        /// <summary>
        /// The amount of blur to be applied in addition to user-specified blur.
        /// </summary>
        public readonly Bindable<float> BlurAmount = new BindableFloat();

        /// <summary>
        /// The amount of dim to be used when <see cref="IgnoreUserSettings"/> is <c>true</c>.
        /// </summary>
        public readonly Bindable<float> DimWhenUserSettingsIgnored = new Bindable<float>();

        internal readonly Bindable<bool> IsBreakTime = new Bindable<bool>();

        private readonly DimmableBackground dimmable;

        protected virtual DimmableBackground CreateFadeContainer() => new DimmableBackground { RelativeSizeAxes = Axes.Both };

        public BackgroundScreenBeatmap(WorkingBeatmap beatmap = null)
        {
            Beatmap = beatmap;

            InternalChild = dimmable = CreateFadeContainer();

            dimmable.StoryboardReplacesBackground.BindTo(StoryboardReplacesBackground);
            dimmable.IgnoreUserSettings.BindTo(IgnoreUserSettings);
            dimmable.IsBreakTime.BindTo(IsBreakTime);
            dimmable.BlurAmount.BindTo(BlurAmount);
            dimmable.DimWhenUserSettingsIgnored.BindTo(DimWhenUserSettingsIgnored);
        }

        protected virtual BeatmapBackground CreateBeatmapBackground(WorkingBeatmap beatmap) => new BeatmapBackground(beatmap);

        [BackgroundDependencyLoader]
        private void load()
        {
            var background = CreateBeatmapBackground(beatmap);
            LoadComponent(background);
            switchBackground(background);
        }

        private CancellationTokenSource cancellationSource;

        public WorkingBeatmap Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value && beatmap != null)
                    return;

                beatmap = value;

                Schedule(() =>
                {
                    if ((Background as BeatmapBackground)?.Beatmap.BeatmapInfo.BackgroundEquals(beatmap?.BeatmapInfo) ?? false)
                        return;

                    cancellationSource?.Cancel();
                    LoadComponentAsync(CreateBeatmapBackground(beatmap), switchBackground, (cancellationSource = new CancellationTokenSource()).Token);
                });
            }
        }

        private void switchBackground(BeatmapBackground b)
        {
            float newDepth = 0;

            if (Background != null)
            {
                newDepth = Background.Depth + 1;
                Background.FadeOut(250);
                Background.Expire();
            }

            b.Depth = newDepth;
            b.FadeInFromZero(500, Easing.OutQuint);
            Background = dimmable.Background = b;
        }

        public override bool Equals(BackgroundScreen other)
        {
            if (!(other is BackgroundScreenBeatmap otherBeatmapBackground)) return false;

            return base.Equals(other) && beatmap == otherBeatmapBackground.Beatmap;
        }

        public partial class DimmableBackground : UserDimContainer, IColouredDimmable
        {
            /// <summary>
            /// The amount of blur to be applied to the background in addition to user-specified blur.
            /// </summary>
            /// <remarks>
            /// Used in contexts where there can potentially be both user and screen-specified blurring occuring at the same time, such as in <see cref="PlayerLoader"/>
            /// </remarks>
            public readonly Bindable<float> BlurAmount = new BindableFloat();

            public readonly Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

            public BeatmapBackground Background
            {
                get => background;
                set
                {
                    background?.Expire();

                    base.Add(background = value);
                    background.BlurTo(blurTarget, 0, Easing.OutQuint);
                }
            }

            private Bindable<double> userBlurLevel { get; set; }
            private Bindable<double> userDimColour { get; set; }

            private BeatmapBackground background;

            public override void Add(Drawable drawable)
            {
                ArgumentNullException.ThrowIfNull(drawable);

                if (drawable is Background)
                    throw new InvalidOperationException($"Use {nameof(Background)} to set a background.");

                base.Add(drawable);
            }

            /// <summary>
            /// As an optimisation, we add the two blur portions to be applied rather than actually applying two separate blurs.
            /// </summary>
            private Vector2 blurTarget => !IgnoreUserSettings.Value
                ? new Vector2(BlurAmount.Value + (float)userBlurLevel.Value * USER_BLUR_FACTOR)
                : new Vector2(BlurAmount.Value);


            public DimmableBackground()
            {
                sharedData = new ColouredDimmableSharedData();
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                userBlurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
                userDimColour = config.GetBindable<double>(OsuSetting.DimColour);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                userBlurLevel.ValueChanged += _ => UpdateVisuals();
                userDimColour.ValueChanged += _ => UpdateVisuals();
                BlurAmount.ValueChanged += _ => UpdateVisuals();
                StoryboardReplacesBackground.ValueChanged += _ => UpdateVisuals();
            }

            protected override DrawNode CreateDrawNode() => new DimmableBackgroundDrawNode(this, sharedData);

            protected override float DimLevel
            {
                get
                {
                    if ((IgnoreUserSettings.Value || ShowStoryboard.Value) && StoryboardReplacesBackground.Value)
                        return 1;

                    return base.DimLevel;
                }
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

            // The atrocity below is necessary because FadeColour and TransformTo interpolate
            // colours in linear space, then convert them back to sRGB.
            // Which means that animating ColourOffset directly will NOT make it a perfect
            // complement to DrawColour, so adding them won't necessary give 1.0 (assuming
            // no external sources of dimming).
            // And using dimContent's DrawColour directly may include external sources of dimming
            // and dimming caused by storyboard.
            // So this roundabout way is used instead, where AnimatedInvertedDimLevel approximates
            // dimContent's DrawColour, but this will only be precise if there are
            // no external sources of dimming :(

            // Needed to ensure that DimColour is not affected by the storyboard replacing
            // the background, only by user settings, also used in tests.
            protected float DimLevelNoStoryboard => base.DimLevel;

            private Colour4 invertedDimLevel => new Colour4(
                1f - DimLevelNoStoryboard,
                1f - DimLevelNoStoryboard,
                1f - DimLevelNoStoryboard,
                1f
            );

            private Colour4 animatedInvertedDimLevel;

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

                Background?.BlurTo(blurTarget, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                sharedData.Dispose();
            }

            protected class DimmableBackgroundDrawNode : CompositeDrawableDrawNode
            {
                public new DimmableBackground Source => (DimmableBackground)base.Source;

                private ColouredDimmableSharedData sharedData;

                private Colour4 drawColourOffset;

                public DimmableBackgroundDrawNode(DimmableBackground source, ColouredDimmableSharedData sharedData)
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
}
