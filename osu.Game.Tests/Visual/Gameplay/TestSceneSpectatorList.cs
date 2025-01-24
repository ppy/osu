// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneSpectatorList : OsuTestScene
    {
        private int counter;

        [Test]
        public void TestBasics()
        {
            SpectatorList list = null!;
            Bindable<LocalUserPlayingState> playingState = new Bindable<LocalUserPlayingState>();
            GameplayState gameplayState = new GameplayState(new Beatmap(), new OsuRuleset(), healthProcessor: new OsuHealthProcessor(0), localUserPlayingState: playingState);
            TestSpectatorClient client = new TestSpectatorClient();

            AddStep("create spectator list", () =>
            {
                Children = new Drawable[]
                {
                    client,
                    new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies =
                        [
                            (typeof(GameplayState), gameplayState),
                            (typeof(SpectatorClient), client)
                        ],
                        Child = list = new SpectatorList
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                };
            });

            AddStep("start playing", () => playingState.Value = LocalUserPlayingState.Playing);

            AddRepeatStep("add a user", () =>
            {
                int id = Interlocked.Increment(ref counter);
                ((ISpectatorClient)client).UserStartedWatching([
                    new SpectatorUser
                    {
                        OnlineID = id,
                        Username = $"User {id}"
                    }
                ]);
            }, 10);

            AddRepeatStep("remove random user", () => ((ISpectatorClient)client).UserEndedWatching(client.WatchingUsers[RNG.Next(client.WatchingUsers.Count)].OnlineID), 5);

            AddStep("change font to venera", () => list.Font.Value = Typeface.Venera);
            AddStep("change font to torus", () => list.Font.Value = Typeface.Torus);
            AddStep("change header colour", () => list.HeaderColour.Value = new Colour4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1));

            AddStep("enter break", () => playingState.Value = LocalUserPlayingState.Break);
            AddStep("stop playing", () => playingState.Value = LocalUserPlayingState.NotPlaying);
        }
    }
}
