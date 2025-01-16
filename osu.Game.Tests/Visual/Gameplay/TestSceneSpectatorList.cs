// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneSpectatorList : OsuTestScene
    {
        private readonly BindableList<SpectatorList.Spectator> spectators = new BindableList<SpectatorList.Spectator>();
        private readonly Bindable<LocalUserPlayingState> localUserPlayingState = new Bindable<LocalUserPlayingState>();

        private int counter;

        [Test]
        public void TestBasics()
        {
            SpectatorList list = null!;
            AddStep("create spectator list", () => Child = list = new SpectatorList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spectators = { BindTarget = spectators },
                UserPlayingState = { BindTarget = localUserPlayingState }
            });

            AddStep("start playing", () => localUserPlayingState.Value = LocalUserPlayingState.Playing);

            AddRepeatStep("add a user", () =>
            {
                int id = Interlocked.Increment(ref counter);
                spectators.Add(new SpectatorList.Spectator(id, $"User {id}"));
            }, 10);

            AddRepeatStep("remove random user", () => spectators.RemoveAt(RNG.Next(0, spectators.Count)), 5);

            AddStep("change font to venera", () => list.Font.Value = Typeface.Venera);
            AddStep("change font to torus", () => list.Font.Value = Typeface.Torus);
            AddStep("change header colour", () => list.HeaderColour.Value = new Colour4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1));

            AddStep("enter break", () => localUserPlayingState.Value = LocalUserPlayingState.Break);
            AddStep("stop playing", () => localUserPlayingState.Value = LocalUserPlayingState.NotPlaying);
        }
    }
}
