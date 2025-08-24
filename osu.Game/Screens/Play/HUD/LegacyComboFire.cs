// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A re-implementation of the "Combo Fire" effect from old versions of osu!stable. Intentional visual changes:
    /// <list type="bullet">
    /// <item>The top-right corner of the orange fire is masked out.</item>
    /// <item>Blue fire extends slightly beyond the normal height to avoid clipping.</item>
    /// <item>Fire smoothly fades in, out, and between colours.</item>
    /// </list>
    /// </summary>
    public partial class LegacyComboFire : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private Player? player { get; set; }

        [Resolved]
        private SkinEditor? skinEditor { get; set; }

        private readonly LegacyComboFireInner inner;

        public LegacyComboFire()
        {
            // The reciprocal heights specified here make this component's representation in the skin editor more bearable, only covering the bottom quarter of the screen (by default) instead of the whole thing.
            const float skin_editor_height = 0.25f;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            RelativeSizeAxes = Axes.Both;
            Height = skin_editor_height;

            AddInternal(inner = new LegacyComboFireInner
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.Both,
                Height = 1 / skin_editor_height,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Proxy into the player's combo fire container to draw at the correct depth.
            // Skip proxying if inside the skin editor toolbox.
            if (player != null && skinEditor == null)
                player.ComboFireProxyContainer.Add(CreateProxy());

            // Inside the skin editor toolbox, don't use the reciprocal heights trick.
            if (skinEditor != null)
            {
                Height = 1;
                inner.Height = 1;
            }
        }

        private partial class LegacyComboFireInner : Drawable
        {
            #region Constants from osu!stable

            private const int orange_combo = 30;
            private const int blue_combo = 500;

            /// <remarks>
            /// In practice the max alpha is achieved much earlier than this (at 89 combo) due to the clamp on <see cref="alphaFactor"/>.
            /// </remarks>
            private const int max_alpha_combo = 329;

            /// <remarks>
            /// This used to be a config option called <c>ComboFireHeight</c>. It was removed from the options menu early on, and the fire looks awful at higher values, so let's just keep it at the default (<c>3</c>).
            /// </remarks>
            private const int config_height = 3;

            private static readonly float alpha_factor_max =
                (float)IBeatmapDifficultyInfo.DifficultyRange(config_height, 0.05, 0.3, 0.8);

            private static readonly float height_scale = (Math.Max(0, config_height / 10f - 0.5f) + 1) / 2;

            #endregion

            private const double fade_duration = 300;

            [Resolved]
            private Player? player { get; set; }

            // Used only for the speed of the fire animation. The fades and colour transitions don't depend on the gameplay clock rate.
            [Resolved]
            private IGameplayClock? gameplayClock { get; set; }

            private IShader? shader;
            private Texture? orange;
            private Texture? blue;
            private Texture? effects;

            private BindableNumber<int>? comboBindable;
            private IBindable<bool>? isBreakBindable;

            private float breakAlphaBacking = 1;
            private float comboAlphaBacking;

            /// <summary>
            /// Separate internal alpha control to fade out during breaks, regardless of HUD setting.
            /// </summary>
            private float breakAlpha
            {
                get => breakAlphaBacking;
                set
                {
                    breakAlphaBacking = value;
                    computeAlpha();
                }
            }

            /// <summary>
            /// Separate internal alpha control to fade in/out based on combo.
            /// </summary>
            private float comboAlpha
            {
                get => comboAlphaBacking;
                set
                {
                    comboAlphaBacking = value;
                    computeAlpha();
                }
            }

            private void computeAlpha() => Alpha = breakAlpha * comboAlpha;

            private FireColour fireColour;

            private float orangeToBlueLerp { get; set; }

            /// <summary>
            /// An alpha control passed directly to the shader for use in a final alpha multiplication step.
            /// </summary>
            /// <remarks>
            /// The exact effect this has on the image is somewhat arbitrary, hence the generic name. Increasing this value results in greater or equal opacity.
            /// </remarks>
            private float alphaFactor;

            [BackgroundDependencyLoader]
            private void load(ScoreProcessor? scoreProcessor, TextureStore textures, ShaderManager shaders)
            {
                shader = shaders.Load(@"PositionAndColour", @"LegacyComboFire");
                orange = textures.Get(@"Gameplay/LegacyComboFire/orange", WrapMode.ClampToEdge, WrapMode.ClampToEdge);
                blue = textures.Get(@"Gameplay/LegacyComboFire/blue", WrapMode.ClampToEdge, WrapMode.ClampToEdge);
                effects = textures.Get(@"Gameplay/LegacyComboFire/effects", WrapMode.ClampToEdge, WrapMode.ClampToEdge);

                if (scoreProcessor != null)
                {
                    comboBindable = scoreProcessor.Combo.GetBoundCopy();
                    comboBindable.BindValueChanged(onComboChange, true);
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (player != null)
                {
                    isBreakBindable = player.IsBreakTime.GetBoundCopy();
                    isBreakBindable.BindValueChanged(_ => this.TransformTo(nameof(breakAlpha), isBreakBindable.Value ? 0f : 1f, HUDOverlay.FADE_DURATION, HUDOverlay.FADE_EASING), true);
                }
            }

            protected override void Update()
            {
                base.Update();

                Invalidate(Invalidation.DrawNode);
            }

            protected override DrawNode CreateDrawNode() => new LegacyComboFireDrawNode(this);

            private void onComboChange(ValueChangedEvent<int> combo)
            {
                FireColour oldColour = fireColour;
                fireColour = getColour(combo.NewValue);

                // When the new colour is None, let the fade out transformation handle alpha changes
                if (fireColour != FireColour.None)
                {
                    alphaFactor = Math.Clamp
                    (
                        (float)(combo.NewValue - orange_combo + 1) / (max_alpha_combo - orange_combo + 1),
                        0,
                        alpha_factor_max
                    );
                }

                // Don't play transforms if the colour didn't change
                if (oldColour == fireColour)
                    return;

                if (fireColour == FireColour.None)
                    this.TransformTo(nameof(comboAlpha), 0f, fade_duration);
                else
                {
                    if (oldColour == FireColour.None)
                        this.TransformTo(nameof(comboAlpha), 1f, fade_duration);

                    this.TransformTo
                    (
                        nameof(orangeToBlueLerp),
                        newValue: fireColour == FireColour.Orange ? 0f : 1f,
                        // If fading in from no colour, set the fire colour immediately
                        duration: oldColour == FireColour.None ? 0 : fade_duration
                    );
                }

                static FireColour getColour(int combo)
                {
                    if (combo >= blue_combo)
                        return FireColour.Blue;

                    if (combo >= orange_combo)
                        return FireColour.Orange;

                    return FireColour.None;
                }
            }

            private enum FireColour
            {
                None,
                Orange,
                Blue,
            }

            private class LegacyComboFireDrawNode : DrawNode
            {
                protected new LegacyComboFireInner Source => (LegacyComboFireInner)base.Source;

                private IShader? shader;
                private Texture? orange;
                private Texture? blue;
                private Texture? effects;
                private float timeSeconds;
                private float alphaFactor;
                private float orangeToBlueLerp;
                private Matrix3 screenSpaceMatrix;

                private IUniformBuffer<FireParameters>? fireParametersBuffer;
                private IVertexBatch<Vertex2D>? quadBatch;

                public LegacyComboFireDrawNode(LegacyComboFireInner source)
                    : base(source)
                {
                }

                public override void ApplyState()
                {
                    base.ApplyState();

                    shader = Source.shader;
                    orange = Source.orange;
                    blue = Source.blue;
                    effects = Source.effects;
                    timeSeconds = (float)(Source.gameplayClock ?? Source.Clock).CurrentTime / 1000;
                    alphaFactor = Source.alphaFactor;
                    orangeToBlueLerp = Source.orangeToBlueLerp;

                    // Compute a projection matrix that transforms UV coordinates directly to screen space
                    Vector2 topLeft = Vector2.Lerp(Source.ScreenSpaceDrawQuad.BottomLeft, Source.ScreenSpaceDrawQuad.TopLeft, height_scale);
                    Vector2 topRight = Vector2.Lerp(Source.ScreenSpaceDrawQuad.BottomRight, Source.ScreenSpaceDrawQuad.TopRight, height_scale);
                    screenSpaceMatrix = Matrix3.Identity;
                    // Basis vectors
                    screenSpaceMatrix.Row0.Xy = topRight - topLeft;
                    screenSpaceMatrix.Row1.Xy = Source.ScreenSpaceDrawQuad.BottomLeft - topLeft;
                    // Offset
                    screenSpaceMatrix.Row2.Xy = topLeft;
                }

                protected override void Draw(IRenderer renderer)
                {
                    base.Draw(renderer);

                    if (shader == null ||
                        orange == null ||
                        blue == null ||
                        effects == null)
                        return;

                    if (!renderer.BindTexture(orange, 0) ||
                        !renderer.BindTexture(blue, 1) ||
                        !renderer.BindTexture(effects, 2))
                        return;

                    RectangleF orangeTexRect = orange.GetTextureRect();
                    RectangleF blueTexRect = blue.GetTextureRect();
                    RectangleF effectsTexRect = effects.GetTextureRect();

                    fireParametersBuffer ??= renderer.CreateUniformBuffer<FireParameters>();
                    quadBatch ??= renderer.CreateQuadBatch<Vertex2D>(1, 1);

                    renderer.SetBlend(DrawColourInfo.Blending);
                    renderer.PushLocalMatrix(screenSpaceMatrix);
                    shader.Bind();
                    shader.BindUniformBlock(@"m_FireParameters", fireParametersBuffer);

                    fireParametersBuffer.Data = new FireParameters
                    {
                        Time = timeSeconds,
                        AlphaFactor = alphaFactor,
                        OrangeToBlueLerp = orangeToBlueLerp,
                        OrangeTexRect = new Vector4(orangeTexRect.Left, orangeTexRect.Top, orangeTexRect.Right, orangeTexRect.Bottom),
                        BlueTexRect = new Vector4(blueTexRect.Left, blueTexRect.Top, blueTexRect.Right, blueTexRect.Bottom),
                        EffectsTexRect = new Vector4(effectsTexRect.Left, effectsTexRect.Top, effectsTexRect.Right, effectsTexRect.Bottom),
                    };

                    // Extra height allowance given for blue fire to avoid clipping
                    const float extra_height = 0.35f;

                    quadBatch.Add(new Vertex2D
                    {
                        Position = new Vector2(0, -extra_height),
                        Colour = DrawColourInfo.Colour.TopLeft,
                    });
                    quadBatch.Add(new Vertex2D
                    {
                        Position = new Vector2(0, 1),
                        Colour = DrawColourInfo.Colour.BottomLeft,
                    });
                    quadBatch.Add(new Vertex2D
                    {
                        Position = new Vector2(1, 1),
                        Colour = DrawColourInfo.Colour.BottomRight,
                    });
                    quadBatch.Add(new Vertex2D
                    {
                        Position = new Vector2(1, -extra_height),
                        Colour = DrawColourInfo.Colour.TopRight,
                    });

                    quadBatch.Draw();

                    shader.Unbind();
                    renderer.PopLocalMatrix();
                }

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);

                    fireParametersBuffer?.Dispose();
                    quadBatch?.Dispose();
                }

                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                private record struct FireParameters
                {
                    public UniformFloat Time;
                    public UniformFloat AlphaFactor;
                    public UniformFloat OrangeToBlueLerp;
                    private readonly UniformPadding4 pad;
                    public UniformVector4 OrangeTexRect;
                    public UniformVector4 BlueTexRect;
                    public UniformVector4 EffectsTexRect;
                }
            }
        }
    }
}
