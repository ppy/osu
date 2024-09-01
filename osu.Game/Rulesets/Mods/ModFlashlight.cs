// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
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
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.OpenGL.Vertices;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModFlashlight : Mod
    {
        public override string Name => "Flashlight";
        public override string Acronym => "FL";
        public override IconUsage? Icon => OsuIcon.ModFlashlight;
        public override ModType Type => ModType.DifficultyIncrease;
        public override LocalisableString Description => "Restricted view area.";
        public override bool Ranked => UsesDefaultConfiguration;

        [SettingSource("Flashlight size", "Multiplier applied to the default flashlight size.")]
        public abstract BindableFloat SizeMultiplier { get; }

        [SettingSource("Change size based on combo", "Decrease the flashlight size as combo increases.")]
        public abstract BindableBool ComboBasedSize { get; }

        /// <summary>
        /// The default size of the flashlight in ruleset-appropriate dimensions.
        /// <see cref="SizeMultiplier"/> and <see cref="ComboBasedSize"/> will apply their adjustments on top of this size.
        /// </summary>
        public abstract float DefaultFlashlightSize { get; }
    }

    public abstract partial class ModFlashlight<T> : ModFlashlight, IApplicableToDrawableRuleset<T>, IApplicableToScoreProcessor
        where T : HitObject
    {
        public const double FLASHLIGHT_FADE_DURATION = 800;
        protected readonly BindableInt Combo = new BindableInt();

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            Combo.BindTo(scoreProcessor.Combo);
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy)
        {
            switch (rank)
            {
                case ScoreRank.X:
                    return ScoreRank.XH;

                case ScoreRank.S:
                    return ScoreRank.SH;

                default:
                    return rank;
            }
        }

        public virtual void ApplyToDrawableRuleset(DrawableRuleset<T> drawableRuleset)
        {
            var flashlight = CreateFlashlight();

            flashlight.RelativeSizeAxes = Axes.Both;
            flashlight.Colour = Color4.Black;
            // Flashlight mods should always draw above any other mod adding overlays.
            flashlight.Depth = float.MinValue;

            flashlight.Combo.BindTo(Combo);
            flashlight.GetPlayfieldScale = () => drawableRuleset.Playfield.Scale;

            drawableRuleset.Overlays.Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                // workaround for 1px gaps on the edges of the playfield which would sometimes show with "gameplay" screen scaling active.
                Padding = new MarginPadding(-1),
                Child = flashlight,
            });
        }

        protected abstract Flashlight CreateFlashlight();

        public abstract partial class Flashlight : Drawable
        {
            public readonly BindableInt Combo = new BindableInt();

            private IShader shader = null!;

            protected override DrawNode CreateDrawNode() => new FlashlightDrawNode(this);

            public override bool RemoveCompletedTransforms => false;

            internal Func<Vector2>? GetPlayfieldScale;

            private readonly float defaultFlashlightSize;
            private readonly float sizeMultiplier;
            private readonly bool comboBasedSize;

            protected Flashlight(ModFlashlight modFlashlight)
            {
                defaultFlashlightSize = modFlashlight.DefaultFlashlightSize;
                sizeMultiplier = modFlashlight.SizeMultiplier.Value;
                comboBasedSize = modFlashlight.ComboBasedSize.Value;
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaderManager)
            {
                shader = shaderManager.Load("PositionAndColour", FragmentShader);
            }

            [Resolved]
            private Player? player { get; set; }

            private readonly IBindable<bool> isBreakTime = new BindableBool();

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Combo.ValueChanged += _ => UpdateFlashlightSize(GetSize());

                if (player != null)
                {
                    isBreakTime.BindTo(player.IsBreakTime);
                    isBreakTime.BindValueChanged(_ => UpdateFlashlightSize(GetSize()), true);
                }
            }

            protected abstract void UpdateFlashlightSize(float size);

            protected abstract string FragmentShader { get; }

            public float GetSize()
            {
                float size = defaultFlashlightSize * sizeMultiplier;

                if (GetPlayfieldScale != null)
                {
                    Vector2 playfieldScale = GetPlayfieldScale();

                    Debug.Assert(Precision.AlmostEquals(Math.Abs(playfieldScale.X), Math.Abs(playfieldScale.Y)),
                        @"Playfield has non-proportional scaling. Flashlight implementations should be revisited with regard to balance.");
                    size *= Math.Abs(playfieldScale.X);
                }

                if (isBreakTime.Value)
                    size *= 2.5f;
                else if (comboBasedSize)
                    size *= GetComboScaleFor(Combo.Value);

                return size;
            }

            protected virtual float GetComboScaleFor(int combo)
            {
                if (combo >= 200)
                    return 0.625f;
                if (combo >= 100)
                    return 0.8125f;

                return 1.0f;
            }

            private Vector2 flashlightPosition;

            protected Vector2 FlashlightPosition
            {
                get => flashlightPosition;
                set
                {
                    if (flashlightPosition == value) return;

                    flashlightPosition = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            private Vector2 flashlightSize;

            protected Vector2 FlashlightSize
            {
                get => flashlightSize;
                set
                {
                    if (flashlightSize == value) return;

                    flashlightSize = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            private float flashlightDim;

            public float FlashlightDim
            {
                get => flashlightDim;
                set
                {
                    if (flashlightDim == value) return;

                    flashlightDim = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            private float flashlightSmoothness = 1.1f;

            public float FlashlightSmoothness
            {
                get => flashlightSmoothness;
                set
                {
                    if (flashlightSmoothness == value) return;

                    flashlightSmoothness = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            private class FlashlightDrawNode : DrawNode
            {
                protected new Flashlight Source => (Flashlight)base.Source;

                private IShader shader = null!;
                private Quad screenSpaceDrawQuad;
                private Vector2 flashlightPosition;
                private Vector2 flashlightSize;
                private float flashlightDim;
                private float flashlightSmoothness;

                private IVertexBatch<PositionAndColourVertex>? quadBatch;
                private Action<TexturedVertex2D>? addAction;

                public FlashlightDrawNode(Flashlight source)
                    : base(source)
                {
                }

                public override void ApplyState()
                {
                    base.ApplyState();

                    shader = Source.shader;
                    screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
                    flashlightPosition = Vector2Extensions.Transform(Source.FlashlightPosition, DrawInfo.Matrix);
                    flashlightSize = Source.FlashlightSize * DrawInfo.Matrix.ExtractScale().Xy;
                    flashlightDim = Source.FlashlightDim;
                    flashlightSmoothness = Source.flashlightSmoothness;
                }

                private IUniformBuffer<FlashlightParameters>? flashlightParametersBuffer;

                protected override void Draw(IRenderer renderer)
                {
                    base.Draw(renderer);

                    if (quadBatch == null)
                    {
                        quadBatch = renderer.CreateQuadBatch<PositionAndColourVertex>(1, 1);
                        addAction = v => quadBatch.Add(new PositionAndColourVertex
                        {
                            Position = v.Position,
                            Colour = v.Colour
                        });
                    }

                    flashlightParametersBuffer ??= renderer.CreateUniformBuffer<FlashlightParameters>();
                    flashlightParametersBuffer.Data = flashlightParametersBuffer.Data with
                    {
                        Position = flashlightPosition,
                        Size = flashlightSize,
                        Dim = flashlightDim,
                        Smoothness = flashlightSmoothness
                    };

                    shader.Bind();
                    shader.BindUniformBlock(@"m_FlashlightParameters", flashlightParametersBuffer);

                    renderer.DrawQuad(renderer.WhitePixel, screenSpaceDrawQuad, DrawColourInfo.Colour, vertexAction: addAction);

                    shader.Unbind();
                }

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);
                    quadBatch?.Dispose();
                    flashlightParametersBuffer?.Dispose();
                }

                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                private record struct FlashlightParameters
                {
                    public UniformVector2 Position;
                    public UniformVector2 Size;
                    public UniformFloat Dim;
                    public UniformFloat Smoothness;
                    private readonly UniformPadding8 pad1;
                }
            }
        }
    }
}
