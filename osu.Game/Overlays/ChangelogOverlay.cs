// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Changelog;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class ChangelogOverlay : OnlineOverlay<ChangelogHeader>
    {
        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        public readonly Bindable<APIChangelogBuild> Current = new Bindable<APIChangelogBuild>();

        private List<APIChangelogBuild> builds;

        protected List<APIUpdateStream> Streams;

        public ChangelogOverlay()
            : base(OverlayColourScheme.Purple, false)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Header.Build.BindTarget = Current;

            Current.BindValueChanged(e =>
            {
                if (e.NewValue != null)
                    loadContent(new ChangelogSingleBuild(e.NewValue));
                else
                    loadContent(new ChangelogListing(builds));
            });
        }

        protected override ChangelogHeader CreateHeader() => new ChangelogHeader
        {
            ListingSelected = ShowListing,
        };

        protected override Color4 BackgroundColour => ColourProvider.Background4;

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

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Back:
                    if (Current.Value == null)
                    {
                        Hide();
                    }
                    else
                    {
                        Current.Value = null;
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

        private void performAfterFetch(Action action) => Schedule(() =>
        {
            fetchListing()?.ContinueWith(_ =>
                Schedule(action), TaskContinuationOptions.OnlyOnRanToCompletion);
        });

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

                await API.PerformAsync(req).ConfigureAwait(false);

                return tcs.Task;
            }).Unwrap();
        }

        private CancellationTokenSource loadContentCancellation;

        private void loadContent(ChangelogContent newContent)
        {
            Content.FadeTo(0.2f, 300, Easing.OutQuint);

            loadContentCancellation?.Cancel();

            LoadComponentAsync(newContent, c =>
            {
                Content.FadeIn(300, Easing.OutQuint);

                c.BuildSelected = ShowBuild;
                Child = c;
            }, (loadContentCancellation = new CancellationTokenSource()).Token);
        }
    }
}
