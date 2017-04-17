// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Desktop.VisualTests;
using osu.Framework.Desktop.Platform;

namespace osu.Desktop.Tests
{
    [TestFixture]
    public class VisualTests
    {
        [Test]
        public void TestVisualTests()
        {
            using (var host = new HeadlessGameHost())
            {
                host.Run(new AutomatedVisualTestGame());
            }
        }
    }
}
