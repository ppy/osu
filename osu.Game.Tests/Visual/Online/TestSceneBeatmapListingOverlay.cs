// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Overlays;
using NUnit.Framework;
using osu.Game.Online.API.Requests;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapListingOverlay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BeatmapListingOverlay),
        };

        protected override bool UseOnlineAPI => true;

        private readonly BeatmapListingOverlay overlay;

        public TestSceneBeatmapListingOverlay()
        {
            Add(overlay = new BeatmapListingOverlay());
        }

        [Test]
        public void TestShowTag()
        {
            AddStep("Show Rem tag", () => overlay.ShowTag("Rem"));
        }

        [Test]
        public void TestShowGenre()
        {
            AddStep("Show Anime genre", () => overlay.ShowGenre(BeatmapSearchGenre.Anime));
        }

        [Test]
        public void TestShowLanguage()
        {
            AddStep("Show Japanese language", () => overlay.ShowLanguage(BeatmapSearchLanguage.Japanese));
        }

        [Test]
        public void TestShow()
        {
            AddStep("Show", overlay.Show);
        }

        [Test]
        public void TestHide()
        {
            AddStep("Hide", overlay.Hide);
        }
    }
}
