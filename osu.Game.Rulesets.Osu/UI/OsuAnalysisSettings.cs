// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuAnalysisSettings : AnalysisSettings
    {
        protected new DrawableOsuRuleset drawableRuleset => (DrawableOsuRuleset)base.drawableRuleset;

        private readonly PlayerCheckbox hitMarkerToggle;
        private readonly PlayerCheckbox aimMarkerToggle;
        private readonly PlayerCheckbox hideCursorToggle;
        private readonly PlayerCheckbox aimLinesToggle;

        public OsuAnalysisSettings(DrawableRuleset drawableRuleset)
            : base(drawableRuleset)
        {
            Children = new Drawable[]
            {
                hitMarkerToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.HitMarkers },
                aimMarkerToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.AimMarkers },
                aimLinesToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.AimLines },
                hideCursorToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.HideCursor }
            };
        }

        protected override void LoadComplete()
        {
            drawableRuleset.Playfield.MarkersContainer.HitMarkerEnabled.BindTo(hitMarkerToggle.Current);
            drawableRuleset.Playfield.MarkersContainer.AimMarkersEnabled.BindTo(aimMarkerToggle.Current);
            drawableRuleset.Playfield.MarkersContainer.AimLinesEnabled.BindTo(aimLinesToggle.Current);
            hideCursorToggle.Current.BindValueChanged(onCursorToggle);
        }

        private void onCursorToggle(ValueChangedEvent<bool> hide)
        {
            // this only hides half the cursor
            if (hide.NewValue)
            {
                drawableRuleset.Playfield.Cursor.FadeOut();
            } else
            {
                drawableRuleset.Playfield.Cursor.FadeIn();
            }
        }
    }
}