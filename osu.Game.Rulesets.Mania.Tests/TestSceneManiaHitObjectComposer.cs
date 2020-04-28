// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestSceneManiaHitObjectComposer : EditorClockTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ManiaBlueprintContainer)
        };

        [Cached(typeof(EditorBeatmap))]
        [Cached(typeof(IBeatSnapProvider))]
        private readonly EditorBeatmap editorBeatmap;

        protected override Container<Drawable> Content { get; }

        private ManiaHitObjectComposer composer;

        public TestSceneManiaHitObjectComposer()
        {
            base.Content.Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    editorBeatmap = new EditorBeatmap(new ManiaBeatmap(new StageDefinition { Columns = 4 }))
                    {
                        BeatmapInfo = { Ruleset = new ManiaRuleset().RulesetInfo }
                    },
                    Content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
            });

            for (int i = 0; i < 10; i++)
            {
                editorBeatmap.Add(new Note { StartTime = 100 * i });
            }
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                composer = new ManiaHitObjectComposer(new ManiaRuleset())
            };

            BeatDivisor.Value = 8;
        });
    }
}
