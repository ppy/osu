// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Tests.Visual;
using osu.Game.Tests.Visual.Metadata;

namespace osu.Game.Tests.Online
{
    [TestFixture]
    [HeadlessTest]
    public partial class TestSceneMetadataClient : OsuTestScene
    {
        private TestMetadataClient client = null!;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = client = new TestMetadataClient();
        });

        [Test]
        public void TestWatchingMultipleTimesInvokesServerMethodsOnce()
        {
            int countBegin = 0;
            int countEnd = 0;

            IDisposable token1 = null!;
            IDisposable token2 = null!;

            AddStep("setup", () =>
            {
                client.OnBeginWatchingUserPresence += () => countBegin++;
                client.OnEndWatchingUserPresence += () => countEnd++;
            });

            AddStep("begin watching presence (1)", () => token1 = client.BeginWatchingUserPresence());
            AddAssert("server method invoked once", () => countBegin, () => Is.EqualTo(1));

            AddStep("begin watching presence (2)", () => token2 = client.BeginWatchingUserPresence());
            AddAssert("server method not invoked a second time", () => countBegin, () => Is.EqualTo(1));

            AddStep("end watching presence (1)", () => token1.Dispose());
            AddAssert("server method not invoked", () => countEnd, () => Is.EqualTo(0));

            AddStep("end watching presence (2)", () => token2.Dispose());
            AddAssert("server method invoked once", () => countEnd, () => Is.EqualTo(1));
        }
    }
}
