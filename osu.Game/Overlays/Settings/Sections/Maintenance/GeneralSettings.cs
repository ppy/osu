﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override string Header => "General";

        private TriangleButton importBeatmapsButton;
        private TriangleButton importScoresButton;
        private TriangleButton importSkinsButton;
        private TriangleButton importCollectionsButton;
        private TriangleButton deleteBeatmapsButton;
        private TriangleButton deleteScoresButton;
        private TriangleButton deleteSkinsButton;
        private TriangleButton restoreButton;
        private TriangleButton undeleteButton;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapManager beatmaps, ScoreManager scores, SkinManager skins, [CanBeNull] CollectionManager collectionManager, [CanBeNull] StableImportManager stableImportManager, DialogOverlay dialogOverlay)
        {
            if (stableImportManager?.SupportsImportFromStable == true)
            {
                Add(importBeatmapsButton = new SettingsButton
                {
                    Text = "Import beatmaps from stable",
                    Action = () =>
                    {
                        importBeatmapsButton.Enabled.Value = false;
                        stableImportManager.ImportFromStableAsync(StableContent.Beatmaps).ContinueWith(t => Schedule(() => importBeatmapsButton.Enabled.Value = true));
                    }
                });
            }

            Add(deleteBeatmapsButton = new DangerousSettingsButton
            {
                Text = "Delete ALL beatmaps",
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
                    Text = "Import scores from stable",
                    Action = () =>
                    {
                        importScoresButton.Enabled.Value = false;
                        stableImportManager.ImportFromStableAsync(StableContent.Scores).ContinueWith(t => Schedule(() => importScoresButton.Enabled.Value = true));
                    }
                });
            }

            Add(deleteScoresButton = new DangerousSettingsButton
            {
                Text = "Delete ALL scores",
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
                    Text = "Import skins from stable",
                    Action = () =>
                    {
                        importSkinsButton.Enabled.Value = false;
                        stableImportManager.ImportFromStableAsync(StableContent.Skins).ContinueWith(t => Schedule(() => importSkinsButton.Enabled.Value = true));
                    }
                });
            }

            Add(deleteSkinsButton = new DangerousSettingsButton
            {
                Text = "Delete ALL skins",
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
                        Text = "Import collections from stable",
                        Action = () =>
                        {
                            importCollectionsButton.Enabled.Value = false;
                            stableImportManager.ImportFromStableAsync(StableContent.Collections).ContinueWith(t => Schedule(() => importCollectionsButton.Enabled.Value = true));
                        }
                    });
                }

                Add(new DangerousSettingsButton
                {
                    Text = "Delete ALL collections",
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
                    Text = "Restore all hidden difficulties",
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
                    Text = "Restore all recently deleted beatmaps",
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
