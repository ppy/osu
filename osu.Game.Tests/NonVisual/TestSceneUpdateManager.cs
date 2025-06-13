// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Tests.Visual;
using osu.Game.Updater;

namespace osu.Game.Tests.NonVisual
{
    [HeadlessTest]
    public partial class TestSceneUpdateManager : OsuTestScene
    {
        [Cached(typeof(INotificationOverlay))]
        private readonly INotificationOverlay notifications = new TestNotificationOverlay();

        private TestUpdateManager manager = null!;
        private OsuConfigManager config = null!;

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("add manager", () =>
            {
                config = new OsuConfigManager(LocalStorage);
                config.SetValue(OsuSetting.ReleaseStream, ReleaseStream.Lazer);

                Child = new DependencyProvidingContainer
                {
                    CachedDependencies = [(typeof(OsuConfigManager), config)],
                    Child = manager = new TestUpdateManager()
                };
            });

            // Updates should be checked when the object is loaded for the first time.
            AddUntilStep("check pending", () => manager.IsPending);
            AddStep("complete check", () => manager.Complete());
            AddUntilStep("1 check completed", () => manager.Completions, () => Is.EqualTo(1));
            AddUntilStep("no check pending", () => !manager.IsPending);
        }

        /// <summary>
        /// Updates should be checked when the release stream is changed.
        /// </summary>
        [Test]
        public void TestReleaseStreamChanged()
        {
            AddStep("change release stream", () => config.SetValue(OsuSetting.ReleaseStream, ReleaseStream.Tachyon));

            AddUntilStep("check pending", () => manager.IsPending);
            AddStep("complete check", () => manager.Complete());
            AddUntilStep("2 checks completed", () => manager.Completions, () => Is.EqualTo(2));
            AddUntilStep("no check pending", () => !manager.IsPending);

            AddStep("change release stream", () => config.SetValue(OsuSetting.ReleaseStream, ReleaseStream.Lazer));

            AddUntilStep("check pending", () => manager.IsPending);
            AddStep("complete check", () => manager.Complete());
            AddUntilStep("3 checks completed", () => manager.Completions, () => Is.EqualTo(3));
            AddUntilStep("no check pending", () => !manager.IsPending);
        }

        /// <summary>
        /// Changing the release stream should start a new invocation and cancel the existing one.
        /// </summary>
        [Test]
        public void TestNewInvocationOnReleaseStreamChanged()
        {
            AddStep("change release stream", () => config.SetValue(OsuSetting.ReleaseStream, ReleaseStream.Tachyon));
            AddUntilStep("check pending", () => manager.IsPending);
            AddStep("change release stream", () => config.SetValue(OsuSetting.ReleaseStream, ReleaseStream.Lazer));
            AddUntilStep("3 invocations", () => manager.Invocations, () => Is.EqualTo(3));

            AddStep("complete check", () => manager.Complete());
            AddUntilStep("2 checks completed", () => manager.Completions, () => Is.EqualTo(2));
            AddUntilStep("no check pending", () => !manager.IsPending);
        }

        /// <summary>
        /// Updates should be checked when the user requests them to.
        /// </summary>
        [Test]
        public void TestUserRequest()
        {
            AddStep("request check", () => manager.CheckForUpdate());

            AddUntilStep("check pending", () => manager.IsPending);
            AddStep("complete check", () => manager.Complete());
            AddUntilStep("2 checks completed", () => manager.Completions, () => Is.EqualTo(2));
            AddUntilStep("no check pending", () => !manager.IsPending);

            AddStep("request check", () => manager.CheckForUpdate());

            AddUntilStep("check pending", () => manager.IsPending);
            AddStep("complete check", () => manager.Complete());
            AddUntilStep("3 checks completed", () => manager.Completions, () => Is.EqualTo(3));
            AddUntilStep("no check pending", () => !manager.IsPending);
        }

        /// <summary>
        /// User requests should start a new invocation and cancel the existing one.
        /// </summary>
        [Test]
        public void TestUserRequestOverridesExistingCheck()
        {
            // This part covering double user input is not really possible because the settings button is disabled during the check,
            // but it's kept here for sanity in-case the update manager is used as a standalone object elsewhere.

            AddStep("request check", () => manager.CheckForUpdate());
            AddUntilStep("check pending", () => manager.IsPending);
            AddStep("request check", () => manager.CheckForUpdate());
            AddUntilStep("3 invocations", () => manager.Invocations, () => Is.EqualTo(3));

            AddStep("complete check", () => manager.Complete());
            AddUntilStep("2 checks completed", () => manager.Completions, () => Is.EqualTo(2));
            AddUntilStep("no check pending", () => !manager.IsPending);

            // This next part tests for the user requesting an update during a background check, and is possible to occur in practice.

            AddStep("change release stream", () => config.SetValue(OsuSetting.ReleaseStream, ReleaseStream.Tachyon));
            AddUntilStep("check pending", () => manager.IsPending);
            AddStep("request check", () => manager.CheckForUpdate());
            AddUntilStep("5 invocations", () => manager.Invocations, () => Is.EqualTo(5));

            AddStep("complete check", () => manager.Complete());
            AddUntilStep("3 checks completed", () => manager.Completions, () => Is.EqualTo(3));
            AddUntilStep("no check pending", () => !manager.IsPending);
        }

        private partial class TestUpdateManager : UpdateManager
        {
            public bool IsPending { get; private set; }
            public int Invocations { get; private set; }
            public int Completions { get; private set; }

            private TaskCompletionSource<bool>? pendingCheck;

            protected override async Task<bool> PerformUpdateCheck(CancellationToken cancellationToken)
            {
                Invocations++;

                var check = pendingCheck = new TaskCompletionSource<bool>();
                IsPending = true;

                try
                {
                    bool result = await check.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                    Completions++;
                    return result;
                }
                finally
                {
                    IsPending = false;
                }
            }

            public void Complete()
            {
                pendingCheck?.SetResult(true);
            }
        }

        private partial class TestNotificationOverlay : INotificationOverlay
        {
            public void Post(Notification notification)
            {
            }

            public void Hide()
            {
            }

            public IBindable<int> UnreadCount { get; } = new Bindable<int>();

            public IEnumerable<Notification> AllNotifications { get; } = Enumerable.Empty<Notification>();
        }
    }
}
