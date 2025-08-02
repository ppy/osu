// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Skinning.Components
{
    public partial class IconElement : Container, ISerialisableDrawable
    {
        private static readonly Type[] all_icon_types = new[]
        {
            typeof(FontAwesome.Regular),
            typeof(FontAwesome.Solid),
            typeof(FontAwesome.Brands),
            typeof(OsuIcon),
        };

        private static readonly IconModel[] all_icons = all_icon_types.SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Static))
                                                                      .Select(p => new IconModel(p.Name, (IconUsage)p.GetValue(null)!))
                                                                      .ToArray();

        private SpriteIcon spriteIcon = null!;

        public bool UsesFixedAnchor { get; set; }

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Icon), SettingControlType = typeof(SettingsIconDropdown))]
        public BindableIcon Icon { get; } = new BindableIcon(all_icons.First());

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.IconColour), nameof(SkinnableComponentStrings.TextColourDescription))]
        public BindableColour4 IconColour { get; } = new BindableColour4(Colour4.White);

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            Child = spriteIcon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(40),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Icon.BindValueChanged(e => spriteIcon.Icon = e.NewValue.Icon, true);
            IconColour.BindValueChanged(e => spriteIcon.Colour = e.NewValue, true);
        }

        public record IconModel(string Name, IconUsage Icon);

        public partial class BindableIcon : Bindable<IconModel>
        {
            public BindableIcon(IconModel defaultValue)
                : base(defaultValue)
            {
            }

            public override void Parse(object? input, IFormatProvider provider)
            {
                // When IconElement is deserialised from the skin, the value is passed here in the form of JObject.
                if (input is JObject jObject)
                    base.Parse(jObject.ToObject<IconModel>(), provider);
                else
                    base.Parse(input, provider);
            }
        }

        private partial class SettingsIconDropdown : SettingsDropdown<IconModel>
        {
            protected override OsuDropdown<IconModel> CreateDropdown() => new IconDropdown();

            private partial class IconDropdown : DropdownControl
            {
                public IconDropdown()
                {
                    foreach (var icon in all_icons)
                        AddDropdownItem(icon);

                    AlwaysShowSearchBar = true;
                }

                protected override LocalisableString GenerateItemText(IconModel item)
                {
                    if (item.Icon.Family is OsuIcon.FONT_NAME or OsuIcon.LEGACY_FONT_NAME)
                        return $"osu!/{item.Name}";

                    return $"{item.Icon.Weight}/{item.Name}";
                }

                protected override DropdownMenu CreateMenu() => new IconDropdownMenu
                {
                    MaxHeight = 200,
                };
            }

            private partial class IconDropdownMenu : DropdownControl.OsuDropdownMenu
            {
                protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new IconDropdownMenuItem(item);
            }

            private partial class IconDropdownMenuItem : DropdownControl.OsuDropdownMenu.DrawableOsuDropdownMenuItem
            {
                protected new IconItemContent Content => (IconItemContent)base.Content;

                public IconDropdownMenuItem(MenuItem item)
                    : base(item)
                {
                    var dropdownItem = (DropdownMenuItem<IconModel>)item;
                    Content.Icon = dropdownItem.Value.Icon;
                }

                protected override Drawable CreateContent() => new IconItemContent();

                protected partial class IconItemContent : ItemContent
                {
                    private readonly SpriteIcon spriteIcon;

                    public IconUsage Icon
                    {
                        get => spriteIcon.Icon;
                        set => spriteIcon.Icon = value;
                    }

                    public IconItemContent()
                    {
                        Label.Padding = new MarginPadding { Left = 35 };

                        AddInternal(spriteIcon = new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            X = 15,
                            Size = new Vector2(16),
                        });
                    }
                }
            }
        }
    }
}
