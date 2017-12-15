// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseBeatmapInfoWedge : OsuTestCase
    {
        private BeatmapManager beatmaps;
        private readonly Random random;
        private readonly BeatmapInfoWedge infoWedge;
        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        public TestCaseBeatmapInfoWedge()
        {
            random = new Random(0123);

            Add(infoWedge = new BeatmapInfoWedge
            {
                Size = new Vector2(0.5f, 245),
                RelativeSizeAxes = Axes.X,
                Margin = new MarginPadding
                {
                    Top = 20,
                },
            });

            AddStep("show", () =>
            {
                Content.FadeInFromZero(250);
                infoWedge.State = Visibility.Visible;
                infoWedge.UpdateBeatmap(beatmap);
            });
            AddStep("hide", () =>
            {
                infoWedge.State = Visibility.Hidden;
                Content.FadeOut(100);
            });
            AddStep("random beatmap", randomBeatmap);
            AddStep("null beatmap", () => infoWedge.UpdateBeatmap(beatmap.Default));
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;
            beatmap.BindTo(game.Beatmap);
        }

        private void randomBeatmap()
        {
            var sets = beatmaps.GetAllUsableBeatmapSets();
            if (sets.Count == 0)
                return;

            var b = sets[random.Next(0, sets.Count)].Beatmaps[0];
            beatmap.Value = beatmaps.GetWorkingBeatmap(b);
            infoWedge.UpdateBeatmap(beatmap);
        }
    }
}
