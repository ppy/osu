// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections
{
    public class SkinSection : SettingsSection
    {
        private SkinSettingsDropdown skinDropdown;

        public override string Header => "Skin";

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

        [Resolved]
        private SkinManager skins { get; set; }

        private IBindable<WeakReference<SkinInfo>> managerUpdated;
        private IBindable<WeakReference<SkinInfo>> managerRemoved;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            FlowContent.Spacing = new Vector2(0, 5);

            Children = new Drawable[]
            {
                skinDropdown = new SkinSettingsDropdown(),
                new ExportSkinButton(),
                new SettingsSlider<float, SizeSlider>
                {
                    LabelText = "Gameplay cursor size",
                    Current = config.GetBindable<float>(OsuSetting.GameplayCursorSize),
                    KeyboardStep = 0.01f
                },
                new SettingsCheckbox
                {
                    LabelText = "Adjust gameplay cursor size based on current beatmap",
                    Current = config.GetBindable<bool>(OsuSetting.AutoCursorSize)
                },
                new SettingsCheckbox
                {
                    LabelText = "Beatmap skins",
                    Current = config.GetBindable<bool>(OsuSetting.BeatmapSkins)
                },
                new SettingsCheckbox
                {
                    LabelText = "Beatmap hitsounds",
                    Current = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds)
                },
            };

            managerUpdated = skins.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(itemUpdated);

            managerRemoved = skins.ItemRemoved.GetBoundCopy();
            managerRemoved.BindValueChanged(itemRemoved);

            config.BindWith(OsuSetting.Skin, configBindable);

            skinDropdown.Current = dropdownBindable;
            updateItems();

            // Todo: This should not be necessary when OsuConfigManager is databased
            if (skinDropdown.Items.All(s => s.ID != configBindable.Value))
                configBindable.Value = 0;

            configBindable.BindValueChanged(id => dropdownBindable.Value = skinDropdown.Items.Single(s => s.ID == id.NewValue), true);
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

        private void updateItems()
        {
            skinItems = skins.GetAllUsableSkins();

            // insert after lazer built-in skins
            int firstNonDefault = skinItems.FindIndex(s => s.ID > 0);
            if (firstNonDefault < 0)
                firstNonDefault = skinItems.Count;

            skinItems.Insert(firstNonDefault, random_skin_info);

            skinDropdown.Items = skinItems;
        }

        private void itemUpdated(ValueChangedEvent<WeakReference<SkinInfo>> weakItem)
        {
            if (weakItem.NewValue.TryGetTarget(out var item))
                Schedule(() => skinDropdown.Items = skinDropdown.Items.Where(i => !i.Equals(item)).Append(item).ToArray());
        }

        private void itemRemoved(ValueChangedEvent<WeakReference<SkinInfo>> weakItem)
        {
            if (weakItem.NewValue.TryGetTarget(out var item))
                Schedule(() => skinDropdown.Items = skinDropdown.Items.Where(i => i.ID != item.ID).ToArray());
        }

        private class SkinSettingsDropdown : SettingsDropdown<SkinInfo>
        {
            protected override OsuDropdown<SkinInfo> CreateDropdown() => new SkinDropdownControl();

            private class SkinDropdownControl : DropdownControl
            {
                protected override string GenerateItemText(SkinInfo item) => item.ToString();
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
                Text = "Export selected skin";
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
