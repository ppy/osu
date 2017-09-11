// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Extensions.Color4Extensions;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="Note"/> hit object.
    /// </summary>
    public class DrawableNote : DrawableManiaHitObject<Note>, IKeyBindingHandler<ManiaAction>
    {
        /// <summary>
        /// Gets or sets whether this <see cref="DrawableNote"/> should apply glow to itself.
        /// </summary>
        protected bool ApplyGlow = true;

        private readonly NotePiece headPiece;

        public DrawableNote(Note hitObject, ManiaAction action)
            : base(hitObject, action)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;

            Add(headPiece = new NotePiece
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateGlow();
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                if (base.AccentColour == value)
                    return;
                base.AccentColour = value;

                headPiece.AccentColour = value;

                updateGlow();
            }
        }

        private void updateGlow()
        {
            if (!IsLoaded)
                return;

            if (!ApplyGlow)
                return;

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = AccentColour.Opacity(0.5f),
                Radius = 10,
                Hollow = true
            };
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > HitObject.HitWindows.Bad / 2)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double offset = Math.Abs(Judgement.TimeOffset);

            if (offset > HitObject.HitWindows.Miss / 2)
                return;

            ManiaHitResult? tmpResult = HitObject.HitWindows.ResultFor(offset);

            if (tmpResult.HasValue)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.ManiaResult = tmpResult.Value;
            }
            else
                Judgement.Result = HitResult.Miss;
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (State)
            {
                case ArmedState.Hit:
                    Colour = Color4.Green;
                    break;
            }
        }

        public virtual bool OnPressed(ManiaAction action)
        {
            if (action != Action)
                return false;

            return UpdateJudgement(true);
        }

        public virtual bool OnReleased(ManiaAction action) => false;
    }
}
