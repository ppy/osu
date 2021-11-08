// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Skinning;
using osu.Game.Skinning.Editor;

namespace osu.Game.Overlays.Settings.Sections
{
    public class SkinSection : SettingsSection
    {
        private SkinSettingsDropdown skinDropdown;

        public override LocalisableString Header => SkinSettingsStrings.SkinSectionHeader;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.PaintBrush
        };

        private readonly Bindable<SkinInfo> dropdownBindable = new Bindable<SkinInfo> { Default = SkinInfo.Default };
        private readonly Bindable<int> configBindable = new Bindable<int>();

        private static readonly SkinInfo random_skin_info = new SkinInfo
        {
            ID = SkinInfo.RANDOM_SKIN,
            Name = "<Random Skin>",
        };

        private List<SkinInfo> skinItems;

        private int firstNonDefaultSkinIndex
        {
            get
            {
                int index = skinItems.FindIndex(s => s.ID > 0);
                if (index < 0)
                    index = skinItems.Count;

                return index;
            }
        }

        [Resolved]
        private SkinManager skins { get; set; }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuConfigManager config, [CanBeNull] SkinEditorOverlay skinEditor)
        {
            Children = new Drawable[]
            {
                skinDropdown = new SkinSettingsDropdown
                {
                    LabelText = SkinSettingsStrings.CurrentSkin
                },
                new SettingsButton
                {
                    Text = SkinSettingsStrings.SkinLayoutEditor,
                    Action = () => skinEditor?.Toggle(),
                },
                new ExportSkinButton(),
            };

            skins.ItemUpdated += itemUpdated;
            skins.ItemRemoved += itemRemoved;

            config.BindWith(OsuSetting.Skin, configBindable);

            skinDropdown.Current = dropdownBindable;
            updateItems();

            // Todo: This should not be necessary when OsuConfigManager is databased
            if (skinDropdown.Items.All(s => s.ID != configBindable.Value))
                configBindable.Value = 0;

            configBindable.BindValueChanged(id => Scheduler.AddOnce(updateSelectedSkinFromConfig), true);
            dropdownBindable.BindValueChanged(skin =>
            {
                if (skin.NewValue == random_skin_info)
                {
                    skins.SelectRandomSkin();
                    return;
                }

                configBindable.Value = skin.NewValue.ID;
            });
        }

        private void updateSelectedSkinFromConfig()
        {
            int id = configBindable.Value;

            var skin = skinDropdown.Items.FirstOrDefault(s => s.ID == id);

            if (skin == null)
            {
                // there may be a thread race condition where an item is selected that hasn't yet been added to the dropdown.
                // to avoid adding complexity, let's just ensure the item is added so we can perform the selection.
                skin = skins.Query(s => s.ID == id);
                addItem(skin);
            }

            dropdownBindable.Value = skin;
        }

        private void updateItems()
        {
            skinItems = skins.GetAllUsableSkins();
            skinItems.Insert(firstNonDefaultSkinIndex, random_skin_info);
            sortUserSkins(skinItems);
            skinDropdown.Items = skinItems;
        }

        private void itemUpdated(SkinInfo item) => Schedule(() => addItem(item));

        private void addItem(SkinInfo item)
        {
            List<SkinInfo> newDropdownItems = skinDropdown.Items.Where(i => !i.Equals(item)).Append(item).ToList();
            sortUserSkins(newDropdownItems);
            skinDropdown.Items = newDropdownItems;
        }

        private void itemRemoved(SkinInfo item) => Schedule(() => skinDropdown.Items = skinDropdown.Items.Where(i => i.ID != item.ID).ToArray());

        private void sortUserSkins(List<SkinInfo> skinsList)
        {
            // Sort user skins separately from built-in skins
            skinsList.Sort(firstNonDefaultSkinIndex, skinsList.Count - firstNonDefaultSkinIndex,
                Comparer<SkinInfo>.Create((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skins != null)
            {
                skins.ItemUpdated -= itemUpdated;
                skins.ItemRemoved -= itemRemoved;
            }
        }

        private class SkinSettingsDropdown : SettingsDropdown<SkinInfo>
        {
            protected override OsuDropdown<SkinInfo> CreateDropdown() => new SkinDropdownControl();

            private class SkinDropdownControl : DropdownControl
            {
                protected override LocalisableString GenerateItemText(SkinInfo item) => item.ToString();
            }
        }

        private class ExportSkinButton : SettingsButton
        {
            [Resolved]
            private SkinManager skins { get; set; }

            private Bindable<Skin> currentSkin;

            [BackgroundDependencyLoader]
            private void load()
            {
                Text = SkinSettingsStrings.ExportSkinButton;
                Action = export;

                currentSkin = skins.CurrentSkin.GetBoundCopy();
                currentSkin.BindValueChanged(skin => Enabled.Value = skin.NewValue.SkinInfo.ID > 0, true);
            }

            private void export()
            {
                try
                {
                    skins.Export(currentSkin.Value.SkinInfo);
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not export current skin: {e.Message}", level: LogLevel.Error);
                }
            }
        }
    }
}
