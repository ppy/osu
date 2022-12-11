// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Display the specified mod at a fixed size.
    /// </summary>
    public partial class ClickableModIcon : ModIcon
    {
        private readonly Action? action;

        private readonly Bindable<bool> replayLoaded = new Bindable<bool>();

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="mod">The mod to be displayed</param>
        /// <param name="replayLoaded">Whether replay is loaded.</param>
        /// <param name="showTooltip">Whether a tooltip describing the mod should display on hover.</param>
        public ClickableModIcon(IMod mod, Bindable<bool> replayLoaded, bool showTooltip = true)
            : base(mod, showTooltip)
        {
            this.replayLoaded.BindTo(replayLoaded);

            if (mod is ICanBeToggledDuringReplay dmod)
            {
                dmod.IsDisabled.BindValueChanged(s =>
                {
                    Colour = s.NewValue ? OsuColour.Gray(0.7f) : Colour = Color4.White;
                });

                action = () =>
                {
                    if (!replayLoaded.Value) return;

                    dmod.IsDisabled.Toggle();
                };
            }
            else
            {
                action = null;
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            action?.Invoke();
            return true;
        }
    }
}
