// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Storyboards;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneEditorBeatmapCreation : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override bool IsolateSavingFromDatabase => false;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private Guid currentBeatmapSetID => EditorBeatmap.BeatmapInfo.BeatmapSet?.ID ?? Guid.Empty;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            // if we save a beatmap with a hash collision, things fall over.
            // probably needs a more solid resolution in the future but this will do for now.
            AddStep("make new beatmap unique", () => EditorBeatmap.Metadata.Title = Guid.NewGuid().ToString());
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null) => new DummyWorkingBeatmap(Audio, null);

        [Test]
        public void TestCreateNewBeatmap()
        {
            AddAssert("status is none", () => EditorBeatmap.BeatmapInfo.Status == BeatmapOnlineStatus.None);
            AddStep("save beatmap", () => Editor.Save());
            AddAssert("new beatmap in database", () => beatmapManager.QueryBeatmapSet(s => s.ID == currentBeatmapSetID)?.Value.DeletePending == false);
            AddAssert("status is modified", () => EditorBeatmap.BeatmapInfo.Status == BeatmapOnlineStatus.LocallyModified);
        }

        [Test]
        public void TestExitWithoutSave()
        {
            EditorBeatmap editorBeatmap = null!;

            AddStep("store editor beatmap", () => editorBeatmap = EditorBeatmap);

            AddStep("exit without save", () => Editor.Exit());
            AddStep("hold to confirm", () =>
            {
                var confirmButton = DialogOverlay.CurrentDialog.ChildrenOfType<PopupDialogDangerousButton>().First();

                InputManager.MoveMouseTo(confirmButton);
                InputManager.PressButton(MouseButton.Left);
            });

            AddUntilStep("wait for exit", () => !Editor.IsCurrentScreen());
            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("new beatmap not persisted", () => beatmapManager.QueryBeatmapSet(s => s.ID == editorBeatmap.BeatmapInfo.BeatmapSet.AsNonNull().ID)?.Value.DeletePending == true);
        }

        [Test]
        public void TestAddAudioTrack()
        {
            AddStep("enter compose mode", () => InputManager.Key(Key.F1));
            AddUntilStep("wait for timeline load", () => Editor.ChildrenOfType<Timeline>().FirstOrDefault()?.IsLoaded == true);

            AddStep("enter setup mode", () => InputManager.Key(Key.F4));
            AddAssert("track is virtual", () => ((LoggingTrack)Beatmap.Value.Track).UnderlyingTrack is TrackVirtual);
            AddAssert("switch track to real track", () =>
            {
                var setup = Editor.ChildrenOfType<SetupScreen>().First();

                string temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    using (var zip = ZipArchive.Open(temp))
                        zip.WriteToDirectory(extractedFolder);

                    bool success = setup.ChildrenOfType<ResourcesSection>().First().ChangeAudioTrack(new FileInfo(Path.Combine(extractedFolder, "03. Renatus - Soleily 192kbps.mp3")));

                    // ensure audio file is copied to beatmap as "audio.mp3" rather than original filename.
                    Assert.That(Beatmap.Value.Metadata.AudioFile == "audio.mp3");

                    return success;
                }
                finally
                {
                    File.Delete(temp);
                    Directory.Delete(extractedFolder, true);
                }
            });

            AddAssert("track is not virtual", () => Beatmap.Value.Track is not TrackVirtual);
            AddUntilStep("track length changed", () => Beatmap.Value.Track.Length > 60000);

            AddStep("test play", () => Editor.TestGameplay());

            AddUntilStep("wait for dialog", () => DialogOverlay.CurrentDialog != null);
            AddStep("confirm save", () => InputManager.Key(Key.Number1));

            AddUntilStep("wait for return to editor", () => Editor.IsCurrentScreen());

            AddAssert("track is still not virtual", () => Beatmap.Value.Track is not TrackVirtual);
            AddAssert("track length correct", () => Beatmap.Value.Track.Length > 60000);

            AddUntilStep("track not playing", () => !EditorClock.IsRunning);
            AddStep("play track", () => InputManager.Key(Key.Space));
            AddUntilStep("wait for track playing", () => EditorClock.IsRunning);
        }

        [Test]
        public void TestCreateNewDifficulty([Values] bool sameRuleset)
        {
            string firstDifficultyName = Guid.NewGuid().ToString();
            string secondDifficultyName = Guid.NewGuid().ToString();

            AddStep("set unique difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = firstDifficultyName);
            AddStep("add timing point", () => EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 1000 }));
            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new[]
            {
                new HitCircle
                {
                    Position = new Vector2(0),
                    StartTime = 0
                },
                new HitCircle
                {
                    Position = OsuPlayfield.BASE_SIZE,
                    StartTime = 1000
                }
            }));

            AddStep("save beatmap", () => Editor.Save());
            AddAssert("new beatmap persisted", () =>
            {
                var beatmap = beatmapManager.QueryBeatmap(b => b.DifficultyName == firstDifficultyName);
                var set = beatmapManager.QueryBeatmapSet(s => s.ID == currentBeatmapSetID);

                return beatmap != null
                       && beatmap.DifficultyName == firstDifficultyName
                       && set != null
                       && set.PerformRead(s => s.Beatmaps.Single().ID == beatmap.ID);
            });
            AddAssert("can save again", () => Editor.Save());

            AddStep("create new difficulty", () => Editor.CreateNewDifficulty(sameRuleset ? new OsuRuleset().RulesetInfo : new CatchRuleset().RulesetInfo));

            if (sameRuleset)
            {
                AddUntilStep("wait for dialog", () => DialogOverlay.CurrentDialog is CreateNewDifficultyDialog);
                AddStep("confirm creation with no objects", () => DialogOverlay.CurrentDialog!.PerformOkAction());
            }

            AddUntilStep("wait for created", () =>
            {
                string? difficultyName = Editor.ChildrenOfType<EditorBeatmap>().SingleOrDefault()?.BeatmapInfo.DifficultyName;
                return difficultyName != null && difficultyName != firstDifficultyName;
            });

            AddAssert("created difficulty has timing point", () =>
            {
                var timingPoint = EditorBeatmap.ControlPointInfo.TimingPoints.Single();
                return timingPoint.Time == 0 && timingPoint.BeatLength == 1000;
            });
            AddAssert("created difficulty has no objects", () => EditorBeatmap.HitObjects.Count == 0);

            AddAssert("status is modified", () => EditorBeatmap.BeatmapInfo.Status == BeatmapOnlineStatus.LocallyModified);

            AddStep("set unique difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = secondDifficultyName);
            AddStep("save beatmap", () => Editor.Save());
            AddAssert("new beatmap persisted", () =>
            {
                var beatmap = beatmapManager.QueryBeatmap(b => b.DifficultyName == secondDifficultyName);
                var set = beatmapManager.QueryBeatmapSet(s => s.ID == currentBeatmapSetID);

                return beatmap != null
                       && beatmap.DifficultyName == secondDifficultyName
                       && set != null
                       && set.PerformRead(s =>
                           s.Beatmaps.Count == 2 && s.Beatmaps.Any(b => b.DifficultyName == secondDifficultyName) && s.Beatmaps.All(b => s.Status == BeatmapOnlineStatus.LocallyModified));
            });
        }

        [Test]
        public void TestCopyDifficulty()
        {
            string originalDifficultyName = Guid.NewGuid().ToString();
            string copyDifficultyName = $"{originalDifficultyName} (copy)";

            AddStep("set unique difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = originalDifficultyName);
            AddStep("add timing point", () => EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 1000 }));
            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new[]
            {
                new HitCircle
                {
                    Position = new Vector2(0),
                    StartTime = 0
                },
                new HitCircle
                {
                    Position = OsuPlayfield.BASE_SIZE,
                    StartTime = 1000
                }
            }));
            AddStep("set approach rate", () => EditorBeatmap.Difficulty.ApproachRate = 4);
            AddStep("set combo colours", () =>
            {
                var beatmapSkin = EditorBeatmap.BeatmapSkin.AsNonNull();
                beatmapSkin.ComboColours.Clear();
                beatmapSkin.ComboColours.AddRange(new[]
                {
                    new Colour4(255, 0, 0, 255),
                    new Colour4(0, 0, 255, 255)
                });
            });
            AddStep("set status & online ID", () =>
            {
                EditorBeatmap.BeatmapInfo.OnlineID = 123456;
                EditorBeatmap.BeatmapInfo.Status = BeatmapOnlineStatus.WIP;
            });

            AddStep("save beatmap", () => Editor.Save());
            AddAssert("new beatmap persisted", () =>
            {
                var beatmap = beatmapManager.QueryBeatmap(b => b.DifficultyName == originalDifficultyName);
                var set = beatmapManager.QueryBeatmapSet(s => s.ID == currentBeatmapSetID);

                return beatmap != null
                       && beatmap.DifficultyName == originalDifficultyName
                       && set != null
                       && set.PerformRead(s => s.Beatmaps.Single().ID == beatmap.ID);
            });
            AddAssert("can save again", () => Editor.Save());

            AddStep("create new difficulty", () => Editor.CreateNewDifficulty(new OsuRuleset().RulesetInfo));

            AddUntilStep("wait for dialog", () => DialogOverlay.CurrentDialog is CreateNewDifficultyDialog);
            AddStep("confirm creation as a copy", () => DialogOverlay.CurrentDialog!.Buttons.ElementAt(1).TriggerClick());

            AddUntilStep("wait for created", () =>
            {
                string? difficultyName = Editor.ChildrenOfType<EditorBeatmap>().SingleOrDefault()?.BeatmapInfo.DifficultyName;
                return difficultyName != null && difficultyName != originalDifficultyName;
            });

            AddAssert("created difficulty has copy suffix in name", () => EditorBeatmap.BeatmapInfo.DifficultyName == copyDifficultyName);
            AddAssert("created difficulty has timing point", () =>
            {
                var timingPoint = EditorBeatmap.ControlPointInfo.TimingPoints.Single();
                return timingPoint.Time == 0 && timingPoint.BeatLength == 1000;
            });
            AddAssert("created difficulty has objects", () => EditorBeatmap.HitObjects.Count == 2);
            AddAssert("approach rate correctly copied", () => EditorBeatmap.Difficulty.ApproachRate == 4);
            AddAssert("combo colours correctly copied", () => EditorBeatmap.BeatmapSkin.AsNonNull().ComboColours.Count == 2);

            AddAssert("status is modified", () => EditorBeatmap.BeatmapInfo.Status == BeatmapOnlineStatus.LocallyModified);
            AddAssert("online ID not copied", () => EditorBeatmap.BeatmapInfo.OnlineID == -1);

            AddStep("save beatmap", () => Editor.Save());

            BeatmapInfo? refetchedBeatmap = null;
            Live<BeatmapSetInfo>? refetchedBeatmapSet = null;

            AddStep("refetch from database", () =>
            {
                refetchedBeatmap = beatmapManager.QueryBeatmap(b => b.DifficultyName == copyDifficultyName);
                refetchedBeatmapSet = beatmapManager.QueryBeatmapSet(s => s.ID == currentBeatmapSetID);
            });

            AddAssert("new beatmap persisted", () =>
            {
                return refetchedBeatmap != null
                       && refetchedBeatmap.DifficultyName == copyDifficultyName
                       && refetchedBeatmapSet != null
                       && refetchedBeatmapSet.PerformRead(s =>
                           s.Beatmaps.Count == 2
                           && s.Beatmaps.Any(b => b.DifficultyName == originalDifficultyName)
                           && s.Beatmaps.Any(b => b.DifficultyName == copyDifficultyName));
            });
            AddAssert("old beatmap file not deleted", () => refetchedBeatmapSet.AsNonNull().PerformRead(s => s.Files.Count == 2));
        }

        [Test]
        public void TestCopyDifficultyDoesNotChangeCollections()
        {
            string originalDifficultyName = Guid.NewGuid().ToString();

            AddStep("set unique difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = originalDifficultyName);
            AddStep("save beatmap", () => Editor.Save());

            string originalMd5 = string.Empty;
            BeatmapCollection collection = null!;

            AddStep("setup a collection with original beatmap", () =>
            {
                collection = new BeatmapCollection("test copy");
                collection.BeatmapMD5Hashes.Add(originalMd5 = EditorBeatmap.BeatmapInfo.MD5Hash);

                realm.Write(r =>
                {
                    r.Add(collection);
                });
            });

            AddAssert("collection contains original beatmap", () =>
                !string.IsNullOrEmpty(originalMd5) && collection.BeatmapMD5Hashes.Contains(originalMd5));

            AddStep("create new difficulty", () => Editor.CreateNewDifficulty(new OsuRuleset().RulesetInfo));

            AddUntilStep("wait for dialog", () => DialogOverlay.CurrentDialog is CreateNewDifficultyDialog);
            AddStep("confirm creation as a copy", () => DialogOverlay.CurrentDialog!.Buttons.ElementAt(1).TriggerClick());

            AddUntilStep("wait for created", () =>
            {
                string? difficultyName = Editor.ChildrenOfType<EditorBeatmap>().SingleOrDefault()?.BeatmapInfo.DifficultyName;
                return difficultyName != null && difficultyName != originalDifficultyName;
            });

            AddStep("save without changes", () => Editor.Save());

            AddAssert("collection still points to old beatmap", () => !collection.BeatmapMD5Hashes.Contains(EditorBeatmap.BeatmapInfo.MD5Hash)
                                                                      && collection.BeatmapMD5Hashes.Contains(originalMd5));

            AddStep("clean up collection", () =>
            {
                realm.Write(r =>
                {
                    r.Remove(collection);
                });
            });
        }

        [Test]
        public void TestCreateMultipleNewDifficultiesSucceeds()
        {
            Guid setId = Guid.Empty;

            AddStep("retrieve set ID", () => setId = EditorBeatmap.BeatmapInfo.BeatmapSet!.ID);
            AddStep("set difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = "New Difficulty");
            AddStep("save beatmap", () => Editor.Save());
            AddAssert("new beatmap persisted", () =>
            {
                var set = beatmapManager.QueryBeatmapSet(s => s.ID == setId);
                return set != null && set.PerformRead(s => s.Beatmaps.Count == 1 && s.Files.Count == 1);
            });

            AddStep("try to create new difficulty", () => Editor.CreateNewDifficulty(new OsuRuleset().RulesetInfo));
            AddUntilStep("wait for dialog", () => DialogOverlay.CurrentDialog is CreateNewDifficultyDialog);
            AddStep("confirm creation with no objects", () => DialogOverlay.CurrentDialog!.PerformOkAction());

            AddUntilStep("wait for created", () =>
            {
                string? difficultyName = Editor.ChildrenOfType<EditorBeatmap>().SingleOrDefault()?.BeatmapInfo.DifficultyName;
                return difficultyName != null && difficultyName != "New Difficulty";
            });
            AddAssert("new difficulty has correct name", () => EditorBeatmap.BeatmapInfo.DifficultyName == "New Difficulty (1)");
            AddAssert("new difficulty persisted", () =>
            {
                var set = beatmapManager.QueryBeatmapSet(s => s.ID == setId);
                return set != null && set.PerformRead(s => s.Beatmaps.Count == 2 && s.Files.Count == 2);
            });
        }

        [Test]
        public void TestSavingBeatmapFailsWithSameNamedDifficulties([Values] bool sameRuleset)
        {
            Guid setId = Guid.Empty;
            const string duplicate_difficulty_name = "duplicate";

            AddStep("retrieve set ID", () => setId = EditorBeatmap.BeatmapInfo.BeatmapSet!.ID);
            AddStep("set difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = duplicate_difficulty_name);
            AddStep("save beatmap", () => Editor.Save());
            AddAssert("new beatmap persisted", () =>
            {
                var set = beatmapManager.QueryBeatmapSet(s => s.ID == setId);
                return set != null && set.PerformRead(s => s.Beatmaps.Count == 1 && s.Files.Count == 1);
            });

            AddStep("create new difficulty", () => Editor.CreateNewDifficulty(sameRuleset ? new OsuRuleset().RulesetInfo : new CatchRuleset().RulesetInfo));

            if (sameRuleset)
            {
                AddUntilStep("wait for dialog", () => DialogOverlay.CurrentDialog is CreateNewDifficultyDialog);
                AddStep("confirm creation with no objects", () => DialogOverlay.CurrentDialog!.PerformOkAction());
            }

            AddUntilStep("wait for created", () =>
            {
                string? difficultyName = Editor.ChildrenOfType<EditorBeatmap>().SingleOrDefault()?.BeatmapInfo.DifficultyName;
                return difficultyName != null && difficultyName != duplicate_difficulty_name;
            });

            AddStep("set difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = duplicate_difficulty_name);
            AddStep("try to save beatmap", () => Editor.Save());
            AddAssert("beatmap set not corrupted", () =>
            {
                var set = beatmapManager.QueryBeatmapSet(s => s.ID == setId);
                // the difficulty was already created at the point of the switch.
                // what we want to check is that both difficulties do not use the same file.
                return set != null && set.PerformRead(s => s.Beatmaps.Count == 2 && s.Files.Count == 2);
            });
        }

        [Test]
        public void TestExitBlockedWhenSavingBeatmapWithSameNamedDifficulties()
        {
            Guid setId = Guid.Empty;
            const string duplicate_difficulty_name = "duplicate";

            AddStep("retrieve set ID", () => setId = EditorBeatmap.BeatmapInfo.BeatmapSet!.ID);
            AddStep("set difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = duplicate_difficulty_name);
            AddStep("save beatmap", () => Editor.Save());
            AddAssert("new beatmap persisted", () =>
            {
                var set = beatmapManager.QueryBeatmapSet(s => s.ID == setId);
                return set != null && set.PerformRead(s => s.Beatmaps.Count == 1 && s.Files.Count == 1);
            });

            AddStep("create new difficulty", () => Editor.CreateNewDifficulty(new CatchRuleset().RulesetInfo));

            AddUntilStep("wait for created", () =>
            {
                string? difficultyName = Editor.ChildrenOfType<EditorBeatmap>().SingleOrDefault()?.BeatmapInfo.DifficultyName;
                return difficultyName != null && difficultyName != duplicate_difficulty_name;
            });
            AddUntilStep("wait for editor load", () => Editor.IsLoaded && DialogOverlay.IsLoaded);

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new[]
            {
                new Fruit
                {
                    StartTime = 0
                },
                new Fruit
                {
                    StartTime = 1000
                }
            }));

            AddStep("set difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = duplicate_difficulty_name);
            AddUntilStep("wait for has unsaved changes", () => Editor.HasUnsavedChanges);

            AddStep("exit", () => Editor.Exit());
            AddUntilStep("wait for dialog", () => DialogOverlay.CurrentDialog is PromptForSaveDialog);
            AddStep("attempt to save", () => DialogOverlay.CurrentDialog!.PerformOkAction());
            AddAssert("editor is still current", () => Editor.IsCurrentScreen());
        }

        [Test]
        public void TestCreateNewDifficultyForInconvertibleRuleset()
        {
            Guid setId = Guid.Empty;

            AddStep("retrieve set ID", () => setId = EditorBeatmap.BeatmapInfo.BeatmapSet!.ID);
            AddStep("save beatmap", () => Editor.Save());
            AddStep("try to create new taiko difficulty", () => Editor.CreateNewDifficulty(new TaikoRuleset().RulesetInfo));

            AddUntilStep("wait for created", () =>
            {
                string? difficultyName = Editor.ChildrenOfType<EditorBeatmap>().SingleOrDefault()?.BeatmapInfo.DifficultyName;
                return difficultyName != null && difficultyName == "New Difficulty";
            });
            AddAssert("new difficulty persisted", () =>
            {
                var set = beatmapManager.QueryBeatmapSet(s => s.ID == setId);
                return set != null && set.PerformRead(s => s.Beatmaps.Count == 2 && s.Files.Count == 2);
            });

            AddStep("add timing point", () => EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 1000 }));
            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new[]
            {
                new Hit
                {
                    StartTime = 0
                },
                new Hit
                {
                    StartTime = 1000
                }
            }));
            AddStep("save beatmap", () => Editor.Save());
            AddStep("try to create new catch difficulty", () => Editor.CreateNewDifficulty(new CatchRuleset().RulesetInfo));

            AddUntilStep("wait for created", () =>
            {
                string? difficultyName = Editor.ChildrenOfType<EditorBeatmap>().SingleOrDefault()?.BeatmapInfo.DifficultyName;
                return difficultyName != null && difficultyName == "New Difficulty (1)";
            });
            AddAssert("new difficulty persisted", () =>
            {
                var set = beatmapManager.QueryBeatmapSet(s => s.ID == setId);
                return set != null && set.PerformRead(s => s.Beatmaps.Count == 3 && s.Files.Count == 3);
            });
        }
    }
}
