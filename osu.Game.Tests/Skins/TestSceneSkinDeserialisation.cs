// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning;
using osu.Game.Skinning.Components;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

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
    [HeadlessTest]
    public partial class TestSceneSkinDeserialisation : OsuTestScene
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
            // Covers "Argon" accuracy/score/combo counters, and wedges
            "Archives/modified-argon-20231108.osk",
            // Covers "Argon" performance points counter
            "Archives/modified-argon-20240305.osk",
            // Covers default rank display
            "Archives/modified-default-20230809.osk",
            // Covers legacy rank display
            "Archives/modified-classic-20230809.osk",
            // Covers legacy key counter
            "Archives/modified-classic-20240724.osk",
            // Covers skinnable mod display
            "Archives/modified-default-20241207.osk"
        };

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        /// <summary>
        /// If this test fails, new test resources should be added to include new components.
        /// </summary>
        [Test]
        public void TestSkinnableComponentsCoveredByDeserialisationTests()
        {
            HashSet<Type> instantiatedTypes = new HashSet<Type>();

            AddStep("load skin", () =>
            {
                foreach (string oskFile in available_skins)
                {
                    var skin = importSkinFromArchives(oskFile);

                    foreach (var target in skin.LayoutInfos)
                    {
                        foreach (var info in target.Value.AllDrawables)
                            instantiatedTypes.Add(info.Type);
                    }
                }
            });

            var existingTypes = SerialisedDrawableInfo.GetAllAvailableDrawables().Where(t => (Activator.CreateInstance(t) as ISerialisableDrawable)?.IsEditable == true);

            AddAssert("all types available", () => instantiatedTypes, () => Is.EquivalentTo(existingTypes));
        }

        [Test]
        public void TestDeserialiseModifiedDefault()
        {
            Skin skin = null!;

            AddStep("load skin", () => skin = importSkinFromArchives("Archives/modified-default-20220723.osk"));

            AddAssert("layouts count = 2", () => skin.LayoutInfos, () => Has.Count.EqualTo(2));
            AddAssert("hud count = 12",
                () => skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].AllDrawables.ToArray(),
                () => Has.Length.EqualTo(12));
        }

        [Test]
        public void TestDeserialiseModifiedArgon()
        {
            Skin skin = null!;

            AddStep("load skin", () => skin = importSkinFromArchives("Archives/modified-argon-20231106.osk"));

            AddAssert("layouts count = 2", () => skin.LayoutInfos, () => Has.Count.EqualTo(2));
            AddAssert("hud count = 13",
                () => skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].AllDrawables.ToArray(),
                () => Has.Length.EqualTo(13));

            AddAssert("hud contains player name",
                () => skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].AllDrawables.Select(i => i.Type),
                () => Does.Contain(typeof(PlayerName)));
        }

        [Test]
        public void TestDeserialiseInvalidDrawables()
        {
            Skin skin = null!;

            AddStep("load skin", () => skin = importSkinFromArchives("Archives/argon-invalid-drawable.osk"));

            AddAssert("skin does not contain star fountain",
                () => skin.LayoutInfos.SelectMany(kvp => kvp.Value.AllDrawables).Select(d => d.Type),
                () => Does.Not.Contain(typeof(StarFountain)));
        }

        [Test]
        public void TestDeserialiseModifiedClassic()
        {
            Skin skin = null!;

            AddStep("load skin", () => skin = importSkinFromArchives("Archives/modified-classic-20220723.osk"));

            AddAssert("layouts count = 2", () => skin.LayoutInfos, () => Has.Count.EqualTo(2));
            AddAssert("hud count = 11",
                () => skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].AllDrawables.ToArray(),
                () => Has.Length.EqualTo(11));

            AddAssert("song select count = 1",
                () => skin.LayoutInfos[GlobalSkinnableContainers.SongSelect].AllDrawables.ToArray(),
                () => Has.Length.EqualTo(1));

            AddAssert("song select component correct", () =>
            {
                var skinnableInfo = skin.LayoutInfos[GlobalSkinnableContainers.SongSelect].AllDrawables.First();

                Assert.That(skinnableInfo.Type, Is.EqualTo(typeof(SkinnableSprite)));
                Assert.That(skinnableInfo.Settings.First().Key, Is.EqualTo("sprite_name"));
                Assert.That(skinnableInfo.Settings.First().Value, Is.EqualTo("ppy_logo-2.png"));
                return true;
            });

            AddStep("load another skin", () => skin = importSkinFromArchives("Archives/modified-classic-20220801.osk"));

            AddAssert("hud count = 13",
                () => skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].AllDrawables.ToArray(),
                () => Has.Length.EqualTo(13));

            AddAssert("hud contains ur counter",
                () => skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].AllDrawables.Select(i => i.Type),
                () => Does.Contain(typeof(UnstableRateCounter)));

            AddAssert("hud contains colour hit error",
                () => skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].AllDrawables.Select(i => i.Type),
                () => Does.Contain(typeof(ColourHitErrorMeter)));

            AddAssert("hud contains legacy song progress",
                () => skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].AllDrawables.Select(i => i.Type),
                () => Does.Contain(typeof(LegacySongProgress)));
        }

        private Skin importSkinFromArchives(string filename)
        {
            var imported = skins.Import(new ImportTask(TestResources.OpenResource(filename), Path.GetFileNameWithoutExtension(filename))).GetResultSafely();
            return imported.PerformRead(skinInfo => skins.GetSkin(skinInfo));
        }
    }
}
