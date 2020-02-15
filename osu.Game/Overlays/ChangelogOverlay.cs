// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Changelog;

namespace osu.Game.Overlays
{
    public class ChangelogOverlay : FullscreenOverlay
    {
        public readonly Bindable<APIChangelogBuild> Current = new Bindable<APIChangelogBuild>();

        protected ChangelogHeader Header;

        private Container<ChangelogContent> content;

        private SampleChannel sampleBack;

        private List<APIChangelogBuild> builds;

        protected List<APIUpdateStream> Streams;

        public ChangelogOverlay()
            : base(OverlayColourScheme.Purple)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colour)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour.PurpleDarkAlternative,
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new ReverseChildIDFillFlowContainer<Drawable>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            Header = new ChangelogHeader
                            {
                                ListingSelected = ShowListing,
                            },
                            content = new Container<ChangelogContent>
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                            }
                        },
                    },
                },
            };

            sampleBack = audio.Samples.Get(@"UI/generic-select-soft");

            Header.Build.BindTo(Current);

            Current.BindValueChanged(e =>
            {
                if (e.NewValue != null)
                    loadContent(new ChangelogSingleBuild(e.NewValue));
                else
                    loadContent(new ChangelogListing(builds));
            });
        }

        public void ShowListing()
        {
            Current.Value = null;
            Show();
        }

        /// <summary>
        /// Fetches and shows a specific build from a specific update stream.
        /// </summary>
        /// <param name="build">Must contain at least <see cref="APIUpdateStream.Name"/> and
        /// <see cref="APIChangelogBuild.Version"/>. If <see cref="APIUpdateStream.DisplayName"/> and
        /// <see cref="APIChangelogBuild.DisplayVersion"/> are specified, the header will instantly display them.</param>
        public void ShowBuild([NotNull] APIChangelogBuild build)
        {
            if (build == null) throw new ArgumentNullException(nameof(build));

            Current.Value = build;
            Show();
        }

        public void ShowBuild([NotNull] string updateStream, [NotNull] string version)
        {
            if (updateStream == null) throw new ArgumentNullException(nameof(updateStream));
            if (version == null) throw new ArgumentNullException(nameof(version));

            performAfterFetch(() =>
            {
                var build = builds.Find(b => b.Version == version && b.UpdateStream.Name == updateStream)
                            ?? Streams.Find(s => s.Name == updateStream)?.LatestBuild;

                if (build != null)
                    ShowBuild(build);
            });

            Show();
        }

        public override bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Back:
                    if (Current.Value == null)
                    {
                        Hide();
                    }
                    else
                    {
                        Current.Value = null;
                        sampleBack?.Play();
                    }

                    return true;
            }

            return false;
        }

        protected override void PopIn()
        {
            base.PopIn();

            if (initialFetchTask == null)
                // fetch and refresh to show listing, if no other request was made via Show methods
                performAfterFetch(() => Current.TriggerChange());
        }

        private Task initialFetchTask;

        private void performAfterFetch(Action action) => fetchListing()?.ContinueWith(_ =>
            Schedule(action), TaskContinuationOptions.OnlyOnRanToCompletion);

        private Task fetchListing()
        {
            if (initialFetchTask != null)
                return initialFetchTask;

            return initialFetchTask = Task.Run(async () =>
            {
                var tcs = new TaskCompletionSource<bool>();

                var req = new GetChangelogRequest();

                req.Success += res => Schedule(() =>
                {
                    // remap streams to builds to ensure model equality
                    res.Builds.ForEach(b => b.UpdateStream = res.Streams.Find(s => s.Id == b.UpdateStream.Id));
                    res.Streams.ForEach(s => s.LatestBuild.UpdateStream = res.Streams.Find(s2 => s2.Id == s.LatestBuild.UpdateStream.Id));

                    builds = res.Builds;
                    Streams = res.Streams;

                    Header.Populate(res.Streams);

                    tcs.SetResult(true);
                });

                req.Failure += e =>
                {
                    initialFetchTask = null;
                    tcs.SetException(e);
                };

                await API.PerformAsync(req);

                await tcs.Task;
            });
        }

        private CancellationTokenSource loadContentCancellation;

        private void loadContent(ChangelogContent newContent)
        {
            content.FadeTo(0.2f, 300, Easing.OutQuint);

            loadContentCancellation?.Cancel();

            LoadComponentAsync(newContent, c =>
            {
                content.FadeIn(300, Easing.OutQuint);

                c.BuildSelected = ShowBuild;
                content.Child = c;
            }, (loadContentCancellation = new CancellationTokenSource()).Token);
        }
    }
}
