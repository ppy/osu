using OpenTK;
using OpenTK.Graphics;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Desktop.Tests
{
    class TestCaseTextAwesome : TestCase
    {
        public override string Name => @"TextAwesome";

        public override string Description => @"Tests display of icons";

        public override void Reset()
        {
            base.Reset();

            FlowContainer flow;

            Add(flow = new FlowContainer()
            {
                SizeMode = InheritMode.XY,
                Size = new Vector2(0.5f),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            int i = 50;
            foreach (FontAwesome fa in Enum.GetValues(typeof(FontAwesome)))
            {
                flow.Add(new TextAwesome
                {
                    Icon = fa,
                    TextSize = 60,
                    Colour = new Color4(
                        Math.Max(0.5f, RNG.NextSingle()),
                        Math.Max(0.5f, RNG.NextSingle()),
                        Math.Max(0.5f, RNG.NextSingle()),
                        1)
                });

                if (i-- == 0) break;
            }
        }
    }
}
