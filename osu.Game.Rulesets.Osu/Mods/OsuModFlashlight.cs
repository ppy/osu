// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModFlashlight : ModFlashlight, IApplicableToRulesetContainer<OsuHitObject>
    {
        public override double ScoreMultiplier => 1.12;

        public void ApplyToRulesetContainer(RulesetContainer<OsuHitObject> rulesetContainer)
        {
            rulesetContainer.KeyBindingInputManager.Add(new Flashlight
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        private class Flashlight : Drawable, IRequireHighFrequencyMousePosition
        {
            private Shader shader;
            private readonly MousePositionWrapper mousePosWrapper = new MousePositionWrapper
            {
                FlashlightSize = 300f
            };

            protected override DrawNode CreateDrawNode() => new FlashlightDrawNode();

            protected override void ApplyDrawNode(DrawNode node)
            {
                base.ApplyDrawNode(node);

                var flashNode = (FlashlightDrawNode)node;

                flashNode.Shader = shader;
                flashNode.ScreenSpaceDrawQuad = ScreenSpaceDrawQuad;
                flashNode.MousePosWrapper = mousePosWrapper;
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaderManager)
            {
                shader = shaderManager.Load(VertexShaderDescriptor.POSITION, "Flashlight");
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                mousePosWrapper.MousePosition = e.ScreenSpaceMousePosition;
                return base.OnMouseMove(e);
            }
        }

        private class MousePositionWrapper
        {
            public Vector2 MousePosition;
            public float FlashlightSize;
            public bool FlashlightUniformUpdated;
        }

        private class FlashlightDrawNode : DrawNode
        {
            public Shader Shader;
            public Quad ScreenSpaceDrawQuad;
            public MousePositionWrapper MousePosWrapper;

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                Shader.Bind();
                // ReSharper disable once AssignmentInConditionalExpression
                if(MousePosWrapper.FlashlightUniformUpdated = !MousePosWrapper.FlashlightUniformUpdated)
                    Shader.GetUniform<float>("flashlightSize").UpdateValue(ref MousePosWrapper.FlashlightSize);

                Shader.GetUniform<Vector2>("mousePos").UpdateValue(ref MousePosWrapper.MousePosition);

                Texture.WhitePixel.DrawQuad(ScreenSpaceDrawQuad, DrawColourInfo.Colour, vertexAction: vertexAction);

                Shader.Unbind();
            }
        }
    }
}
