// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading;
using osu.Framework.Screens;
using osu.Game.Overlays.Settings.Sections.Maintenance;

namespace osu.Game.Tests.Visual.Settings
{
    public class TestSceneMigrationScreens : ScreenTestScene
    {
        public TestSceneMigrationScreens()
        {
            AddStep("Push screen", () => Stack.Push(new TestMigrationSelectScreen()));
        }

        private class TestMigrationSelectScreen : MigrationSelectScreen
        {
            protected override void BeginMigration(DirectoryInfo target) => this.Push(new TestMigrationRunScreen());

            private class TestMigrationRunScreen : MigrationRunScreen
            {
                protected override void PerformMigration()
                {
                    Thread.Sleep(3000);
                }

                public TestMigrationRunScreen()
                    : base(null)
                {
                }
            }
        }
    }
}
