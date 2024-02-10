// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Maintenance;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneMigrationScreens : ScreenTestScene
    {
        [Cached(typeof(INotificationOverlay))]
        private readonly NotificationOverlay notifications;

        public TestSceneMigrationScreens()
        {
            Children = new Drawable[]
            {
                notifications = new NotificationOverlay
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                }
            };
        }

        [Test]
        public void TestDeleteSuccess()
        {
            AddStep("Push screen", () => Stack.Push(new TestMigrationSelectScreen(true)));
        }

        [Test]
        public void TestDeleteFails()
        {
            AddStep("Push screen", () => Stack.Push(new TestMigrationSelectScreen(false)));
        }

        private partial class TestMigrationSelectScreen : MigrationSelectScreen
        {
            private readonly bool deleteSuccess;

            public TestMigrationSelectScreen(bool deleteSuccess)
            {
                this.deleteSuccess = deleteSuccess;
            }

            protected override void BeginMigration(DirectoryInfo target) => this.Push(new TestMigrationRunScreen(deleteSuccess));

            private partial class TestMigrationRunScreen : MigrationRunScreen
            {
                private readonly bool success;

                public TestMigrationRunScreen(bool success)
                    : base(null)
                {
                    this.success = success;
                }

                protected override bool PerformMigration()
                {
                    Thread.Sleep(3000);
                    return success;
                }
            }
        }
    }
}
