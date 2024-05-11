// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Game.Rulesets;
using osu.Game.Tests;

namespace osu.Game.Tournament.Tests.NonVisual
{
    public partial class DataLoadTest : TournamentHostTest
    {
        [Test]
        public void TestRulesetGetsValidOnlineID()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = new TestTournament(runOnLoadComplete: () =>
                    {
                        var storage = host.Storage.GetStorageForDirectory(Path.Combine("tournaments", "default"));

                        using (var stream = storage.CreateFileSafely("bracket.json"))
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(@"{
                        ""Ruleset"": {
                            ""ShortName"": ""taiko"",
                            ""OnlineID"": -1,
                            ""Name"": ""osu!taiko"",
                            ""InstantiationInfo"": ""osu.Game.Rulesets.OsuTaiko.TaikoRuleset, osu.Game.Rulesets.Taiko"",
                            ""Available"": true
                        } }");
                        }
                    });

                    LoadTournament(host, osu);

                    osu.BracketLoadTask.WaitSafely();

                    Assert.That(osu.Dependencies.Get<IBindable<RulesetInfo>>().Value.OnlineID, Is.EqualTo(1));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestUnavailableRuleset()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = new TestTournament(true);

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

        public partial class TestTournament : TournamentGameBase
        {
            private readonly bool resetRuleset;
            private readonly Action? runOnLoadComplete;

            public new Task BracketLoadTask => base.BracketLoadTask;

            public TestTournament(bool resetRuleset = false, [InstantHandle] Action? runOnLoadComplete = null)
            {
                this.resetRuleset = resetRuleset;
                this.runOnLoadComplete = runOnLoadComplete;
            }

            protected override void LoadComplete()
            {
                runOnLoadComplete?.Invoke();
                base.LoadComplete();
                if (resetRuleset)
                    Ruleset.Value = new RulesetInfo(); // not available
            }
        }
    }
}
