// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Rulesets;
using osu.Game.Tests;

namespace osu.Game.Tournament.Tests.NonVisual
{
    public class DataLoadTest : TournamentHostTest
    {
        [Test]
        public void TestUnavailableRuleset()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = new TestTournament();

                    LoadTournament(host, osu);
                    var storage = osu.Dependencies.Get<Storage>();

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(Path.Combine(host.Storage.GetFullPath("."), "tournaments", "default")));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        public class TestTournament : TournamentGameBase
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();
                Ruleset.Value = new RulesetInfo(); // not available
            }
        }
    }
}
