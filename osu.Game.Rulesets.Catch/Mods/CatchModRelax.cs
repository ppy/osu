// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;
using System;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModRelax : ModRelax, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Description => @"Use the mouse to control the catcher.";

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset) =>
            ((Container)drawableRuleset.Playfield.Parent).Add(new CatchModRelaxHelper(drawableRuleset.Playfield as CatchPlayfield));

        private class CatchModRelaxHelper : Drawable, IKeyBindingHandler<CatchAction>, IRequireHighFrequencyMousePosition
        {
            private readonly CatcherArea.Catcher catcher;

            public CatchModRelaxHelper(CatchPlayfield catchPlayfield)
            {
                catcher = catchPlayfield.CatcherArea.MovableCatcher;
                RelativeSizeAxes = Axes.Both;
            }

            //disable keyboard controls
            public bool OnPressed(CatchAction action) => true;
            public bool OnReleased(CatchAction action) => true;

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                //lock catcher to mouse position horizontally
                catcher.X = e.MousePosition.X / DrawSize.X;

                //make Yuzu face the direction he's moving
                var direction = Math.Sign(e.Delta.X);
                if (direction != 0)
                    catcher.Scale = new Vector2(Math.Abs(catcher.Scale.X) * direction, catcher.Scale.Y);

                return base.OnMouseMove(e);
            }
        }
    }
}
