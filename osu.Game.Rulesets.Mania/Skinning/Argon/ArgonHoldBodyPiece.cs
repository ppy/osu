// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;
using static osu.Game.Rulesets.Mania.Skinning.Argon.ArgonSnapColouring;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    /// <summary>
    /// Represents length-wise portion of a hold note.
    /// </summary>
    public partial class ArgonHoldBodyPiece : CompositeDrawable
    {
        private readonly IBindable<Color4> baseAccentColour = new Bindable<Color4>();
        private readonly IBindable<int> snapDivisor = new Bindable<int>();
        private readonly Bindable<Color4> accentColour = new Bindable<Color4>();

        [Resolved]
        private OsuColour? colours { get; set; }

        private Drawable background = null!;
        private ArgonHoldNoteHittingLayer hittingLayer = null!;

        public ArgonHoldBodyPiece()
        {
            RelativeSizeAxes = Axes.Both;

            // Without this, the width of the body will be slightly larger than the head/tail.
            Masking = true;
            CornerRadius = ArgonNotePiece.CORNER_RADIUS;
        }

        [BackgroundDependencyLoader(true)]
        private void load(DrawableHitObject? drawableObject)
        {
            InternalChildren = new[]
            {
                background = new Box { RelativeSizeAxes = Axes.Both },
                hittingLayer = new ArgonHoldNoteHittingLayer()
            };

            if (drawableObject is not null)
            {
                baseAccentColour.BindTo(drawableObject.AccentColour);
                baseAccentColour.BindValueChanged(_ => updateNoteAccent(), true);

                drawableObject.HitObjectApplied += hitObjectApplied;
            }
        }

        private void hitObjectApplied(DrawableHitObject hitObject)
        {
            if (hitObject is DrawableHoldNote holdNote)
            {
                snapDivisor.UnbindBindings();
                snapDivisor.BindTo(holdNote.SnapDivisor);
                snapDivisor.BindValueChanged(_ => updateNoteAccent(), true);

                hittingLayer.Recycle();

                hittingLayer.IsHitting.UnbindBindings();
                hittingLayer.IsHitting.BindTo(holdNote.IsHitting);
                hittingLayer.AccentColour.UnbindBindings();
                hittingLayer.AccentColour.BindTo(accentColour);
            }
        }

        private void updateNoteAccent()
        {
            accentColour.Value = snapDivisor.Value == 0
                ? baseAccentColour.Value
                : SnapColourFor(snapDivisor.Value, colours);

            background.Colour = accentColour.Value.Darken(0.6f);
        }
    }
}
