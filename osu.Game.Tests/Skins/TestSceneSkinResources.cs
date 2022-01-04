// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.IO.Archives;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Skins
{
    [HeadlessTest]
    public class TestSceneSkinResources : OsuTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; }

        private ISkin skin;

        [BackgroundDependencyLoader]
        private void load()
        {
            var imported = skins.Import(new ZipArchiveReader(TestResources.OpenResource("Archives/ogg-skin.osk"))).Result;
            skin = imported.PerformRead(skinInfo => skins.GetSkin(skinInfo));
        }

        [Test]
        public void TestRetrieveOggSample() => AddAssert("sample is non-null", () => skin.GetSample(new SampleInfo("sample")) != null);
    }
}
