// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyHoldNoteHeadPiece : LegacyNotePiece
    {
        private readonly IBindable<double?> missingStartTime = new Bindable<double?>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            missingStartTime.BindTo(((DrawableHoldNoteHead)drawableObject).MissingStartTime);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            missingStartTime.BindValueChanged(onMissingStartTimeChanged, true);
        }

        private void onMissingStartTimeChanged(ValueChangedEvent<double?> startTime)
        {
            if (startTime.NewValue == null)
            {
                // Colour revert handled by the DHO transform reset.
                return;
            }

            using (BeginAbsoluteSequence(startTime.NewValue.Value))
                this.FadeColour(Colour4.DarkGray, 60);
        }

        protected override Drawable? GetAnimation(ISkinSource skin)
        {
            // TODO: Should fallback to the head from default legacy skin instead of note.
            return GetAnimationFromLookup(skin, LegacyManiaSkinConfigurationLookups.HoldNoteHeadImage)
                   ?? GetAnimationFromLookup(skin, LegacyManiaSkinConfigurationLookups.NoteImage);
        }
    }
}
