// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
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
        private readonly FillFlowContainer modTogglesContainer;

        public OsuAnalysisSettings(DrawableRuleset drawableRuleset)
            : base(drawableRuleset)
        {
            Children = new Drawable[]
            {
                hitMarkerToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.HitMarkers },
                aimMarkerToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.AimMarkers },
                aimLinesToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.AimLines },
                hideCursorToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.HideCursor },
                new OsuScrollContainer(Direction.Horizontal)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = ModSwitchSmall.DEFAULT_SIZE,
                    ScrollbarOverlapsContent = false,
                    Child = modTogglesContainer = new FillFlowContainer
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Direction = FillDirection.Horizontal,
                        RelativeSizeAxes = Axes.Y,
                        AutoSizeAxes = Axes.X
                    }
                }
            };
            
            foreach (var mod in drawableRuleset.Mods)
            {
                if (mod is IToggleableVisibility toggleableMod)
                {
                    var modSwitch = new SelectableModSwitchSmall(mod)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Active = { Value = true }
                    };
                    modSwitch.Active.BindValueChanged((v) => onModToggle(toggleableMod, v));
                    modTogglesContainer.Add(modSwitch);
                }
            }
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

        private void onModToggle(IToggleableVisibility mod, ValueChangedEvent<bool> toggled)
        {
            if (toggled.NewValue)
            {
                mod.ToggleOnVisibility(drawableRuleset.Playfield);
            } else
            {
                mod.ToggleOffVisibility(drawableRuleset.Playfield);
            }
        }

        private partial class SelectableModSwitchSmall : ModSwitchSmall
        {
            public SelectableModSwitchSmall(IMod mod)
                : base(mod)
            {}

            protected override bool OnClick(ClickEvent e)
            {
                Active.Value = !Active.Value;
                return true;
            }
        }
    }
}