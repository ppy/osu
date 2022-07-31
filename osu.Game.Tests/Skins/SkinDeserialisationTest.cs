// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Audio;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Skins
{
    /// <summary>
    /// Test that the main components (which are serialised based on namespace/class name)
    /// remain compatible with any changes.
    /// </summary>
    /// <remarks>
    /// If this test breaks, check any naming or class structure changes.
    /// Migration rules may need to be added to <see cref="Skin"/>.
    /// </remarks>
    [TestFixture]
    public class SkinDeserialisationTest
    {
        [Test]
        public void TestDeserialiseModifiedDefault()
        {
            using (var stream = TestResources.OpenResource("Archives/modified-default-20220723.osk"))
            using (var storage = new ZipArchiveReader(stream))
            {
                var skin = new TestSkin(new SkinInfo(), null, storage);

                Assert.That(skin.DrawableComponentInfo, Has.Count.EqualTo(2));
                Assert.That(skin.DrawableComponentInfo[SkinnableTarget.MainHUDComponents], Has.Length.EqualTo(9));
            }
        }

        [Test]
        public void TestDeserialiseModifiedClassic()
        {
            using (var stream = TestResources.OpenResource("Archives/modified-classic-20220723.osk"))
            using (var storage = new ZipArchiveReader(stream))
            {
                var skin = new TestSkin(new SkinInfo(), null, storage);

                Assert.That(skin.DrawableComponentInfo, Has.Count.EqualTo(2));
                Assert.That(skin.DrawableComponentInfo[SkinnableTarget.MainHUDComponents], Has.Length.EqualTo(6));
                Assert.That(skin.DrawableComponentInfo[SkinnableTarget.SongSelect], Has.Length.EqualTo(1));

                var skinnableInfo = skin.DrawableComponentInfo[SkinnableTarget.SongSelect].First();

                Assert.That(skinnableInfo.Type, Is.EqualTo(typeof(SkinnableSprite)));
                Assert.That(skinnableInfo.Settings.First().Key, Is.EqualTo("sprite_name"));
                Assert.That(skinnableInfo.Settings.First().Value, Is.EqualTo("ppy_logo-2.png"));
            }
        }

        private class TestSkin : Skin
        {
            public TestSkin(SkinInfo skin, IStorageResourceProvider? resources, IResourceStore<byte[]>? storage = null, string configurationFilename = "skin.ini")
                : base(skin, resources, storage, configurationFilename)
            {
            }

            public override Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => throw new NotImplementedException();

            public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => throw new NotImplementedException();

            public override ISample GetSample(ISampleInfo sampleInfo) => throw new NotImplementedException();
        }
    }
}
