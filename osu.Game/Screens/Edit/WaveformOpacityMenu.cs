// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit
{
    internal class WaveformOpacityMenu : MenuItem
    {
        private readonly Bindable<float> waveformOpacity;

        private readonly Dictionary<float, ToggleMenuItem> menuItemLookup = new Dictionary<float, ToggleMenuItem>();

        public WaveformOpacityMenu(OsuConfigManager config)
            : base("Waveform opacity")
        {
            Items = new[]
            {
                createMenuItem(0.25f),
                createMenuItem(0.5f),
                createMenuItem(0.75f),
                createMenuItem(1f),
            };

            waveformOpacity = config.GetBindable<float>(OsuSetting.EditorWaveformOpacity);
            waveformOpacity.BindValueChanged(opacity =>
            {
                foreach (var kvp in menuItemLookup)
                    kvp.Value.State.Value = kvp.Key == opacity.NewValue;
            }, true);
        }

        private ToggleMenuItem createMenuItem(float opacity)
        {
            var item = new ToggleMenuItem($"{opacity * 100}%", MenuItemType.Standard, _ => updateOpacity(opacity));
            menuItemLookup[opacity] = item;
            return item;
        }

        private void updateOpacity(float opacity) => waveformOpacity.Value = opacity;
    }
}
