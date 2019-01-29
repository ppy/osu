// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Tests.Visual
{
    public abstract class SelectionBlueprintTestCase : OsuTestCase
    {
        private SelectionBlueprint blueprint;

        protected override Container<Drawable> Content => content ?? base.Content;
        private readonly Container content;

        protected SelectionBlueprintTestCase()
        {
            base.Content.Add(content = new Container
            {
                Clock = new FramedClock(new StopwatchClock()),
                RelativeSizeAxes = Axes.Both
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            blueprint = CreateBlueprint();
            blueprint.Depth = float.MinValue;
            blueprint.SelectionRequested += (_, __) => blueprint.Select();

            Add(blueprint);

            AddStep("Select", () => blueprint.Select());
            AddStep("Deselect", () => blueprint.Deselect());
        }

        protected override bool OnClick(ClickEvent e)
        {
            blueprint.Deselect();
            return true;
        }

        protected abstract SelectionBlueprint CreateBlueprint();
    }
}
