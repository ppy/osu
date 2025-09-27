// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit
{
    internal class GenericChangeOpacityMenuItem : MenuItem
    {
        private readonly Bindable<float> bindableOpacity;

        private readonly Dictionary<float, TernaryStateRadioMenuItem> menuItemLookup = new Dictionary<float, TernaryStateRadioMenuItem>();

        public GenericChangeOpacityMenuItem(Bindable<float> bindableOpacity, LocalisableString text, IEnumerable<float> availableOptions)
            : base(text)
        {
            Items = availableOptions.Select(createMenuItem).ToList().AsReadOnly();

            this.bindableOpacity = bindableOpacity;
            bindableOpacity.BindValueChanged(opacity =>
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

        private void updateOpacity(float opacity) => bindableOpacity.Value = opacity;
    }
}
