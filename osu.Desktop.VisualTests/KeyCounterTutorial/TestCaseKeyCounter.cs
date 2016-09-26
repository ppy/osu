using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.GameModes.Testing;
using osu.Game;
using OpenTK.Input;

namespace osu.Desktop.KeyCounterTutorial
{
    class TestCaseKeyCounter : OsuGameBase
    {
        public override void Load()
        {
            base.Load();

            Add(new TestBrowser());
            var kc = new KeyCounter();
            kc.AddKey(new KeyboardCount(@"Z", Key.Z));
            kc.AddKey(new KeyboardCount(@"X", Key.X));
            kc.AddKey(new MouseCount(@"M1", MouseButton.Left));
            kc.AddKey(new MouseCount(@"M2", MouseButton.Right));
            Add(kc);

            ShowPerformanceOverlay = true;
        }
    }
}
