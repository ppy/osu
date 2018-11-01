// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Tests.Visual
{
    public abstract class HitObjectSelectionMaskTestCase : OsuTestCase
    {
        private SelectionMask mask;

        protected override Container<Drawable> Content => content ?? base.Content;
        private readonly Container content;

        protected HitObjectSelectionMaskTestCase()
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
            base.Content.Add(mask = CreateMask());
            mask.SelectionRequested += (_, __) => mask.Select();

            AddStep("Select", () => mask.Select());
            AddStep("Deselect", () => mask.Deselect());
        }

        protected override bool OnClick(ClickEvent e)
        {
            mask.Deselect();
            return true;
        }

        protected abstract SelectionMask CreateMask();
    }
}
