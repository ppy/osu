// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModRelax : ModRelax, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Description => @"Use the mouse to control the catcher.";

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            drawableRuleset.Cursor.Add(new MouseInputHelper((CatchPlayfield)drawableRuleset.Playfield));
        }

        private class MouseInputHelper : Drawable, IKeyBindingHandler<CatchAction>, IRequireHighFrequencyMousePosition
        {
            private readonly Catcher catcher;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public MouseInputHelper(CatchPlayfield playfield)
            {
                catcher = playfield.CatcherArea.MovableCatcher;
                RelativeSizeAxes = Axes.Both;
            }

            //disable keyboard controls
            public bool OnPressed(CatchAction action) => true;

            public void OnReleased(CatchAction action)
            {
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                catcher.UpdatePosition(e.MousePosition.X / DrawSize.X);
                return base.OnMouseMove(e);
            }
        }
    }
}
