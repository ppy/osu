// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Skinning;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class SkinSettings : SettingsSubsection
    {
        protected override LocalisableString Header => CommonStrings.Skins;

        private SettingsButton deleteSkinsButton = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        private readonly BindableBool performingDeletionOperation = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(IDialogOverlay? dialogOverlay)
        {
            Add(deleteSkinsButton = new DangerousSettingsButton
            {
                Text = MaintenanceSettingsStrings.DeleteAllSkins,
                Action = () =>
                {
                    dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
                    {
                        performingDeletionOperation.Value = true;
                        Task.Run(() => skins.Delete()).ContinueWith(_ => Schedule(() => performingDeletionOperation.Value = false));
                    }));
                }
            });
        }

        private IBindable<Live<SkinInfo>> currentSkinInfo = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentSkinInfo = skins.CurrentSkinInfo.GetBoundCopy();
            currentSkinInfo.BindDisabledChanged(_ => updateDisabledState());
            performingDeletionOperation.BindValueChanged(_ => updateDisabledState(), true);
        }

        private void updateDisabledState() => deleteSkinsButton.Enabled.Value = !currentSkinInfo.Disabled && !performingDeletionOperation.Value;
    }
}
