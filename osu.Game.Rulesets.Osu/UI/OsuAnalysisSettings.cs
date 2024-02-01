// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuAnalysisSettings : AnalysisSettings
    {
         private static readonly Logger logger = Logger.GetLogger("osu-analysis-settings");

        protected new DrawableOsuRuleset drawableRuleset => (DrawableOsuRuleset)base.drawableRuleset;

        private readonly PlayerCheckbox hitMarkerToggle;
        private readonly PlayerCheckbox aimMarkerToggle;
        private readonly PlayerCheckbox hideCursorToggle;
        private readonly PlayerCheckbox? hiddenToggle;

        public OsuAnalysisSettings(DrawableRuleset drawableRuleset)
            : base(drawableRuleset)
        {
            Children = new Drawable[]
            {
                hitMarkerToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.HitMarkers },
                aimMarkerToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.AimMarkers },
                hideCursorToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.HideCursor }
            };

            // hidden stuff is just here for testing at the moment; to create the mod disabling functionality
            
            foreach (var mod in drawableRuleset.Mods)
            {
                if (mod is OsuModHidden)
                {
                    logger.Add("Hidden is enabled", LogLevel.Debug);
                    Add(hiddenToggle = new PlayerCheckbox { LabelText = "Disable hidden" });
                    break;
                }
            }
        }

        protected override void LoadComplete()
        {
            drawableRuleset.Playfield.MarkersContainer.HitMarkerEnabled.BindTo(hitMarkerToggle.Current);
            drawableRuleset.Playfield.MarkersContainer.AimMarkersEnabled.BindTo(aimMarkerToggle.Current);
            hideCursorToggle.Current.BindValueChanged(onCursorToggle);
            hiddenToggle?.Current.BindValueChanged(onHiddenToggle);
        }

        private void onCursorToggle(ValueChangedEvent<bool> hide)
        {
            // this only hides half the cursor
            if (hide.NewValue)
            {
                drawableRuleset.Playfield.Cursor.Hide();
            } else
            {
                drawableRuleset.Playfield.Cursor.Show();
            }
        }

        private void onHiddenToggle(ValueChangedEvent<bool> off)
        {
            if (off.NewValue)
            {
                logger.Add("Hidden off", LogLevel.Debug);
            } else
            {
                logger.Add("Hidden on", LogLevel.Debug);
            }
        }
    }
}