// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Visual
{
    public abstract class SelectionBlueprintTestScene : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly EditorClock editorClock = new EditorClock();

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

        protected void AddBlueprint(HitObjectSelectionBlueprint blueprint, [CanBeNull] DrawableHitObject drawableObject = null)
        {
            Add(blueprint.With(d =>
            {
                d.DrawableObject = drawableObject;
                d.Depth = float.MinValue;
                d.Select();
            }));
        }
    }
}
