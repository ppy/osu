// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneNowPlayingOverlay : OsuTestScene
    {
        [Cached]
        private MusicController musicController = new MusicController();

        private NowPlayingOverlay nowPlayingOverlay;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            AddToggleStep("toggle unicode", v => frameworkConfig.SetValue(FrameworkSetting.ShowUnicode, v));

            nowPlayingOverlay = new NowPlayingOverlay
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre
            };

            Add(musicController);
            Add(nowPlayingOverlay);
        }

        [Test]
        public void TestShowHideDisable()
        {
            AddStep(@"set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo));
            AddStep(@"show", () => nowPlayingOverlay.Show());
            AddToggleStep(@"toggle beatmap lock", state => Beatmap.Disabled = state);
            AddStep(@"hide", () => nowPlayingOverlay.Hide());
        }

        [Test]
        public void TestLongMetadata()
        {
            AddStep(@"set metadata within tolerance", () => Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                Metadata =
                {
                    Artist = "very very very very very very very very very very verry long artist",
                    ArtistUnicode = "very very very very very very very very very very verry long artist unicode",
                    Title = "very very very very very verry long title",
                    TitleUnicode = "very very very very very verry long title unicode",
                }
            }));

            AddStep(@"set metadata outside bounds", () => Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                Metadata =
                {
                    Artist = "very very very very very very very very very very verrry long artist",
                    ArtistUnicode = "not very long artist unicode",
                    Title = "very very very very very verrry long title",
                    TitleUnicode = "not very long title unicode",
                }
            }));

            AddStep(@"show", () => nowPlayingOverlay.Show());
        }
    }
}
