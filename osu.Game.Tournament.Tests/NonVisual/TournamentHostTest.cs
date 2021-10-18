// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Platform;

namespace osu.Game.Tournament.Tests.NonVisual
{
    public abstract class TournamentHostTest
    {
        public static TournamentGameBase LoadTournament(GameHost host, TournamentGameBase tournament = null)
        {
            tournament ??= new TournamentGameBase();
            Task.Factory.StartNew(() => host.Run(tournament), TaskCreationOptions.LongRunning)
                .ContinueWith(t => Assert.Fail($"Host threw exception {t.Exception}"), TaskContinuationOptions.OnlyOnFaulted);
            WaitForOrAssert(() => tournament.IsLoaded, @"osu! failed to start in a reasonable amount of time");
            return tournament;
        }

        public static void WaitForOrAssert(Func<bool> result, string failureMessage, int timeout = 90000)
        {
            Task task = Task.Run(() =>
            {
                while (!result()) Thread.Sleep(200);
            });

            Assert.IsTrue(task.Wait(timeout), failureMessage);
        }
    }
}
