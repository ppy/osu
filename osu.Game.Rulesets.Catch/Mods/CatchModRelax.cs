// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public partial class CatchModRelax : ModRelax, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToPlayer
    {
        public override LocalisableString Description => @"Use the mouse to control the catcher.";

        private DrawableCatchRuleset drawableRuleset = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            this.drawableRuleset = (DrawableCatchRuleset)drawableRuleset;
        }

        public void ApplyToPlayer(Player player)
        {
            if (!drawableRuleset.HasReplayLoaded.Value)
            {
                var catchPlayfield = (CatchPlayfield)drawableRuleset.Playfield;
                catchPlayfield.CatcherArea.Add(new MouseInputHelper(catchPlayfield.CatcherArea));
            }
        }

        private partial class MouseInputHelper : Drawable, IKeyBindingHandler<CatchAction>, IRequireHighFrequencyMousePosition
        {
            private readonly CatcherArea catcherArea;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public MouseInputHelper(CatcherArea catcherArea)
            {
                this.catcherArea = catcherArea;

                RelativeSizeAxes = Axes.Both;
            }

            // disable keyboard controls
            public bool OnPressed(KeyBindingPressEvent<CatchAction> e) => true;

            public void OnReleased(KeyBindingReleaseEvent<CatchAction> e)
            {
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                catcherArea.SetCatcherPosition(e.MousePosition.X / DrawSize.X * CatchPlayfield.WIDTH);
                return base.OnMouseMove(e);
            }
        }
    }
}
