// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.OpenGL.Vertices;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
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
        public override string Description => "Restricted view area.";

        [SettingSource("Flashlight size", "Multiplier applied to the default flashlight size.")]
        public abstract BindableNumber<float> SizeMultiplier { get; }

        [SettingSource("Change size based on combo", "Decrease the flashlight size as combo increases.")]
        public abstract BindableBool ComboBasedSize { get; }

        /// <summary>
        /// The default size of the flashlight in ruleset-appropriate dimensions.
        /// <see cref="SizeMultiplier"/> and <see cref="ComboBasedSize"/> will apply their adjustments on top of this size.
        /// </summary>
        public abstract float DefaultFlashlightSize { get; }
    }

    public abstract class ModFlashlight<T> : ModFlashlight, IApplicableToDrawableRuleset<T>, IApplicableToScoreProcessor
        where T : HitObject
    {
        public const double FLASHLIGHT_FADE_DURATION = 800;
        protected readonly BindableInt Combo = new BindableInt();

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            Combo.BindTo(scoreProcessor.Combo);

            // Default value of ScoreProcessor's Rank in Flashlight Mod should be SS+
            scoreProcessor.Rank.Value = ScoreRank.XH;
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

            flashlight.Combo.BindTo(Combo);
            drawableRuleset.KeyBindingInputManager.Add(flashlight);

            flashlight.Breaks = drawableRuleset.Beatmap.Breaks;
        }

        protected abstract Flashlight CreateFlashlight();

        public abstract class Flashlight : Drawable
        {
            public readonly BindableInt Combo = new BindableInt();

            private IShader shader;

            protected override DrawNode CreateDrawNode() => new FlashlightDrawNode(this);

            public override bool RemoveCompletedTransforms => false;

            public List<BreakPeriod> Breaks;

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

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Combo.ValueChanged += OnComboChange;

                using (BeginAbsoluteSequence(0))
                {
                    foreach (var breakPeriod in Breaks)
                    {
                        if (!breakPeriod.HasEffect)
                            continue;

                        if (breakPeriod.Duration < FLASHLIGHT_FADE_DURATION * 2) continue;

                        this.Delay(breakPeriod.StartTime + FLASHLIGHT_FADE_DURATION).FadeOutFromOne(FLASHLIGHT_FADE_DURATION);
                        this.Delay(breakPeriod.EndTime - FLASHLIGHT_FADE_DURATION).FadeInFromZero(FLASHLIGHT_FADE_DURATION);
                    }
                }
            }

            protected abstract void OnComboChange(ValueChangedEvent<int> e);

            protected abstract string FragmentShader { get; }

            protected float GetSizeFor(int combo)
            {
                float size = defaultFlashlightSize * sizeMultiplier;

                if (comboBasedSize)
                {
                    if (combo > 200)
                        size *= 0.8f;
                    else if (combo > 100)
                        size *= 0.9f;
                }

                return size;
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

            private class FlashlightDrawNode : DrawNode
            {
                protected new Flashlight Source => (Flashlight)base.Source;

                private IShader shader;
                private Quad screenSpaceDrawQuad;
                private Vector2 flashlightPosition;
                private Vector2 flashlightSize;
                private float flashlightDim;

                private readonly VertexBatch<PositionAndColourVertex> quadBatch = new QuadBatch<PositionAndColourVertex>(1, 1);
                private readonly Action<TexturedVertex2D> addAction;

                public FlashlightDrawNode(Flashlight source)
                    : base(source)
                {
                    addAction = v => quadBatch.Add(new PositionAndColourVertex
                    {
                        Position = v.Position,
                        Colour = v.Colour
                    });
                }

                public override void ApplyState()
                {
                    base.ApplyState();

                    shader = Source.shader;
                    screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
                    flashlightPosition = Vector2Extensions.Transform(Source.FlashlightPosition, DrawInfo.Matrix);
                    flashlightSize = Source.FlashlightSize * DrawInfo.Matrix.ExtractScale().Xy;
                    flashlightDim = Source.FlashlightDim;
                }

                public override void Draw(Action<TexturedVertex2D> vertexAction)
                {
                    base.Draw(vertexAction);

                    shader.Bind();

                    shader.GetUniform<Vector2>("flashlightPos").UpdateValue(ref flashlightPosition);
                    shader.GetUniform<Vector2>("flashlightSize").UpdateValue(ref flashlightSize);
                    shader.GetUniform<float>("flashlightDim").UpdateValue(ref flashlightDim);

                    DrawQuad(Texture.WhitePixel, screenSpaceDrawQuad, DrawColourInfo.Colour, vertexAction: addAction);

                    shader.Unbind();
                }

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);
                    quadBatch?.Dispose();
                }
            }
        }
    }
}
