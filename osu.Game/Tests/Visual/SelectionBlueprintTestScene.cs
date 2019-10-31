// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Tests.Visual
{
    public abstract class SelectionBlueprintTestScene : OsuTestScene
    {
        protected override Container<Drawable> Content => content ?? base.Content;
        private readonly Container content;

        protected SelectionBlueprintTestScene()
        {
            base.Content.Add(content = new Container
            {
                Clock = new FramedClock(new StopwatchClock()),
                RelativeSizeAxes = Axes.Both
            });
        }

        protected void AddBlueprint(SelectionBlueprint blueprint)
        {
            Add(blueprint.With(d =>
            {
                d.Depth = float.MinValue;
                d.Select();
            }));
        }
    }
}
