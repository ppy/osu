// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
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

        [BackgroundDependencyLoader]
        private void load()
        {
            var background = new BeatmapBackground(beatmap);
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
                    LoadComponentAsync(new BeatmapBackground(beatmap), switchBackground, (cancellationSource = new CancellationTokenSource()).Token);
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
                LoadComponentAsync(new BeatmapBackground(beatmap), switchBackground, (cancellationSource = new CancellationTokenSource()).Token);
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
            dimmable.Background = Background = b;
        }

        public override bool Equals(BackgroundScreen other)
        {
            if (!(other is BackgroundScreenBeatmap otherBeatmapBackground)) return false;

            return base.Equals(other) && beatmap == otherBeatmapBackground.Beatmap;
        }

        public partial class DimmableBackground : UserDimContainer
        {
            /// <summary>
            /// The amount of blur to be applied to the background in addition to user-specified blur.
            /// </summary>
            /// <remarks>
            /// Used in contexts where there can potentially be both user and screen-specified blurring occuring at the same time, such as in <see cref="PlayerLoader"/>
            /// </remarks>
            public readonly Bindable<float> BlurAmount = new BindableFloat();

            public readonly Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

            public partial class DimmableBeatmapBackgroundSprite : Sprite {
                public float DimLevel;
                public float DimColour;

                [BackgroundDependencyLoader]
                private void load(ShaderManager shaders)
                {
                    TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "DimmableBeatmapBackground");
                }

                protected override DrawNode CreateDrawNode() => new DimmableBeatmapBackgroundSpriteDrawNode(this);

                public class DimmableBeatmapBackgroundSpriteDrawNode : SpriteDrawNode {
                    public new DimmableBeatmapBackgroundSprite Source => (DimmableBeatmapBackgroundSprite)base.Source;

                    public DimmableBeatmapBackgroundSpriteDrawNode(DimmableBeatmapBackgroundSprite source)
                        : base(source)
                    {
                    }

                    private float DimLevel;
                    private float DimColour;

                    private IShader textureShader;

                    public override void ApplyState()
                    {
                        base.ApplyState();

                        DimLevel = Source.DimLevel;
                        DimColour = Source.DimColour;

                        textureShader = Source.TextureShader;
                    }

                    private IUniformBuffer<DimmableBeatmapBackgroundParameters> dimmableBeatmapBackgroundParametersBuffer;

                    private void BindParametersBuffer(IRenderer renderer)
                    {
                        dimmableBeatmapBackgroundParametersBuffer ??= renderer.CreateUniformBuffer<DimmableBeatmapBackgroundParameters>();

                        dimmableBeatmapBackgroundParametersBuffer.Data = dimmableBeatmapBackgroundParametersBuffer.Data with
                        {
                            DimColour = new Vector4(DimColour, DimColour, DimColour, 1.0f),
                            DimLevel = DimLevel,
                        };

                        textureShader.BindUniformBlock("m_DimmableBeatmapBackgroundParameters", dimmableBeatmapBackgroundParametersBuffer);
                    }

                    protected override void Draw(IRenderer renderer)
                    {
                        BindParametersBuffer(renderer);
                        base.Draw(renderer);
                    }

                    protected override void DrawOpaqueInterior(IRenderer renderer)
                    {
                        BindParametersBuffer(renderer);
                        base.DrawOpaqueInterior(renderer);
                    }

                    protected override void Dispose(bool isDisposing)
                    {
                        base.Dispose(isDisposing);
                        dimmableBeatmapBackgroundParametersBuffer?.Dispose();
                    }

                    [StructLayout(LayoutKind.Sequential, Pack = 1)]
                    private record struct DimmableBeatmapBackgroundParameters
                    {
                        public UniformVector4 DimColour;
                        public UniformFloat DimLevel;
                        private readonly UniformPadding12 pad1;
                    }
                }
            }

            public partial class DimmableBeatmapBackground : Background {
                private Background Background;

                private float dimLevel;
                private float dimColour;

                public float DimLevel {
                    get => dimLevel;
                    set {
                        dimLevel = value;
                        DimmableBeatmapBackgroundSprite sprite = (DimmableBeatmapBackgroundSprite)Sprite;
                        sprite.DimLevel = dimLevel;

                        bufferedContainer?.ForceRedraw();
                    }
                }

                public float DimColour {
                    get => dimColour;
                    set {
                        dimColour = value;
                        DimmableBeatmapBackgroundSprite sprite = (DimmableBeatmapBackgroundSprite)Sprite;
                        sprite.DimColour = dimColour;

                        bufferedContainer?.ForceRedraw();
                    }
                }

                public DimmableBeatmapBackground(Background background) {
                    Background = background;

                    Sprite = new DimmableBeatmapBackgroundSprite {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        FillMode = FillMode.Fill,
                    };
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    if (Background != null)
                        Sprite.Texture = Background.Sprite.Texture;
                }
            }

            public Background Background
            {
                get => background;
                set
                {
                    background?.Expire();

                    base.Add(background = new DimmableBeatmapBackground(value));
                    background.BlurTo(blurTarget, 0, Easing.OutQuint);
                }
            }

            private Bindable<double> userBlurLevel { get; set; }
            private Bindable<double> userDimColour { get; set; }

            private DimmableBeatmapBackground background;

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

                if (background != null) {
                    background.DimLevel = DimLevel;
                    background.DimColour = DimColour;
                }
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

            protected virtual float DimColour => (float)userDimColour.Value;

            protected override void UpdateVisuals()
            {
                ContentDisplayed = ShowDimContent;

                Content.FadeTo(ContentDisplayed ? 1 : 0, BACKGROUND_FADE_DURATION, Easing.OutQuint);
                background?.TransformTo(nameof(DimmableBeatmapBackground.DimLevel), DimLevel, BACKGROUND_FADE_DURATION, Easing.OutQuint);

                background?.TransformTo(nameof(DimmableBeatmapBackground.DimColour), DimColour, BACKGROUND_FADE_DURATION, Easing.OutQuint);

                Background?.BlurTo(blurTarget, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            }
        }
    }
}
