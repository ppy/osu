// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Online;
using osuTK;
using Realms;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(FirstRunSetupBeatmapScreenStrings), nameof(FirstRunSetupBeatmapScreenStrings.Header))]
    public partial class ScreenBeatmaps : FirstRunSetupScreen
    {
        private ProgressRoundedButton downloadBundledButton = null!;
        private ProgressRoundedButton downloadTutorialButton = null!;

        private OsuTextFlowContainer currentlyLoadedBeatmaps = null!;

        private BundledBeatmapDownloader? tutorialDownloader;
        private BundledBeatmapDownloader? bundledDownloader;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        private IDisposable? beatmapSubscription;

        [BackgroundDependencyLoader]
        private void load()
        {
            Vector2 buttonSize = new Vector2(400, 50);

            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    Colour = OverlayColourProvider.Content1,
                    Text = FirstRunSetupBeatmapScreenStrings.Description,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 30,
                    Children = new Drawable[]
                    {
                        currentlyLoadedBeatmaps = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: HEADER_FONT_SIZE, weight: FontWeight.SemiBold))
                        {
                            Colour = OverlayColourProvider.Content2,
                            TextAnchor = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                        },
                    }
                },
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    Colour = OverlayColourProvider.Content1,
                    Text = FirstRunSetupBeatmapScreenStrings.TutorialDescription,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                downloadTutorialButton = new ProgressRoundedButton
                {
                    Size = buttonSize,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    BackgroundColour = colours.Pink3,
                    Text = FirstRunSetupBeatmapScreenStrings.TutorialButton,
                    Action = downloadTutorial
                },
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    Colour = OverlayColourProvider.Content1,
                    Text = FirstRunSetupBeatmapScreenStrings.BundledDescription,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                downloadBundledButton = new ProgressRoundedButton
                {
                    Size = buttonSize,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    BackgroundColour = colours.Blue3,
                    Text = FirstRunSetupBeatmapScreenStrings.BundledButton,
                    Action = downloadBundled
                },
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    Colour = OverlayColourProvider.Content1,
                    Text = FirstRunSetupBeatmapScreenStrings.ObtainMoreBeatmaps,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapSubscription = realmAccess.RegisterForNotifications(r => r.All<BeatmapSetInfo>().Where(s => !s.DeletePending && !s.Protected), beatmapsChanged);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmapSubscription?.Dispose();
        }

        private void beatmapsChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes) => Schedule(() =>
        {
            currentlyLoadedBeatmaps.Text = FirstRunSetupBeatmapScreenStrings.CurrentlyLoadedBeatmaps(sender.Count);

            if (sender.Count == 0)
            {
                currentlyLoadedBeatmaps.FadeColour(colours.Red1, 500, Easing.OutQuint);
            }
            else if (changes != null && (changes.DeletedIndices.Any() || changes.InsertedIndices.Any()))
            {
                currentlyLoadedBeatmaps.FadeColour(colours.Yellow)
                                       .FadeColour(OverlayColourProvider.Content2, 1500, Easing.OutQuint);

                currentlyLoadedBeatmaps.ScaleTo(1.1f)
                                       .ScaleTo(1, 1500, Easing.OutQuint);
            }
        });

        private void downloadTutorial()
        {
            if (tutorialDownloader != null)
                return;

            tutorialDownloader = new BundledBeatmapDownloader(true);

            AddInternal(tutorialDownloader);

            var downloadTracker = tutorialDownloader.DownloadTrackers.First();

            downloadTracker.State.BindValueChanged(state =>
            {
                if (state.NewValue == DownloadState.LocallyAvailable)
                    downloadTutorialButton.Complete();
            }, true);

            downloadTracker.Progress.BindValueChanged(progress =>
            {
                downloadTutorialButton.SetProgress(progress.NewValue, false);
            }, true);
        }

        private void downloadBundled()
        {
            if (bundledDownloader != null)
                return;

            bundledDownloader = new BundledBeatmapDownloader(false);

            AddInternal(bundledDownloader);

            foreach (var tracker in bundledDownloader.DownloadTrackers)
                tracker.State.BindValueChanged(_ => updateProgress(), true);

            void updateProgress()
            {
                double progress = (double)bundledDownloader.DownloadTrackers.Count(t => t.State.Value == DownloadState.LocallyAvailable) / bundledDownloader.DownloadTrackers.Count();

                if (progress == 1)
                    downloadBundledButton.Complete();
                else
                    downloadBundledButton.SetProgress(progress, true);
            }
        }
    }
}
