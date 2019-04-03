// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osuTK;

namespace osu.Game.Tests.Visual
{
    public class TestCasePositionAxes : OsuTestCase
    {
        private Box box;

        public TestCasePositionAxes()
        {
            Add(new Container()
            {
                Size = new Vector2(1),
                Child = box = new Box
                {
                    Size = new Vector2(25),
                    RelativePositionAxes = Axes.None,
                    Position = new Vector2(250)
                }
            });

            AddStep("blank", () => { });
            AddStep("change axes", () => box.RelativePositionAxes = Axes.Both);
        }
    }
}
