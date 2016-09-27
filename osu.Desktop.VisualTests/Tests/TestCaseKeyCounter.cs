//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Desktop.Tests
{
    class TestCaseKeyCounter : TestCase
    {
        public override string Name => @"KeyCounter";

        public override string Description => @"Key counter test";

        KeyCounter kc;

        public override void Reset()
        {
            base.Reset();
            Add(
                kc = new KeyCounter
                {
                    Position = new Vector2(50, 50)
                }
            );
            kc.AddKey(new KeyCount(@"Z", Key.Z));
            kc.AddKey(new KeyCount(@"X", Key.X));
            kc.AddKey(new MouseCount(@"M1", MouseButton.Left));
            kc.AddKey(new MouseCount(@"M2", MouseButton.Right));
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            kc.TriggerMouseDown(state, args);
            return false;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            kc.TriggerMouseUp(state, args);
            return false;
        }
    }
}
