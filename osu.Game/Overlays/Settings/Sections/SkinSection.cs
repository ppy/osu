// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections
{
    public class SkinSection : SettingsSection
    {
        private SkinSettingsDropdown skinDropdown;

        public override string Header => "皮肤";

        public override IconUsage Icon => FontAwesome.Solid.PaintBrush;

        private readonly Bindable<SkinInfo> dropdownBindable = new Bindable<SkinInfo> { Default = SkinInfo.Default };
        private readonly Bindable<int> configBindable = new Bindable<int>();

        [Resolved]
        private SkinManager skins { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            FlowContent.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                skinDropdown = new SkinSettingsDropdown(),
                new SettingsSlider<float, SizeSlider>
                {
                    LabelText = "菜单光标大小",
                    Bindable = config.GetBindable<float>(OsuSetting.MenuCursorSize),
                    KeyboardStep = 0.01f
                },
                new SettingsSlider<float, SizeSlider>
                {
                    LabelText = "游戏内光标大小",
                    Bindable = config.GetBindable<float>(OsuSetting.GameplayCursorSize),
                    KeyboardStep = 0.01f
                },
                new SettingsCheckbox
                {
                    LabelText = "根据谱面调整光标大小",
                    Bindable = config.GetBindable<bool>(OsuSetting.AutoCursorSize)
                },
                new SettingsCheckbox
                {
                    LabelText = "使用谱面自带皮肤",
                    Bindable = config.GetBindable<bool>(OsuSetting.BeatmapSkins)
                },
                new SettingsCheckbox
                {
                    LabelText = "使用谱面自带音效",
                    Bindable = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds)
                },
            };

            skins.ItemAdded += itemAdded;
            skins.ItemRemoved += itemRemoved;

            config.BindWith(OsuSetting.Skin, configBindable);

            skinDropdown.Bindable = dropdownBindable;
            skinDropdown.Items = skins.GetAllUsableSkins().ToArray();

            // Todo: This should not be necessary when OsuConfigManager is databased
            if (skinDropdown.Items.All(s => s.ID != configBindable.Value))
                configBindable.Value = 0;

            configBindable.BindValueChanged(id => dropdownBindable.Value = skinDropdown.Items.Single(s => s.ID == id.NewValue), true);
            dropdownBindable.BindValueChanged(skin => configBindable.Value = skin.NewValue.ID);
        }

        private void itemRemoved(SkinInfo s) => Schedule(() => skinDropdown.Items = skinDropdown.Items.Where(i => i.ID != s.ID).ToArray());

        private void itemAdded(SkinInfo s) => Schedule(() => skinDropdown.Items = skinDropdown.Items.Append(s).ToArray());

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skins != null)
            {
                skins.ItemAdded -= itemAdded;
                skins.ItemRemoved -= itemRemoved;
            }
        }

        private class SizeSlider : OsuSliderBar<float>
        {
            public override string TooltipText => Current.Value.ToString(@"0.##x");
        }

        private class SkinSettingsDropdown : SettingsDropdown<SkinInfo>
        {
            protected override OsuDropdown<SkinInfo> CreateDropdown() => new SkinDropdownControl();

            private class SkinDropdownControl : DropdownControl
            {
                protected override string GenerateItemText(SkinInfo item) => item.ToString();

                protected override DropdownMenu CreateMenu() => base.CreateMenu().With(m => m.MaxHeight = 200);
            }
        }
    }
}
