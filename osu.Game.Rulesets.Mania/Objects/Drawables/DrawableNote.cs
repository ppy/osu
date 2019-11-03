// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="Note"/> hit object.
    /// </summary>
    public class DrawableNote : DrawableManiaHitObject<Note>, IKeyBindingHandler<ManiaAction>
    {
        private readonly NotePiece headPiece;

        public DrawableNote(Note hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            CornerRadius = 5;
            Masking = true;

            AddInternal(headPiece = new NotePiece());

            AccentColour.BindValueChanged(colour =>
            {
                headPiece.AccentColour = colour.NewValue;

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = colour.NewValue.Lighten(1f).Opacity(0.2f),
                    Radius = 10,
                };
            }, true);
        }

        protected override void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> e)
        {
            base.OnDirectionChanged(e);

            headPiece.Anchor = headPiece.Origin = e.NewValue == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            ApplyResult(r => r.Type = result);
        }

        public virtual bool OnPressed(ManiaAction action)
        {
            if (action != Action.Value)
                return false;

            return UpdateResult(true);
        }

        public virtual bool OnReleased(ManiaAction action) => false;
    }
}
