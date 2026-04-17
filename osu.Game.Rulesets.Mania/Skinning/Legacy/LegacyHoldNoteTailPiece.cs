// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyHoldNoteTailPiece : LegacyNotePiece
    {
        private readonly IBindable<double?> missingStartTime = new Bindable<double?>();

        [Resolved]
        private DrawableHitObject drawableObject { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            missingStartTime.BindTo(((DrawableHoldNoteTail)drawableObject).MissingStartTime);
            missingStartTime.BindValueChanged(onMissingStartTimeChanged, true);

            drawableObject.ApplyCustomUpdateState += onApplyCustomUpdateState;
        }

        private void onMissingStartTimeChanged(ValueChangedEvent<double?> startTime)
            => applyMissingDim();

        private void onApplyCustomUpdateState(DrawableHitObject obj, ArmedState state)
            => applyMissingDim();

        private void applyMissingDim()
        {
            if (missingStartTime.Value == null)
                return;

            using (BeginAbsoluteSequence(missingStartTime.Value.Value))
                this.FadeColour(Colour4.DarkGray, 60);
        }

        protected override void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            // Invert the direction
            base.OnDirectionChanged(direction.NewValue == ScrollingDirection.Up
                ? new ValueChangedEvent<ScrollingDirection>(ScrollingDirection.Down, ScrollingDirection.Down)
                : new ValueChangedEvent<ScrollingDirection>(ScrollingDirection.Up, ScrollingDirection.Up));
        }

        protected override Drawable? GetAnimation(ISkinSource skin)
        {
            // TODO: Should fallback to the head from default legacy skin instead of note.
            return GetAnimationFromLookup(skin, LegacyManiaSkinConfigurationLookups.HoldNoteTailImage)
                   ?? GetAnimationFromLookup(skin, LegacyManiaSkinConfigurationLookups.HoldNoteHeadImage)
                   ?? GetAnimationFromLookup(skin, LegacyManiaSkinConfigurationLookups.NoteImage);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableObject.IsNotNull())
                drawableObject.ApplyCustomUpdateState -= onApplyCustomUpdateState;
        }
    }
}
