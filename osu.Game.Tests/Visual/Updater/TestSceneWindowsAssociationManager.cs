// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Platform;
using osu.Game.Graphics.Sprites;
using osu.Game.Tests.Resources;
using osu.Game.Updater;

namespace osu.Game.Tests.Visual.Updater
{
    [SupportedOSPlatform("windows")]
    [Ignore("These tests modify the windows registry and open programs")]
    public partial class TestSceneWindowsAssociationManager : OsuTestScene
    {
        private static readonly string exe_path = Path.ChangeExtension(typeof(TestSceneWindowsAssociationManager).Assembly.Location, ".exe");

        [Resolved]
        private GameHost host { get; set; } = null!;

        private readonly WindowsAssociationManager associationManager;

        public TestSceneWindowsAssociationManager()
        {
            Children = new Drawable[]
            {
                new OsuSpriteText { Text = Environment.CommandLine },
                associationManager = new WindowsAssociationManager(exe_path, "osu.Test"),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Environment.CommandLine.Contains(".osz", StringComparison.Ordinal))
                ChangeBackgroundColour(ColourInfo.SingleColour(Colour4.DarkOliveGreen));

            if (Environment.CommandLine.Contains("osu://", StringComparison.Ordinal))
                ChangeBackgroundColour(ColourInfo.SingleColour(Colour4.DarkBlue));

            if (Environment.CommandLine.Contains("osump://", StringComparison.Ordinal))
                ChangeBackgroundColour(ColourInfo.SingleColour(Colour4.DarkRed));
        }

        [Test]
        public void TestInstall()
        {
            AddStep("install", () => associationManager.InstallAssociations());
        }

        [Test]
        public void TestOpenBeatmap()
        {
            string beatmapPath = null!;
            AddStep("create temp beatmap", () => beatmapPath = TestResources.GetTestBeatmapForImport());
            AddAssert("beatmap path ends with .osz", () => beatmapPath, () => Does.EndWith(".osz"));
            AddStep("open beatmap", () => host.OpenFileExternally(beatmapPath));
            AddUntilStep("wait for focus", () => host.IsActive.Value);
            AddStep("delete temp beatmap", () => File.Delete(beatmapPath));
        }

        /// <summary>
        /// To check that the icon is correct
        /// </summary>
        [Test]
        public void TestPresentBeatmap()
        {
            string beatmapPath = null!;
            AddStep("create temp beatmap", () => beatmapPath = TestResources.GetTestBeatmapForImport());
            AddAssert("beatmap path ends with .osz", () => beatmapPath, () => Does.EndWith(".osz"));
            AddStep("show beatmap in explorer", () => host.PresentFileExternally(beatmapPath));
            AddUntilStep("wait for focus", () => host.IsActive.Value);
            AddStep("delete temp beatmap", () => File.Delete(beatmapPath));
        }

        [TestCase("osu://s/1")]
        [TestCase("osump://123")]
        public void TestUrl(string url)
        {
            AddStep($"open {url}", () => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }));
        }

        [Test]
        public void TestUninstall()
        {
            AddStep("uninstall", () => associationManager.UninstallAssociations());
        }

        /// <summary>
        /// Useful when testing things out and manually changing the registry.
        /// </summary>
        [Test]
        public void TestNotifyShell()
        {
            AddStep("notify shell of changes", () => associationManager.NotifyShellUpdate());
        }
    }
}
