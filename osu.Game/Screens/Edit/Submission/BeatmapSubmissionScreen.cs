// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO.Archives;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.Edit.Submission
{
    public partial class BeatmapSubmissionScreen : OsuScreen
    {
        private BeatmapSubmissionOverlay overlay = null!;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        protected override bool InitialBackButtonVisibility => false;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private Storage storage { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Cached]
        private BeatmapSubmissionSettings settings { get; } = new BeatmapSubmissionSettings();

        private Container submissionProgress = null!;
        private SubmissionStageProgress exportStep = null!;
        private SubmissionStageProgress createSetStep = null!;
        private SubmissionStageProgress uploadStep = null!;
        private SubmissionStageProgress updateStep = null!;
        private Container successContainer = null!;
        private Container flashLayer = null!;

        private uint? beatmapSetId;
        private MemoryStream? beatmapPackageStream;

        private ProgressNotification? exportProgressNotification;
        private ProgressNotification? updateProgressNotification;

        private Live<BeatmapSetInfo>? importedSet;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                overlay = new BeatmapSubmissionOverlay(),
                submissionProgress = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = 400,
                    AutoSizeEasing = Easing.OutQuint,
                    Alpha = 0,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.6f,
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding(20),
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                createSetStep = new SubmissionStageProgress
                                {
                                    StageDescription = BeatmapSubmissionStrings.Preparing,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                                exportStep = new SubmissionStageProgress
                                {
                                    StageDescription = BeatmapSubmissionStrings.Exporting,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                                uploadStep = new SubmissionStageProgress
                                {
                                    StageDescription = BeatmapSubmissionStrings.Uploading,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                                updateStep = new SubmissionStageProgress
                                {
                                    StageDescription = BeatmapSubmissionStrings.Finishing,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                                successContainer = new Container
                                {
                                    Padding = new MarginPadding(20),
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    AutoSizeAxes = Axes.Both,
                                    CornerRadius = BeatmapCard.CORNER_RADIUS,
                                    Child = flashLayer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Masking = true,
                                        CornerRadius = BeatmapCard.CORNER_RADIUS,
                                        Depth = float.MinValue,
                                        Alpha = 0,
                                        Child = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        }
                                    }
                                },
                            }
                        }
                    }
                }
            });

            overlay.State.BindValueChanged(_ =>
            {
                if (overlay.State.Value == Visibility.Hidden)
                {
                    if (!overlay.Completed)
                    {
                        allowExit();
                        this.Exit();
                    }
                    else
                    {
                        submissionProgress.FadeIn(200, Easing.OutQuint);
                        createBeatmapSet();
                    }
                }
            });
        }

        private void createBeatmapSet()
        {
            bool beatmapHasOnlineId = Beatmap.Value.BeatmapSetInfo.OnlineID > 0;

            var createRequest = beatmapHasOnlineId
                ? PutBeatmapSetRequest.UpdateExisting(
                    (uint)Beatmap.Value.BeatmapSetInfo.OnlineID,
                    Beatmap.Value.BeatmapSetInfo.Beatmaps.Where(b => b.OnlineID > 0).Select(b => (uint)b.OnlineID).ToArray(),
                    (uint)Beatmap.Value.BeatmapSetInfo.Beatmaps.Count(b => b.OnlineID <= 0),
                    settings)
                : PutBeatmapSetRequest.CreateNew((uint)Beatmap.Value.BeatmapSetInfo.Beatmaps.Count, settings);

            createRequest.Success += async response =>
            {
                createSetStep.SetCompleted();
                beatmapSetId = response.BeatmapSetId;

                // at this point the set has an assigned online ID.
                // it's important to proactively store it to the realm database,
                // so that in the event in further failures in the process, the online ID is not lost.
                // losing it can incur creation of redundant new sets server-side, or even cause online ID confusion.
                if (!beatmapHasOnlineId)
                {
                    await realmAccess.WriteAsync(r =>
                    {
                        var refetchedSet = r.Find<BeatmapSetInfo>(Beatmap.Value.BeatmapSetInfo.ID);
                        refetchedSet!.OnlineID = (int)beatmapSetId.Value;
                    }).ConfigureAwait(true);
                }

                await createBeatmapPackage(response).ConfigureAwait(true);
            };
            createRequest.Failure += ex =>
            {
                createSetStep.SetFailed(ex.Message);
                Logger.Log($"Beatmap set submission failed on creation: {ex}");
                allowExit();
            };

            createSetStep.SetInProgress();
            api.Queue(createRequest);
        }

        private async Task createBeatmapPackage(PutBeatmapSetResponse response)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            exportStep.SetInProgress();

            try
            {
                beatmapPackageStream = new MemoryStream();
                exportProgressNotification = new ProgressNotification();

                var legacyBeatmapExporter = new SubmissionBeatmapExporter(storage, response);

                await legacyBeatmapExporter
                      .ExportToStreamAsync(Beatmap.Value.BeatmapSetInfo.ToLive(realmAccess), beatmapPackageStream, exportProgressNotification)
                      .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                exportStep.SetFailed(ex.Message);
                exportProgressNotification = null;
                Logger.Log($"Beatmap set submission failed on export: {ex}");
                allowExit();
                return;
            }

            exportStep.SetCompleted();
            exportProgressNotification = null;

            await Task.Delay(200).ConfigureAwait(true);

            if (response.Files.Count > 0)
                await patchBeatmapSet(response.Files).ConfigureAwait(true);
            else
                replaceBeatmapSet();
        }

        private async Task patchBeatmapSet(ICollection<BeatmapSetFile> onlineFiles)
        {
            Debug.Assert(beatmapSetId != null);
            Debug.Assert(beatmapPackageStream != null);

            var onlineFilesByFilename = onlineFiles.ToDictionary(f => f.Filename, f => f.SHA2Hash);

            // disposing the `ArchiveReader` makes the underlying stream no longer readable which we don't want.
            // make a local copy to defend against it.
            using var archiveReader = new ZipArchiveReader(new MemoryStream(beatmapPackageStream.ToArray()));
            var filesToUpdate = new HashSet<string>();

            foreach (string filename in archiveReader.Filenames)
            {
                string localHash = archiveReader.GetStream(filename).ComputeSHA2Hash();

                if (!onlineFilesByFilename.Remove(filename, out string? onlineHash))
                {
                    filesToUpdate.Add(filename);
                    continue;
                }

                if (!localHash.Equals(onlineHash, StringComparison.OrdinalIgnoreCase))
                    filesToUpdate.Add(filename);
            }

            var changedFiles = new Dictionary<string, byte[]>();

            foreach (string file in filesToUpdate)
                changedFiles.Add(file, await archiveReader.GetStream(file).ReadAllBytesToArrayAsync().ConfigureAwait(true));

            var patchRequest = new PatchBeatmapPackageRequest(beatmapSetId.Value);
            patchRequest.FilesChanged.AddRange(changedFiles);
            patchRequest.FilesDeleted.AddRange(onlineFilesByFilename.Keys);
            patchRequest.Success += uploadCompleted;
            patchRequest.Failure += ex =>
            {
                uploadStep.SetFailed(ex.Message);
                Logger.Log($"Beatmap submission failed on upload: {ex}");
                allowExit();
            };
            patchRequest.Progressed += (current, total) => uploadStep.SetInProgress(total > 0 ? (float)current / total : null);

            api.Queue(patchRequest);
            uploadStep.SetInProgress();
        }

        private void replaceBeatmapSet()
        {
            Debug.Assert(beatmapSetId != null);
            Debug.Assert(beatmapPackageStream != null);

            var uploadRequest = new ReplaceBeatmapPackageRequest(beatmapSetId.Value, beatmapPackageStream.ToArray());

            uploadRequest.Success += uploadCompleted;
            uploadRequest.Failure += ex =>
            {
                uploadStep.SetFailed(ex.Message);
                Logger.Log($"Beatmap submission failed on upload: {ex}");
                allowExit();
            };
            uploadRequest.Progressed += (current, total) => uploadStep.SetInProgress((float)current / Math.Max(total, 1));

            api.Queue(uploadRequest);
            uploadStep.SetInProgress();
        }

        private void uploadCompleted()
        {
            uploadStep.SetCompleted();
            updateLocalBeatmap().ConfigureAwait(true);
        }

        private async Task updateLocalBeatmap()
        {
            Debug.Assert(beatmapSetId != null);
            Debug.Assert(beatmapPackageStream != null);

            updateStep.SetInProgress();
            await Task.Delay(200).ConfigureAwait(true);

            try
            {
                importedSet = await beatmaps.ImportAsUpdate(
                    updateProgressNotification = new ProgressNotification(),
                    new ImportTask(beatmapPackageStream, $"{beatmapSetId}.osz"),
                    Beatmap.Value.BeatmapSetInfo).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                updateStep.SetFailed(ex.Message);
                Logger.Log($"Beatmap submission failed on local update: {ex}");
                allowExit();
                return;
            }

            updateStep.SetCompleted();
            showBeatmapCard();
            allowExit();

            if (configManager.Get<bool>(OsuSetting.EditorSubmissionLoadInBrowserAfterSubmission))
            {
                await Task.Delay(1000).ConfigureAwait(true);
                game?.OpenUrlExternally($"{api.Endpoints.WebsiteUrl}/beatmapsets/{beatmapSetId}");
            }
        }

        private void showBeatmapCard()
        {
            Debug.Assert(beatmapSetId != null);

            var getBeatmapSetRequest = new GetBeatmapSetRequest((int)beatmapSetId.Value);
            getBeatmapSetRequest.Success += beatmapSet =>
            {
                LoadComponentAsync(new BeatmapCardExtra(beatmapSet, false), loaded =>
                {
                    successContainer.Add(loaded);
                    flashLayer.FadeOutFromOne(2000, Easing.OutQuint);
                });
            };

            api.Queue(getBeatmapSetRequest);
        }

        private void allowExit()
        {
            BackButtonVisibility.Value = true;
        }

        protected override void Update()
        {
            base.Update();

            if (exportProgressNotification != null && exportProgressNotification.Ongoing)
                exportStep.SetInProgress(exportProgressNotification.Progress);

            if (updateProgressNotification != null && updateProgressNotification.Ongoing)
                updateStep.SetInProgress(updateProgressNotification.Progress);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // We probably want a method of cancelling in the future…
            if (!BackButtonVisibility.Value)
                return true;

            if (importedSet != null)
            {
                game?.PerformFromScreen(s =>
                {
                    if (s is OsuScreen osuScreen)
                    {
                        Debug.Assert(importedSet != null);
                        var targetBeatmap = importedSet.Value.Beatmaps.FirstOrDefault(b => b.DifficultyName == Beatmap.Value.BeatmapInfo.DifficultyName)
                                            ?? importedSet.Value.Beatmaps.First();
                        osuScreen.Beatmap.Value = beatmaps.GetWorkingBeatmap(targetBeatmap);
                    }

                    s.Push(new EditorLoader());
                }, [typeof(SongSelect)]);

                return false;
            }

            return base.OnExiting(e);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            overlay.Show();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            beatmapPackageStream?.Dispose();
        }
    }
}
