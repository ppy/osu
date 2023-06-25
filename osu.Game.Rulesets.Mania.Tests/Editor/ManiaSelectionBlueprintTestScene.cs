// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public abstract partial class ManiaSelectionBlueprintTestScene : SelectionBlueprintTestScene
    {
        protected override Container<Drawable> Content => blueprints ?? base.Content;

        private readonly Container? blueprints;

        [Cached(typeof(Playfield))]
        public Playfield Playfield { get; }

        private readonly ScrollingTestContainer scrollingTestContainer;

        protected ScrollingDirection Direction
        {
            set => scrollingTestContainer.Direction = value;
        }

        protected ManiaSelectionBlueprintTestScene(int columns)
        {
            var stageDefinitions = new List<StageDefinition> { new StageDefinition(columns) };
            base.Content.Child = scrollingTestContainer = new ScrollingTestContainer(ScrollingDirection.Up)
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    Playfield = new ManiaPlayfield(stageDefinitions)
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    blueprints = new Container
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                }
            };

            AddToggleStep("Downward scroll", b => Direction = b ? ScrollingDirection.Down : ScrollingDirection.Up);
        }
    }
}
