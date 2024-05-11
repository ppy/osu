// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit
{
    internal class WaveformOpacityMenuItem : MenuItem
    {
        private readonly Bindable<float> waveformOpacity;

        private readonly Dictionary<float, TernaryStateRadioMenuItem> menuItemLookup = new Dictionary<float, TernaryStateRadioMenuItem>();

        public WaveformOpacityMenuItem(Bindable<float> waveformOpacity)
            : base(EditorStrings.WaveformOpacity)
        {
            Items = new[]
            {
                createMenuItem(0.25f),
                createMenuItem(0.5f),
                createMenuItem(0.75f),
                createMenuItem(1f),
            };

            this.waveformOpacity = waveformOpacity;
            waveformOpacity.BindValueChanged(opacity =>
            {
                foreach (var kvp in menuItemLookup)
                    kvp.Value.State.Value = kvp.Key == opacity.NewValue ? TernaryState.True : TernaryState.False;
            }, true);
        }

        private TernaryStateRadioMenuItem createMenuItem(float opacity)
        {
            var item = new TernaryStateRadioMenuItem($"{opacity * 100}%", MenuItemType.Standard, _ => updateOpacity(opacity));
            menuItemLookup[opacity] = item;
            return item;
        }

        private void updateOpacity(float opacity) => waveformOpacity.Value = opacity;
    }
}
