// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit
{
    internal class TimelineObjectsOpacityMenuItem : MenuItem
    {
        private readonly Bindable<float> timelineObjectsOpacity;

        private readonly Dictionary<float, TernaryStateRadioMenuItem> menuItemLookup = new Dictionary<float, TernaryStateRadioMenuItem>();

        public TimelineObjectsOpacityMenuItem(Bindable<float> timelineObjectsOpacity)
            : base(EditorStrings.TimelineObjectsOpacity)
        {
            Items = new[]
            {
                createMenuItem(0f),
                createMenuItem(0.25f),
                createMenuItem(0.5f),
                createMenuItem(0.75f),
                createMenuItem(1f),
            };

            this.timelineObjectsOpacity = timelineObjectsOpacity;
            timelineObjectsOpacity.BindValueChanged(opacity =>
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

        private void updateOpacity(float opacity) => timelineObjectsOpacity.Value = opacity;
    }
}
