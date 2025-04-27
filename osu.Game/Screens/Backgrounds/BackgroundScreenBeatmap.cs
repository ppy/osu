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
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Play;
using osuTK;

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

        /// <summary>
        /// Reloads beatmap's background.
        /// </summary>
        public void RefreshBackground()
        {
            Schedule(() =>
            {
                cancellationSource?.Cancel();
                LoadComponentAsync(CreateBeatmapBackground(beatmap), switchBackground, (cancellationSource = new CancellationTokenSource()).Token);
            });
        }

        private void switchBackground(BeatmapBackground b)
        {
            float newDepth = 0;

            if (Background != null)
            {
                newDepth = Background.Depth + 1;
                Background.FinishTransforms();
                Background.FadeOut(250);
                Background.Expire();
            }

            b.Depth = newDepth;
            Background = dimmable.BeatmapBackground = b;
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

            public Background Background => background;

            public BeatmapBackground BeatmapBackground
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
                    throw new InvalidOperationException($"Use {nameof(BeatmapBackground)} to set a background.");

                base.Add(drawable);
            }

            /// <summary>
            /// As an optimisation, we add the two blur portions to be applied rather than actually applying two separate blurs.
            /// </summary>
            private Vector2 blurTarget => !IgnoreUserSettings.Value
                ? new Vector2(BlurAmount.Value + (float)userBlurLevel.Value * USER_BLUR_FACTOR)
                : new Vector2(BlurAmount.Value);

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
                    if (IgnoreUserSettings.Value)
                        return Colour4.Black;

                    float grayscaleDimColour = (float)userDimColour.Value * DimLevel;
                    return new Colour4(grayscaleDimColour, grayscaleDimColour, grayscaleDimColour, 1.0f);
                }
            }

            private Colour4 colourOffset;

            protected Colour4 ColourOffset
            {
                get => colourOffset;
                set
                {
                    if (value.Equals(colourOffset))
                        return;

                    colourOffset = value;
                    Invalidate(Invalidation.Colour);
                }
            }

            public Colour4 DrawColourOffset => ColourOffset * DrawColourInfo.Colour;

            protected override void UpdateVisuals()
            {
                base.UpdateVisuals();

                this.TransformTo(nameof(ColourOffset), DimColour, BACKGROUND_FADE_DURATION, Easing.OutQuint);

                Background?.BlurTo(blurTarget, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            }
        }
    }
}
