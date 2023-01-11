// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using Vector4 = System.Numerics.Vector4;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneSegmentedGraph : OsuTestScene
    {
        private readonly SegmentedGraph<int> graph;

        public TestSceneSegmentedGraph()
        {
            Children = new Drawable[]
            {
                graph = new SegmentedGraph<int>(10)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(1, 0.5f),
                },
            };

            for (int i = 0; i < graph.TierColours.Length; i++)
            {
                graph.TierColours[i] =
                    new Colour4(Vector4.Lerp(Colour4.Blue.Vector, Colour4.White.Vector, i * 1f / (graph.TierColours.Length - 1)));
            }

            AddStep("values from 1-10", () => graph.Values = Enumerable.Range(1, 10).ToArray());
            AddStep("values from 1-100", () => graph.Values = Enumerable.Range(1, 100).ToArray());
            AddStep("values from 1-500", () => graph.Values = Enumerable.Range(1, 500).ToArray());
            AddStep("sin() function of size 100", () => sinFunction());
            AddStep("sin() function of size 500", () => sinFunction(500));
            AddStep("bumps of size 100", () => bumps());
            AddStep("bumps of size 500", () => bumps(500));
            AddStep("100 random values", () => randomValues());
            AddStep("500 random values", () => randomValues(500));
            AddStep("reversed values from 1-10", () => graph.Values = Enumerable.Range(1, 10).Reverse().ToArray());
        }

        private void sinFunction(int size = 100)
        {
            const int max_value = 255;
            int[] values = new int[size];

            float step = 2 * MathF.PI / size;
            float x = 0;

            for (int i = 0; i < size; i++)
            {
                values[i] = (int)(max_value * MathF.Sin(x));
                x += step;
            }

            graph.Values = values;
        }

        private void bumps(int size = 100)
        {
            const int max_value = 255;
            int[] values = new int[size];

            float step = 2 * MathF.PI / size;
            float x = 0;

            for (int i = 0; i < size; i++)
            {
                values[i] = (int)(max_value * Math.Abs(MathF.Sin(x)));
                x += step;
            }

            graph.Values = values;
        }

        private void randomValues(int size = 100)
        {
            Random rng = new Random();

            int[] values = new int[size];

            for (int i = 0; i < size; i++)
            {
                values[i] = rng.Next(255);
            }

            graph.Values = values;
        }
    }
}
