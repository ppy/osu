// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Screens.Setup;
using osu.Game.Screens.Edit.Screens.Setup.Screens;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditorSetup : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Setup), typeof(AudioSettings), typeof(DesignSettings), typeof(DifficultySettings), typeof(ModeSettings), typeof(GeneralSettings), typeof(AdvancedSettings), typeof(ColoursSettings), typeof(ColouredEditorSliderBar<>) };

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame, SkinManager skins)
        {
            osuGame.Beatmap.Value = new TestWorkingBeatmap(new OsuRuleset().RulesetInfo);

            Setup setup = new Setup();

            // This needs fixing for more proper testing
            osuGame.Beatmap.Value.Metadata.Artist = "Test artist";
            osuGame.Beatmap.Value.Metadata.ArtistUnicode = "Test artist - Romanised";
            osuGame.Beatmap.Value.Metadata.Title = "Test title";
            osuGame.Beatmap.Value.Metadata.TitleUnicode = "Test title - Romanised";
            //osuGame.Beatmap.Value.BeatmapSetInfo.Beatmaps.Add(new BeatmapInfo());
            //osuGame.Beatmap.Value.Metadata.Beatmaps[0].Version = "Test beatmap version (0)"; This causes problems so let's not care about it as of yet
            osuGame.Beatmap.Value.Metadata.Author.Username = "Test beatmap creator";
            osuGame.Beatmap.Value.Metadata.Source = "Test source";
            osuGame.Beatmap.Value.Metadata.Tags = "Test tag list";

            setup.Beatmap.BindTo(osuGame.Beatmap);

            Child = setup;
        }
    }
}
