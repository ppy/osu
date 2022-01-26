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
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Skinning;
using osu.Game.Skinning.Editor;
using Realms;

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

        private readonly Bindable<Live<SkinInfo>> dropdownBindable = new Bindable<Live<SkinInfo>> { Default = DefaultSkin.CreateInfo().ToLiveUnmanaged() };
        private readonly Bindable<string> configBindable = new Bindable<string>();

        private static readonly Live<SkinInfo> random_skin_info = new SkinInfo
        {
            ID = SkinInfo.RANDOM_SKIN,
            Name = "<Random Skin>",
        }.ToLiveUnmanaged();

        private readonly List<Live<SkinInfo>> dropdownItems = new List<Live<SkinInfo>>();

        [Resolved]
        private SkinManager skins { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; }

        private IDisposable realmSubscription;

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

            config.BindWith(OsuSetting.Skin, configBindable);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            skinDropdown.Current = dropdownBindable;

            realmSubscription = realm.RegisterForNotifications(r => realm.Realm.All<SkinInfo>()
                                                                         .Where(s => !s.DeletePending)
                                                                         .OrderByDescending(s => s.Protected) // protected skins should be at the top.
                                                                         .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase), skinsChanged);

            configBindable.BindValueChanged(id => Scheduler.AddOnce(updateSelectedSkinFromConfig));

            dropdownBindable.BindValueChanged(dropdownSelectionChanged);
        }

        private void dropdownSelectionChanged(ValueChangedEvent<Live<SkinInfo>> skin)
        {
            // Only handle cases where it's clear the user has intent to change skins.
            if (skin.OldValue == null) return;

            if (skin.NewValue.Equals(random_skin_info))
            {
                var skinBefore = skins.CurrentSkinInfo.Value;

                skins.SelectRandomSkin();

                if (skinBefore == skins.CurrentSkinInfo.Value)
                {
                    // the random selection didn't change the skin, so we should manually update the dropdown to match.
                    dropdownBindable.Value = skins.CurrentSkinInfo.Value;
                }

                return;
            }

            configBindable.Value = skin.NewValue.ID.ToString();
        }

        private void skinsChanged(IRealmCollection<SkinInfo> sender, ChangeSet changes, Exception error)
        {
            // This can only mean that realm is recycling, else we would see the protected skins.
            // Because we are using `Live<>` in this class, we don't need to worry about this scenario too much.
            if (!sender.Any())
                return;

            int protectedCount = sender.Count(s => s.Protected);

            // For simplicity repopulate the full list.
            // In the future we should change this to properly handle ChangeSet events.
            dropdownItems.Clear();
            foreach (var skin in sender)
                dropdownItems.Add(skin.ToLive(realm));
            dropdownItems.Insert(protectedCount, random_skin_info);

            skinDropdown.Items = dropdownItems;

            updateSelectedSkinFromConfig();
        }

        private void updateSelectedSkinFromConfig()
        {
            Live<SkinInfo> skin = null;

            if (Guid.TryParse(configBindable.Value, out var configId))
                skin = skinDropdown.Items.FirstOrDefault(s => s.ID == configId);

            dropdownBindable.Value = skin ?? skinDropdown.Items.First();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            realmSubscription?.Dispose();
        }

        private class SkinSettingsDropdown : SettingsDropdown<Live<SkinInfo>>
        {
            protected override OsuDropdown<Live<SkinInfo>> CreateDropdown() => new SkinDropdownControl();

            private class SkinDropdownControl : DropdownControl
            {
                protected override LocalisableString GenerateItemText(Live<SkinInfo> item) => item.ToString();
            }
        }

        public class ExportSkinButton : SettingsButton
        {
            [Resolved]
            private SkinManager skins { get; set; }

            [Resolved]
            private Storage storage { get; set; }

            private Bindable<Skin> currentSkin;

            [BackgroundDependencyLoader]
            private void load()
            {
                Text = SkinSettingsStrings.ExportSkinButton;
                Action = export;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                currentSkin = skins.CurrentSkin.GetBoundCopy();
                currentSkin.BindValueChanged(skin => Enabled.Value = skin.NewValue.SkinInfo.PerformRead(s => !s.Protected), true);
            }

            private void export()
            {
                try
                {
                    currentSkin.Value.SkinInfo.PerformRead(s => new LegacySkinExporter(storage).Export(s));
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not export current skin: {e.Message}", level: LogLevel.Error);
                }
            }
        }
    }
}
