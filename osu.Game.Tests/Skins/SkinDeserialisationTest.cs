// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Audio;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
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
        private static readonly string[] available_skins =
        {
            // Covers song progress before namespace changes, and most other components.
            "Archives/modified-default-20220723.osk",
            "Archives/modified-classic-20220723.osk",
            // Covers legacy song progress, UR counter, colour hit error metre.
            "Archives/modified-classic-20220801.osk",
            // Covers clicks/s counter
            "Archives/modified-default-20220818.osk",
            // Covers longest combo counter
            "Archives/modified-default-20221012.osk",
            // Covers Argon variant of song progress bar
            "Archives/modified-argon-20221024.osk",
            // Covers TextElement and BeatmapInfoDrawable
            "Archives/modified-default-20221102.osk",
            // Covers BPM counter.
            "Archives/modified-default-20221205.osk",
            // Covers judgement counter.
            "Archives/modified-default-20230117.osk",
            // Covers player avatar and flag.
            "Archives/modified-argon-20230305.osk",
            // Covers key counters
            "Archives/modified-argon-pro-20230618.osk",
            // Covers "Argon" health display
            "Archives/modified-argon-pro-20231001.osk",
            // Covers player name text component.
            "Archives/modified-argon-20231106.osk",
        };

        /// <summary>
        /// If this test fails, new test resources should be added to include new components.
        /// </summary>
        [Test]
        public void TestSkinnableComponentsCoveredByDeserialisationTests()
        {
            HashSet<Type> instantiatedTypes = new HashSet<Type>();

            foreach (string oskFile in available_skins)
            {
                using (var stream = TestResources.OpenResource(oskFile))
                using (var storage = new ZipArchiveReader(stream))
                {
                    var skin = new TestSkin(new SkinInfo(), null, storage);

                    foreach (var target in skin.LayoutInfos)
                    {
                        foreach (var info in target.Value.AllDrawables)
                            instantiatedTypes.Add(info.Type);
                    }
                }
            }

            var editableTypes = SerialisedDrawableInfo.GetAllAvailableDrawables().Where(t => (Activator.CreateInstance(t) as ISerialisableDrawable)?.IsEditable == true);

            Assert.That(instantiatedTypes, Is.EquivalentTo(editableTypes));
        }

        [Test]
        public void TestDeserialiseModifiedDefault()
        {
            using (var stream = TestResources.OpenResource("Archives/modified-default-20220723.osk"))
            using (var storage = new ZipArchiveReader(stream))
            {
                var skin = new TestSkin(new SkinInfo(), null, storage);

                Assert.That(skin.LayoutInfos, Has.Count.EqualTo(2));
                Assert.That(skin.LayoutInfos[SkinComponentsContainerLookup.TargetArea.MainHUDComponents].AllDrawables.ToArray(), Has.Length.EqualTo(9));
            }
        }

        [Test]
        public void TestDeserialiseModifiedClassic()
        {
            using (var stream = TestResources.OpenResource("Archives/modified-classic-20220723.osk"))
            using (var storage = new ZipArchiveReader(stream))
            {
                var skin = new TestSkin(new SkinInfo(), null, storage);

                Assert.That(skin.LayoutInfos, Has.Count.EqualTo(2));
                Assert.That(skin.LayoutInfos[SkinComponentsContainerLookup.TargetArea.MainHUDComponents].AllDrawables.ToArray(), Has.Length.EqualTo(6));
                Assert.That(skin.LayoutInfos[SkinComponentsContainerLookup.TargetArea.SongSelect].AllDrawables.ToArray(), Has.Length.EqualTo(1));

                var skinnableInfo = skin.LayoutInfos[SkinComponentsContainerLookup.TargetArea.SongSelect].AllDrawables.First();

                Assert.That(skinnableInfo.Type, Is.EqualTo(typeof(SkinnableSprite)));
                Assert.That(skinnableInfo.Settings.First().Key, Is.EqualTo("sprite_name"));
                Assert.That(skinnableInfo.Settings.First().Value, Is.EqualTo("ppy_logo-2.png"));
            }

            using (var stream = TestResources.OpenResource("Archives/modified-classic-20220801.osk"))
            using (var storage = new ZipArchiveReader(stream))
            {
                var skin = new TestSkin(new SkinInfo(), null, storage);
                Assert.That(skin.LayoutInfos[SkinComponentsContainerLookup.TargetArea.MainHUDComponents].AllDrawables.ToArray(), Has.Length.EqualTo(8));
                Assert.That(skin.LayoutInfos[SkinComponentsContainerLookup.TargetArea.MainHUDComponents].AllDrawables.Select(i => i.Type), Contains.Item(typeof(UnstableRateCounter)));
                Assert.That(skin.LayoutInfos[SkinComponentsContainerLookup.TargetArea.MainHUDComponents].AllDrawables.Select(i => i.Type), Contains.Item(typeof(ColourHitErrorMeter)));
                Assert.That(skin.LayoutInfos[SkinComponentsContainerLookup.TargetArea.MainHUDComponents].AllDrawables.Select(i => i.Type), Contains.Item(typeof(LegacySongProgress)));
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
