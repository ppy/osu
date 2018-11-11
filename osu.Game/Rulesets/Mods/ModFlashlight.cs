// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModFlashlight<T> : Mod, IApplicableToRulesetContainer<T>, IApplicableToScoreProcessor
        where T : HitObject
    {
        public override string Name => "Flashlight";
        public override string ShortenedName => "FL";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_flashlight;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Restricted view area.";
        public override bool Ranked => true;

        public const double FLASHLIGHT_FADE_DURATION = 800;
        protected readonly BindableInt Combo = new BindableInt();

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            Combo.BindTo(scoreProcessor.Combo);
        }

        public virtual void ApplyToRulesetContainer(RulesetContainer<T> rulesetContainer)
        {
            var flashlight = CreateFlashlight();
            flashlight.Combo = Combo;
            flashlight.RelativeSizeAxes = Axes.Both;
            flashlight.Colour = Color4.Black;
            rulesetContainer.KeyBindingInputManager.Add(flashlight);

            flashlight.Breaks = rulesetContainer.Beatmap.Breaks;
        }

        public abstract Flashlight CreateFlashlight();

        public abstract class Flashlight : Drawable
        {
            internal BindableInt Combo;
            private Shader shader;
            protected readonly FlashlightUniformWrapper MousePosWrapper = new FlashlightUniformWrapper();

            protected override DrawNode CreateDrawNode() => new FlashlightDrawNode();

            public override bool RemoveCompletedTransforms => false;

            public List<BreakPeriod> Breaks;

            protected override void ApplyDrawNode(DrawNode node)
            {
                base.ApplyDrawNode(node);

                var flashNode = (FlashlightDrawNode)node;

                flashNode.Shader = shader;
                flashNode.ScreenSpaceDrawQuad = ScreenSpaceDrawQuad;
                flashNode.MousePosWrapper = MousePosWrapper;
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaderManager)
            {
                shader = shaderManager.Load("PositionAndColour", "Flashlight");
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Combo.ValueChanged += OnComboChange;

                this.FadeInFromZero(FLASHLIGHT_FADE_DURATION);

                foreach (var breakPeriod in Breaks)
                {
                    this.Delay(breakPeriod.StartTime + FLASHLIGHT_FADE_DURATION).FadeOutFromOne(FLASHLIGHT_FADE_DURATION);
                    this.Delay(breakPeriod.EndTime - FLASHLIGHT_FADE_DURATION).FadeInFromZero(FLASHLIGHT_FADE_DURATION);
                }
            }

            protected abstract void OnComboChange(int newCombo);
        }

        public class FlashlightUniformWrapper
        {
            public bool Rectangular;
            public bool RectangularChanged = true;

            public Vector2 FlashlightPosition;
            public bool FlashlightPositionChanged = true;

            public float CircularFlashlightSize;
            public bool CircularFlashlightSizeChanged = true;

            public Vector2 RectangularFlashlightSize;
            public bool RectangularFlashlightSizeChanged = true;
        }

        private class FlashlightDrawNode : DrawNode
        {
            public Shader Shader;
            public Quad ScreenSpaceDrawQuad;
            public FlashlightUniformWrapper MousePosWrapper;

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                Shader.Bind();

                if (MousePosWrapper.RectangularChanged)
                {
                    Shader.GetUniform<bool>("rectangular").UpdateValue(ref MousePosWrapper.Rectangular);
                    MousePosWrapper.RectangularChanged = false;
                }

                if (MousePosWrapper.FlashlightPositionChanged)
                {
                    Shader.GetUniform<Vector2>("flashlightPos").UpdateValue(ref MousePosWrapper.FlashlightPosition);
                    MousePosWrapper.FlashlightPositionChanged = false;
                }

                if (MousePosWrapper.CircularFlashlightSizeChanged)
                {
                    Shader.GetUniform<float>("circularFlashlightSize").UpdateValue(ref MousePosWrapper.CircularFlashlightSize);
                    MousePosWrapper.CircularFlashlightSizeChanged = false;
                }

                if (MousePosWrapper.RectangularFlashlightSizeChanged)
                {
                    Shader.GetUniform<Vector2>("rectangularFlashlightSize").UpdateValue(ref MousePosWrapper.RectangularFlashlightSize);
                    MousePosWrapper.RectangularFlashlightSizeChanged = false;
                }

                Texture.WhitePixel.DrawQuad(ScreenSpaceDrawQuad, DrawColourInfo.Colour, vertexAction: vertexAction);

                Shader.Unbind();
            }
        }
    }
}
