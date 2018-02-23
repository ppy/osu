// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Tests.Visual;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseNewSliderBody : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Path) };

        private readonly NewSliderBody body;

        public TestCaseNewSliderBody()
        {
            Add(body = new NewSliderBody(new SliderCurve
            {
                ControlPoints = new List<Vector2>
                {
                    new Vector2(-200, 0),
                    new Vector2(-50, 75),
                    new Vector2(0, 100),
                    new Vector2(100, -200),
                    new Vector2(230, 0)
                },
                Distance = 480,
                CurveType = CurveType.Bezier
            })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            AddSliderStep("In", 0f, 1f, 0f, v => inLength = v);
            AddSliderStep("Out", 0f, 1f, 1f, v => outLength = v);
            AddSliderStep("Path Width", 0f, 100f, 10f, v => body.PathWidth = v);
        }

        private float _inLength;

        private float inLength
        {
            set
            {
                _inLength = value;
                body.UpdateSnaking(_inLength, _outLength);
            }
        }

        private float _outLength;

        private float outLength
        {
            set
            {
                _outLength = value;
                body.UpdateSnaking(_inLength, _outLength);
            }
        }

        private class NewSliderBody : CompositeDrawable
        {
            private readonly Path path;
            private readonly SliderCurve curve;

            public NewSliderBody(SliderCurve curve)
            {
                this.curve = curve;

                InternalChild = path = new Path();
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                reloadTexture();
                computeSize();
            }

            public float PathWidth
            {
                get => path.PathWidth;
                set { path.PathWidth = value; reloadTexture(); }
            }

            private void reloadTexture()
            {
                var textureWidth = (int)PathWidth * 2;

                //initialise background
                var texture = new Texture(textureWidth, 1);
                var upload = new TextureUpload(textureWidth * 4);
                var bytes = upload.Data;

                const float aa_portion = 0.02f;
                const float border_portion = 0.128f;
                const float gradient_portion = 1 - border_portion;

                const float opacity_at_centre = 0.3f;
                const float opacity_at_edge = 0.8f;

                for (int i = 0; i < textureWidth; i++)
                {
                    float progress = (float)i / (textureWidth - 1);

                    if (progress <= border_portion)
                    {
                        bytes[i * 4] = (byte)(Color4.White.R * 255);
                        bytes[i * 4 + 1] = (byte)(Color4.White.G * 255);
                        bytes[i * 4 + 2] = (byte)(Color4.White.B * 255);
                        bytes[i * 4 + 3] = (byte)(Math.Min(progress / aa_portion, 1) * (Color4.White.A * 255));
                    }
                    else
                    {
                        progress -= border_portion;

                        bytes[i * 4] = (byte)(Color4.Blue.R * 255);
                        bytes[i * 4 + 1] = (byte)(Color4.Blue.G * 255);
                        bytes[i * 4 + 2] = (byte)(Color4.Blue.B * 255);
                        bytes[i * 4 + 3] = (byte)((opacity_at_edge - (opacity_at_edge - opacity_at_centre) * progress / gradient_portion) * (Color4.Blue.A * 255));
                    }
                }

                texture.SetData(upload);
                path.Texture = texture;
            }

            private Vector2 topLeftOffset;

            private void computeSize()
            {
                // Compute the final size
                var fullPath = new List<Vector2>();
                curve.GetPathToProgress(fullPath, 0, 1);

                foreach (Vector2 p in fullPath)
                    path.AddVertex(p);

                Size = path.Size;

                topLeftOffset = path.PositionInBoundingBox(Vector2.Zero);
                OriginPosition = path.PositionInBoundingBox(fullPath[0]);
            }

            public void UpdateSnaking(float t0, float t1)
            {
                var curvePath = new List<Vector2>();
                curve.GetPathToProgress(curvePath, t0, t1);

                path.ClearVertices();
                foreach (Vector2 p in curvePath)
                    path.AddVertex(p);

                var newTopLeftOffset = path.PositionInBoundingBox(Vector2.Zero);
                path.Position = topLeftOffset - newTopLeftOffset;
            }
        }
    }
}
