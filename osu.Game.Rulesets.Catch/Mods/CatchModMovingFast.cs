// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public partial class CatchModMovingFast : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToPlayer
    {
        public override string Name => "Moving Fast";
        public override string Acronym => "MF";
        public override LocalisableString Description => "Dashing by default, slow down!";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => OsuIcon.ModMovingFast;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax) };

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
                catchPlayfield.Catcher.Dashing = true;
                catchPlayfield.CatcherArea.Add(new InvertDashInputHelper(catchPlayfield.CatcherArea));
            }
        }

        private partial class InvertDashInputHelper : Drawable, IKeyBindingHandler<CatchAction>
        {
            private readonly CatcherArea catcherArea;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public InvertDashInputHelper(CatcherArea catcherArea)
            {
                this.catcherArea = catcherArea;

                RelativeSizeAxes = Axes.Both;
            }

            public bool OnPressed(KeyBindingPressEvent<CatchAction> e)
            {
                switch (e.Action)
                {
                    case CatchAction.MoveLeft or CatchAction.MoveRight:
                        break;

                    case CatchAction.Dash:
                        catcherArea.Catcher.Dashing = false;
                        return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<CatchAction> e)
            {
                if (e.Action == CatchAction.Dash)
                    catcherArea.Catcher.Dashing = true;
            }
        }
    }
}
