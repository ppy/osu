// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneManualComboColouring : ScreenTestScene
    {
        [Resolved]
        private AudioManager audio { get; set; }

        private OsuConfigManager localConfig;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(localConfig = new OsuConfigManager(LocalStorage));
        }

        [Test]
        public void TestManualColouring()
        {
            TestPlayer player = null;

            AddStep("enable beatmap skin", () => localConfig.Set(OsuSetting.BeatmapSkins, true));
            AddStep("load beatmap", () => player = loadBeatmap());
            AddUntilStep("wait for player", () => player.IsLoaded);

            // First object has first colour (0 % 4 = 0), combo index 0 = (last combo index = 0) *no new combo*
            // Second object has third colour (2 % 4 = 2), combo index 2 = (last combo index = 0) + (combo offset = 1) + (new combo = true = 1) *new combo*
            // Third object has third colour (6 % 4 = 2), combo index 6 = (last combo index = 2) + (combo offset = 3) + (new combo = true = 1) *new combo*
            // Fourth object has first colour (12 % 4 = 0), combo index 12 = (last combo index = 6) + (combo offset = 5) + (new combo = true = 1) *new combo*
            AddAssert("first object has first colour", () => player.DrawableRuleset.Playfield.AllHitObjects.ElementAt(0).AccentColour.Value == TestBeatmapSkin.Colours[0]);
            AddAssert("second object has third colour", () => player.DrawableRuleset.Playfield.AllHitObjects.ElementAt(1).AccentColour.Value == TestBeatmapSkin.Colours[2]);
            AddAssert("third object has third colour", () => player.DrawableRuleset.Playfield.AllHitObjects.ElementAt(2).AccentColour.Value == TestBeatmapSkin.Colours[2]);
            AddAssert("fourth object has first colour", () => player.DrawableRuleset.Playfield.AllHitObjects.ElementAt(3).AccentColour.Value == TestBeatmapSkin.Colours[0]);
        }

        [Test]
        public void TestNoManualColouringOnDisabledBeatmapSkin()
        {
            TestPlayer player = null;

            AddStep("disable beatmap skin", () => localConfig.Set(OsuSetting.BeatmapSkins, false));
            AddStep("load beatmap", () => player = loadBeatmap());
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("first object has first colour", () => player.DrawableRuleset.Playfield.AllHitObjects.ElementAt(0).AccentColour.Value == SkinConfiguration.DefaultComboColours[0]);
            AddAssert("second object has second colour", () => player.DrawableRuleset.Playfield.AllHitObjects.ElementAt(1).AccentColour.Value == SkinConfiguration.DefaultComboColours[1]);
            AddAssert("third object has third colour", () => player.DrawableRuleset.Playfield.AllHitObjects.ElementAt(2).AccentColour.Value == SkinConfiguration.DefaultComboColours[2]);
            AddAssert("fourth object has fourth colour", () => player.DrawableRuleset.Playfield.AllHitObjects.ElementAt(3).AccentColour.Value == SkinConfiguration.DefaultComboColours[3]);
        }

        private TestPlayer loadBeatmap()
        {
            TestPlayer player;

            Beatmap.Value = new ComboColourSkippingWorkingBeatmap(audio);
            LoadScreen(player = new TestPlayer(false, false));

            return player;
        }

        private class ComboColourSkippingWorkingBeatmap : ClockBackedTestWorkingBeatmap
        {
            public ComboColourSkippingWorkingBeatmap(AudioManager audio)
                : base(new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new HitCircle(),
                        new HitCircle { StartTime = 100, NewCombo = true, ComboOffset = 1 },
                        new HitCircle { StartTime = 200, NewCombo = true, ComboOffset = 3 },
                        new HitCircle { StartTime = 300, NewCombo = true, ComboOffset = 5 },
                    },
                }, null, null, audio)
            {
            }

            protected override ISkin GetSkin() => new TestBeatmapSkin(BeatmapInfo);
        }

        private class TestBeatmapSkin : LegacyBeatmapSkin
        {
            public static Color4[] Colours { get; } =
            {
                Color4Extensions.FromHex("222"),
                Color4Extensions.FromHex("666"),
                Color4Extensions.FromHex("aaa"),
                Color4Extensions.FromHex("eee"),
            };

            public TestBeatmapSkin(BeatmapInfo beatmap)
                : base(beatmap, null, null)
            {
                Configuration.AddComboColours(Colours);
            }
        }
    }
}
