// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay
{
    /// <summary>
    /// A <see cref="ModSelectOverlay"/> used for free-mod selection in online play.
    /// </summary>
    public class FreeModSelectOverlay : ModSelectOverlay
    {
        protected override bool Stacked => false;

        protected override bool AllowConfiguration => false;

        public new Func<Mod, bool> IsValidMod
        {
            get => base.IsValidMod;
            set => base.IsValidMod = m => m.HasImplementation && !(m is ModAutoplay) && value(m);
        }

        public FreeModSelectOverlay()
        {
            IsValidMod = m => true;

            MultiplierSection.Alpha = 0;
            DeselectAllButton.Alpha = 0;

            Drawable selectAllButton;
            Drawable deselectAllButton;

            FooterContainer.AddRange(new[]
            {
                selectAllButton = new TriangleButton
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    Width = 180,
                    Text = "Select All",
                    Action = selectAll,
                },
                // Unlike the base mod select overlay, this button deselects mods instantaneously.
                deselectAllButton = new TriangleButton
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    Width = 180,
                    Text = "Deselect All",
                    Action = deselectAll,
                },
            });

            FooterContainer.SetLayoutPosition(selectAllButton, -2);
            FooterContainer.SetLayoutPosition(deselectAllButton, -1);
        }

        private void selectAll()
        {
            foreach (var section in ModSectionsContainer.Children)
                section.SelectAll();
        }

        private void deselectAll()
        {
            foreach (var section in ModSectionsContainer.Children)
                section.DeselectAll();
        }

        protected override void OnAvailableModsChanged()
        {
            base.OnAvailableModsChanged();

            foreach (var section in ModSectionsContainer.Children)
                ((FreeModSection)section).UpdateCheckboxState();
        }

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
                RelativeSizeAxes = Axes.X,
                Child = checkbox = new HeaderCheckbox
                {
                    LabelText = text,
                    Changed = onCheckboxChanged
                }
            };

            private void onCheckboxChanged(bool value)
            {
                if (value)
                    SelectAll();
                else
                    DeselectAll();
            }

            protected override void ModButtonStateChanged(Mod mod)
            {
                base.ModButtonStateChanged(mod);
                UpdateCheckboxState();
            }

            public void UpdateCheckboxState()
            {
                if (!SelectionAnimationRunning)
                {
                    var validButtons = Buttons.Where(b => b.Mod.HasImplementation);
                    checkbox.Current.Value = validButtons.All(b => b.Selected);
                }
            }
        }

        private class HeaderCheckbox : OsuCheckbox
        {
            public Action<bool> Changed;

            protected override bool PlaySoundsOnUserChange => false;

            public HeaderCheckbox()
                : base(false)

            {
            }

            protected override void ApplyLabelParameters(SpriteText text)
            {
                base.ApplyLabelParameters(text);

                text.Font = OsuFont.GetFont(weight: FontWeight.Bold);
            }

            protected override void OnUserChange(bool value)
            {
                base.OnUserChange(value);
                Changed?.Invoke(value);
            }
        }
    }
}
