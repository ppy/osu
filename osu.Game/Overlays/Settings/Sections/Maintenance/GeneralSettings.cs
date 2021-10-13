// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "General";

        private SettingsButton importBeatmapsButton;
        private SettingsButton importScoresButton;
        private SettingsButton importSkinsButton;
        private SettingsButton importCollectionsButton;
        private SettingsButton deleteBeatmapsButton;
        private SettingsButton deleteScoresButton;
        private SettingsButton deleteSkinsButton;
        private SettingsButton restoreButton;
        private SettingsButton undeleteButton;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapManager beatmaps, ScoreManager scores, SkinManager skins, [CanBeNull] CollectionManager collectionManager, [CanBeNull] StableImportManager stableImportManager, DialogOverlay dialogOverlay)
        {
            if (stableImportManager?.SupportsImportFromStable == true)
            {
                Add(importBeatmapsButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.ImportBeatmapsFromStable,
                    Action = () =>
                    {
                        importBeatmapsButton.Enabled.Value = false;
                        stableImportManager.ImportFromStableAsync(StableContent.Beatmaps).ContinueWith(t => Schedule(() => importBeatmapsButton.Enabled.Value = true));
                    }
                });
            }

            Add(deleteBeatmapsButton = new DangerousSettingsButton
            {
                Text = MaintenanceSettingsStrings.DeleteAllBeatmaps,
                Action = () =>
                {
                    dialogOverlay?.Push(new DeleteAllBeatmapsDialog(() =>
                    {
                        deleteBeatmapsButton.Enabled.Value = false;
                        Task.Run(() => beatmaps.Delete(beatmaps.GetAllUsableBeatmapSets())).ContinueWith(t => Schedule(() => deleteBeatmapsButton.Enabled.Value = true));
                    }));
                }
            });

            if (stableImportManager?.SupportsImportFromStable == true)
            {
                Add(importScoresButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.ImportScoresFromStable,
                    Action = () =>
                    {
                        importScoresButton.Enabled.Value = false;
                        stableImportManager.ImportFromStableAsync(StableContent.Scores).ContinueWith(t => Schedule(() => importScoresButton.Enabled.Value = true));
                    }
                });
            }

            Add(deleteScoresButton = new DangerousSettingsButton
            {
                Text = MaintenanceSettingsStrings.DeleteAllScores,
                Action = () =>
                {
                    dialogOverlay?.Push(new DeleteAllBeatmapsDialog(() =>
                    {
                        deleteScoresButton.Enabled.Value = false;
                        Task.Run(() => scores.Delete(scores.GetAllUsableScores())).ContinueWith(t => Schedule(() => deleteScoresButton.Enabled.Value = true));
                    }));
                }
            });

            if (stableImportManager?.SupportsImportFromStable == true)
            {
                Add(importSkinsButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.ImportSkinsFromStable,
                    Action = () =>
                    {
                        importSkinsButton.Enabled.Value = false;
                        stableImportManager.ImportFromStableAsync(StableContent.Skins).ContinueWith(t => Schedule(() => importSkinsButton.Enabled.Value = true));
                    }
                });
            }

            Add(deleteSkinsButton = new DangerousSettingsButton
            {
                Text = MaintenanceSettingsStrings.DeleteAllSkins,
                Action = () =>
                {
                    dialogOverlay?.Push(new DeleteAllBeatmapsDialog(() =>
                    {
                        deleteSkinsButton.Enabled.Value = false;
                        Task.Run(() => skins.Delete(skins.GetAllUserSkins())).ContinueWith(t => Schedule(() => deleteSkinsButton.Enabled.Value = true));
                    }));
                }
            });

            if (collectionManager != null)
            {
                if (stableImportManager?.SupportsImportFromStable == true)
                {
                    Add(importCollectionsButton = new SettingsButton
                    {
                        Text = MaintenanceSettingsStrings.ImportCollectionsFromStable,
                        Action = () =>
                        {
                            importCollectionsButton.Enabled.Value = false;
                            stableImportManager.ImportFromStableAsync(StableContent.Collections).ContinueWith(t => Schedule(() => importCollectionsButton.Enabled.Value = true));
                        }
                    });
                }

                Add(new DangerousSettingsButton
                {
                    Text = MaintenanceSettingsStrings.DeleteAllCollections,
                    Action = () =>
                    {
                        dialogOverlay?.Push(new DeleteAllBeatmapsDialog(collectionManager.DeleteAll));
                    }
                });
            }

            AddRange(new Drawable[]
            {
                restoreButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.RestoreAllHiddenDifficulties,
                    Action = () =>
                    {
                        restoreButton.Enabled.Value = false;
                        Task.Run(() =>
                        {
                            foreach (var b in beatmaps.QueryBeatmaps(b => b.Hidden).ToList())
                                beatmaps.Restore(b);
                        }).ContinueWith(t => Schedule(() => restoreButton.Enabled.Value = true));
                    }
                },
                undeleteButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.RestoreAllRecentlyDeletedBeatmaps,
                    Action = () =>
                    {
                        undeleteButton.Enabled.Value = false;
                        Task.Run(() => beatmaps.Undelete(beatmaps.QueryBeatmapSets(b => b.DeletePending).ToList())).ContinueWith(t => Schedule(() => undeleteButton.Enabled.Value = true));
                    }
                },
            });
        }
    }
}
