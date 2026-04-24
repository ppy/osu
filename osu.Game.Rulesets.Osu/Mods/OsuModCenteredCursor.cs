// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public partial class OsuModCenteredCursor : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToPlayer
    {
        public override string Name => "Centered Cursor";
        public override LocalisableString Description => "Cursor stays in the middle!";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "CC";
        public override ModType Type => ModType.Fun;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModFlashlight), typeof(OsuModAutopilot), typeof(OsuModRelax), typeof(OsuModBubbles) }).ToArray();

        private OsuInputManager osuInputManager = null!;
        private bool hasReplay;
        private ExternalMousePosGetter externalMousePosGetter = null!;

        public void Update(Playfield playfield)
        {
            externalMousePosGetter.Enable = !hasReplay;

            // The coords of the cursor in playfield local space
            Vector2 osuPos;

            // If it's a replay we don't need to do mouse conversion
            if (hasReplay)
            {
                osuPos = playfield.Cursor!.ActiveCursor.Position;
            }
            else
            {
                var mousePos = externalMousePosGetter.MousePos;

                // We convert the coords using the playfield parent because the playfield is moving so the values would be wrong
                osuPos = playfield.Parent!.ToLocalSpace(mousePos);

                new ConvertedMousePositionAbsoluteInput { Position = playfield.ToScreenSpace(osuPos) }.Apply(osuInputManager.CurrentState, osuInputManager);
            }

            playfield.Position = playfield.LayoutSize / 2 - osuPos;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Grab the input manager for future use
            osuInputManager = ((DrawableOsuRuleset)drawableRuleset).KeyBindingInputManager;

            // Added this way, ExternalMousePosGetter receives OnMouseMove before the playfield drawables, so it can block propagation to the playfield
            drawableRuleset.PlayfieldAdjustmentContainer.Add(externalMousePosGetter = new ExternalMousePosGetter { RelativeSizeAxes = Axes.Both });

            // Reset playfield position while paused so the resume overlay reads the real cursor position correctly.
            // This avoids the resume overlay forcing the user to move the mouse to the center, which would cause a cursor jump/teleportation when resuming.
            drawableRuleset.IsPaused.BindValueChanged(p =>
            {
                if (p.NewValue) drawableRuleset.Playfield.Position = Vector2.Zero;
            });
        }

        public void ApplyToPlayer(Player player)
        {
            if (osuInputManager.ReplayInputHandler != null) hasReplay = true;
        }

        private class ConvertedMousePositionAbsoluteInput : MousePositionAbsoluteInput;

        private partial class ExternalMousePosGetter : Drawable, IRequireHighFrequencyMousePosition
        {
            public bool Enable = true;
            public Vector2 MousePos { get; private set; } = Vector2.Zero;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                if (!Enable)
                    return base.OnMouseMove(e);

                // We skip our own added mouse position
                if (e.CurrentState.Mouse.LastSource is ConvertedMousePositionAbsoluteInput)
                    return base.OnMouseMove(e);

                MousePos = e.ScreenSpaceMousePosition;

                // We block real mouse position propagation to the playfield
                return true;
            }
        }
    }
}
