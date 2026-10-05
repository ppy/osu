// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Overlays.Settings;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A skin component that contains text and allows the user to choose its font.
    /// </summary>
    public abstract partial class FontAdjustableSkinComponent : Container, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Font))]
        public Bindable<Typeface> Font { get; } = new Bindable<Typeface>(Typeface.Torus);

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.TextWeight), SettingControlType = typeof(WeightDropdown))]
        public Bindable<FontWeight> TextWeight { get; } = new Bindable<FontWeight>(FontWeight.Regular);

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.TextColour))]
        public BindableColour4 TextColour { get; } = new BindableColour4(Colour4.White);

        /// <summary>
        /// Implement to apply the user font selection to one or more components.
        /// </summary>
        protected abstract void SetFont(FontUsage font);

        protected abstract void SetTextColour(Colour4 textColour);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Font.BindValueChanged(_ => updateFont());
            TextWeight.BindValueChanged(_ => updateFont(), true);

            TextColour.BindValueChanged(e => SetTextColour(e.NewValue), true);
        }

        private void updateFont() => SetFont(OsuFont.GetFont(Font.Value, weight: TextWeight.Value));

        private partial class WeightDropdown : SettingsDropdown<FontWeight>
        {
            public FontAdjustableSkinComponent FontComponent => (FontAdjustableSkinComponent)SettingSourceObject;
            protected override OsuDropdown<FontWeight> CreateDropdown() => new DropdownControl(this);

            private new partial class DropdownControl : SettingsDropdown<FontWeight>.DropdownControl
            {
                private readonly WeightDropdown settingsDropdown;

                private IBindable<Typeface> font = null!;

                public DropdownControl(WeightDropdown settingsDropdown)
                {
                    this.settingsDropdown = settingsDropdown;
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    font = settingsDropdown.FontComponent.Font.GetBoundCopy();
                    font.BindValueChanged(_ => updateItems(), true);
                }

                private void updateItems()
                {
                    ClearItems();

                    switch (font.Value)
                    {
                        case Typeface.Venera:
                            AddDropdownItem(FontWeight.Light);
                            AddDropdownItem(FontWeight.Bold);
                            AddDropdownItem(FontWeight.Black);

                            Current.Default = FontWeight.Bold;

                            if (!Items.Contains(Current.Value))
                                Current.SetDefault();
                            break;

                        default:
                            AddDropdownItem(FontWeight.Light);
                            AddDropdownItem(FontWeight.Regular);
                            AddDropdownItem(FontWeight.SemiBold);
                            AddDropdownItem(FontWeight.Bold);

                            Current.Default = FontWeight.Regular;

                            if (!Items.Contains(Current.Value))
                                Current.SetDefault();
                            break;
                    }
                }
            }
        }
    }
}
