// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Match
{
    /// <summary>
    /// A <see cref="ModSelectOverlay"/> used for free-mod selection in online play.
    /// </summary>
    public class FreeModSelectOverlay : ModSelectOverlay
    {
        protected override bool AllowCustomisation => false;

        protected override bool Stacked => false;

        protected override ModSection CreateModSection(ModType type) => new FreeModSection(type);

        private class FreeModSection : ModSection
        {
            private HeaderCheckbox checkbox;

            public FreeModSection(ModType type)
                : base(type)
            {
            }

            protected override Drawable CreateHeader(string text) => new Container
            {
                AutoSizeAxes = Axes.Y,
                Width = 175,
                Child = checkbox = new HeaderCheckbox
                {
                    LabelText = text,
                    Changed = onCheckboxChanged
                }
            };

            private void onCheckboxChanged(bool value)
            {
                foreach (var button in ButtonsContainer.OfType<ModButton>())
                {
                    if (value)
                        button.SelectAt(0);
                    else
                        button.Deselect();
                }
            }

            protected override void Update()
            {
                base.Update();

                // If any of the buttons aren't selected, deselect the checkbox.
                foreach (var button in ButtonsContainer.OfType<ModButton>())
                {
                    if (button.Mods.Any(m => m.HasImplementation) && !button.Selected)
                        checkbox.Current.Value = false;
                }
            }
        }

        private class HeaderCheckbox : OsuCheckbox
        {
            public Action<bool> Changed;

            protected override void OnUserChange(bool value)
            {
                base.OnUserChange(value);
                Changed?.Invoke(value);
            }
        }
    }
}
