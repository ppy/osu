// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
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
    public partial class CatchModMovingAlways : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToPlayer
    {
        public override string Name => "Moving Always";
        public override string Acronym => "MA";
        public override LocalisableString Description => "Absolutely restless... no sense in stopping!";
        public override ModType Type => ModType.Fun;
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
                catchPlayfield.CatcherArea.Add(new NonstopMoveInputHelper(catchPlayfield.CatcherArea));
            }
        }

        private partial class NonstopMoveInputHelper : Drawable, IKeyBindingHandler<CatchAction>
        {
            private readonly CatcherArea catcherArea;

            // To work with CatcherArea's OnPressed and OnReleased helping set its currentDirection accordingly always to -1 (left) or 1 (right)
            private int currentDirection;

            private const int left = -1;
            private const int right = 1;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public NonstopMoveInputHelper(CatcherArea catcherArea)
            {
                this.catcherArea = catcherArea;

                RelativeSizeAxes = Axes.Both;

                // Just like Catcher's default VisualDirection except already moving in that direction
                this.catcherArea.OnPressed(new KeyBindingPressEvent<CatchAction>(new InputState(), CatchAction.MoveRight));
                currentDirection = right;
            }

            public bool OnPressed(KeyBindingPressEvent<CatchAction> e)
            {
                switch (e.Action)
                {
                    case CatchAction.MoveLeft:
                        if (currentDirection == right) // act out an extra left key press to transition to moving left instead of a stop
                            catcherArea.OnPressed(new KeyBindingPressEvent<CatchAction>(new InputState(), CatchAction.MoveLeft));
                        else // act out a neutralizing left key release to stay moving left instead of moving faster left
                            catcherArea.OnReleased(new KeyBindingReleaseEvent<CatchAction>(new InputState(), CatchAction.MoveLeft));

                        currentDirection = left;
                        break;

                    case CatchAction.MoveRight:
                        if (currentDirection == left) // act out an extra right key press to transition to moving right instead of a stop
                            catcherArea.OnPressed(new KeyBindingPressEvent<CatchAction>(new InputState(), CatchAction.MoveRight));
                        else // act out a neutralizing right key release to stay moving right instead of moving faster right
                            catcherArea.OnReleased(new KeyBindingReleaseEvent<CatchAction>(new InputState(), CatchAction.MoveRight));

                        currentDirection = right;
                        break;

                    case CatchAction.Dash:
                        break;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<CatchAction> e)
            {
                // Moving Always means releasing a movement key does nothing in trying to stop moving, so neutralize by acting out its key press
                switch (e.Action)
                {
                    case CatchAction.MoveLeft:
                        catcherArea.OnPressed(new KeyBindingPressEvent<CatchAction>(new InputState(), CatchAction.MoveLeft));
                        break;

                    case CatchAction.MoveRight:
                        catcherArea.OnPressed(new KeyBindingPressEvent<CatchAction>(new InputState(), CatchAction.MoveRight));
                        break;

                    case CatchAction.Dash:
                        break;
                }
            }
        }
    }
}
