// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class FreeModSelectOverlay : ModSelectOverlay
    {
        public FreeModSelectOverlay(Func<Mod, bool> isValidMod = null)
            : base(isValidMod)
        {
        }

        protected override ModSection CreateModSection(ModType type) => new FreeModSection(type);

        private class FreeModSection : ModSection
        {
            private HeaderCheckbox checkbox;

            public FreeModSection(ModType type)
                : base(type)
            {
            }

            protected override ModButton CreateModButton(Mod mod) => new FreeModButton(mod);

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
                        // Note: Buttons where only part of the group has an implementation are not fully supported.
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

        private class FreeModButton : ModButton
        {
            public FreeModButton(Mod mod)
                : base(mod)
            {
            }

            protected override bool OnClick(ClickEvent e)
            {
                onClick();
                return true;
            }

            protected override void OnRightClick(MouseUpEvent e) => onClick();

            private void onClick()
            {
                if (Selected)
                    Deselect();
                else
                    SelectNext(1);
            }
        }
    }
}
