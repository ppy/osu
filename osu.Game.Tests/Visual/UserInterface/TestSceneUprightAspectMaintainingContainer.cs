// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK.Graphics;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneUprightAspectMaintainingContainer : OsuGridTestScene
    {
        private const int rows = 3;
        private const int columns = 4;

        private readonly ScaleMode[] scaleModeValues = { ScaleMode.NoScaling, ScaleMode.Horizontal, ScaleMode.Vertical };
        private readonly float[] scalingFactorValues = { 1.0f / 3, 1.0f / 2, 1.0f, 1.5f };

        private readonly List<List<Container>> parentContainers = new List<List<Container>>(rows);
        private readonly List<List<UprightAspectMaintainingContainer>> childContainers = new List<List<UprightAspectMaintainingContainer>>(rows);

        // Preferably should be set to (4 * 2^n)
        private const int rotation_step_count = 3;

        private readonly List<int> flipStates = new List<int>();
        private readonly List<float> rotationSteps = new List<float>();
        private readonly List<float> scaleSteps = new List<float>();

        public TestSceneUprightAspectMaintainingContainer()
            : base(rows, columns)
        {
            for (int i = 0; i < rows; i++)
            {
                parentContainers.Add(new List<Container>());
                childContainers.Add(new List<UprightAspectMaintainingContainer>());

                for (int j = 0; j < columns; j++)
                {
                    UprightAspectMaintainingContainer child;
                    Container parent = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Height = 80,
                        Width = 80,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = new Color4(255, 0, 0, 160),
                            },
                            new OsuSpriteText
                            {
                                Text = "Parent",
                            },
                            child = new UprightAspectMaintainingContainer
                            {
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                AutoSizeAxes = Axes.Both,

                                // These are the parameters being Tested
                                Scaling = scaleModeValues[i],
                                ScalingFactor = scalingFactorValues[j],

                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = new Color4(0, 0, 255, 160),
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = "Text",
                                        Font = OsuFont.Numeric,
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                        Padding = new MarginPadding
                                        {
                                            Horizontal = 4,
                                            Vertical = 4,
                                        }
                                    },
                                }
                            }
                        }
                    };

                    Container cellInfo = new Container
                    {
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "Scaling: " + scaleModeValues[i].ToString(),
                            },
                            new OsuSpriteText
                            {
                                Text = "ScalingFactor: " + scalingFactorValues[j].ToString("0.00"),
                                Margin = new MarginPadding
                                {
                                    Top = 15,
                                },
                            },
                        },
                    };

                    Cell(i * columns + j).Add(cellInfo);
                    Cell(i * columns + j).Add(parent);
                    parentContainers[i].Add(parent);
                    childContainers[i].Add(child);
                }
            }

            flipStates.AddRange(new[] { 1, -1 });
            rotationSteps.AddRange(Enumerable.Range(0, rotation_step_count).Select(x => 360f * ((float)x / rotation_step_count)));
            scaleSteps.AddRange(new[] { 1, 0.3f, 1.5f });
        }

        [Test]
        public void ExplicitlySizedParent()
        {
            var parentStates = from xFlip in flipStates
                               from yFlip in flipStates
                               from xScale in scaleSteps
                               from yScale in scaleSteps
                               from rotation in rotationSteps
                               select new { xFlip, yFlip, xScale, yScale, rotation };

            foreach (var state in parentStates)
            {
                Vector2 parentScale = new Vector2(state.xFlip * state.xScale, state.yFlip * state.yScale);
                float parentRotation = state.rotation;

                AddStep("S: (" + parentScale.X.ToString("0.00") + ", " + parentScale.Y.ToString("0.00") + "), R: " + parentRotation.ToString("0.00"), () =>
                {
                    foreach (List<Container> list in parentContainers)
                    {
                        foreach (Container container in list)
                        {
                            container.Scale = parentScale;
                            container.Rotation = parentRotation;
                        }
                    }
                });

                AddAssert("Check if state is valid", () =>
                {
                    foreach (int i in Enumerable.Range(0, parentContainers.Count))
                    {
                        foreach (int j in Enumerable.Range(0, parentContainers[i].Count))
                        {
                            if (!uprightAspectMaintainingContainerStateIsValid(parentContainers[i][j], childContainers[i][j]))
                                return false;
                        }
                    }

                    return true;
                });
            }
        }

        private bool uprightAspectMaintainingContainerStateIsValid(Container parent, UprightAspectMaintainingContainer child)
        {
            Matrix3 parentMatrix = parent.DrawInfo.Matrix;
            Matrix3 childMatrix = child.DrawInfo.Matrix;
            Vector3 childScale = childMatrix.ExtractScale();
            Vector3 parentScale = parentMatrix.ExtractScale();

            // Orientation check
            if (!(isNearlyZero(MathF.Abs(childMatrix.M21)) && isNearlyZero(MathF.Abs(childMatrix.M12))))
                return false;

            // flip check
            if (!(childMatrix.M11 * childMatrix.M22 > 0))
                return false;

            // Aspect ratio check
            if (!isNearlyZero(childScale.X - childScale.Y))
                return false;

            // ScalingMode check
            switch (child.Scaling)
            {
                case ScaleMode.NoScaling:
                    if (!(isNearlyZero(childMatrix.M11 - 1.0f) && isNearlyZero(childMatrix.M22 - 1.0f)))
                        return false;

                    break;

                case ScaleMode.Vertical:
                    if (!(checkScaling(child.ScalingFactor, parentScale.Y, childScale.Y)))
                        return false;

                    break;

                case ScaleMode.Horizontal:
                    if (!(checkScaling(child.ScalingFactor, parentScale.X, childScale.X)))
                        return false;

                    break;
            }

            return true;
        }

        private bool checkScaling(float scalingFactor, float parentScale, float childScale)
        {
            if (scalingFactor <= 1.0f)
            {
                if (!isNearlyZero(1.0f + (parentScale - 1.0f) * scalingFactor - childScale))
                    return false;
            }
            else if (scalingFactor > 1.0f)
            {
                if (parentScale < 1.0f)
                {
                    if (!isNearlyZero((parentScale * (1.0f / scalingFactor)) - childScale))
                        return false;
                }
                else if (!isNearlyZero(parentScale * scalingFactor - childScale))
                    return false;
            }

            return true;
        }

        private bool isNearlyZero(float f, float epsilon = Precision.FLOAT_EPSILON)
        {
            return f < epsilon;
        }
    }
}
