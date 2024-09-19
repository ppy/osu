// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Audio.Effects;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Menus
{
    public partial class TestSceneAudioDucking : OsuGameTestScene
    {
        [Test]
        public void TestMomentaryDuck()
        {
            AddStep("duck momentarily", () => Game.MusicController.DuckMomentarily(1000));
        }

        [Test]
        public void TestMultipleDucks()
        {
            IDisposable duckOp1 = null!;
            IDisposable duckOp2 = null!;

            double normalVolume = 1;

            AddStep("get initial volume", () =>
            {
                normalVolume = Game.Audio.Tracks.AggregateVolume.Value;
            });

            AddStep("duck one", () =>
            {
                duckOp1 = Game.MusicController.Duck(new DuckParameters
                {
                    DuckVolumeTo = 0.5,
                });
            });

            AddUntilStep("wait for duck to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume * 0.5f).Within(0.01));

            AddStep("duck two", () =>
            {
                duckOp2 = Game.MusicController.Duck(new DuckParameters
                {
                    DuckVolumeTo = 0.2,
                });
            });

            AddUntilStep("wait for duck to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume * 0.2f).Within(0.01));

            AddStep("restore two", () => duckOp2.Dispose());
            AddUntilStep("wait for restore to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume * 0.5f).Within(0.01));

            AddStep("restore one", () => duckOp1.Dispose());
            AddUntilStep("wait for restore to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume).Within(0.01));
        }

        [Test]
        public void TestMultipleDucksSameParameters()
        {
            var duckParameters = new DuckParameters
            {
                DuckVolumeTo = 0.5,
            };

            IDisposable duckOp1 = null!;
            IDisposable duckOp2 = null!;

            double normalVolume = 1;

            AddStep("get initial volume", () =>
            {
                normalVolume = Game.Audio.Tracks.AggregateVolume.Value;
            });

            AddStep("duck one", () =>
            {
                duckOp1 = Game.MusicController.Duck(duckParameters);
            });

            AddUntilStep("wait for duck to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume * 0.5f).Within(0.01));

            AddStep("duck two", () =>
            {
                duckOp2 = Game.MusicController.Duck(duckParameters);
            });

            AddUntilStep("wait for duck to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume * 0.5f).Within(0.01));

            AddStep("restore two", () => duckOp2.Dispose());
            AddUntilStep("wait for restore to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume * 0.5f).Within(0.01));

            AddStep("restore one", () => duckOp1.Dispose());
            AddUntilStep("wait for restore to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume).Within(0.01));
        }

        [Test]
        public void TestMultipleDucksReverseOrder()
        {
            IDisposable duckOp1 = null!;
            IDisposable duckOp2 = null!;

            double normalVolume = 1;

            AddStep("get initial volume", () =>
            {
                normalVolume = Game.Audio.Tracks.AggregateVolume.Value;
            });

            AddStep("duck one", () =>
            {
                duckOp1 = Game.MusicController.Duck(new DuckParameters
                {
                    DuckVolumeTo = 0.5,
                });
            });

            AddUntilStep("wait for duck to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume * 0.5f).Within(0.01));

            AddStep("duck two", () =>
            {
                duckOp2 = Game.MusicController.Duck(new DuckParameters
                {
                    DuckVolumeTo = 0.2,
                });
            });

            AddUntilStep("wait for duck to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume * 0.2f).Within(0.01));

            AddStep("restore one", () => duckOp1.Dispose());

            // reverse order, less extreme duck removed so won't change
            AddUntilStep("wait for restore to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume * 0.2f).Within(0.01));

            AddStep("restore two", () => duckOp2.Dispose());
            AddUntilStep("wait for restore to complete", () => Game.Audio.Tracks.AggregateVolume.Value, () => Is.EqualTo(normalVolume).Within(0.01));
        }

        [Test]
        public void TestMultipleDisposalIsNoop()
        {
            IDisposable duckOp1 = null!;

            AddStep("duck", () => duckOp1 = Game.MusicController.Duck());
            AddStep("restore", () => duckOp1.Dispose());
            AddStep("restore", () => duckOp1.Dispose());
        }

        [Test]
        public void TestMultipleDucksDifferentPieces()
        {
            IDisposable duckOp1 = null!;
            IDisposable duckOp2 = null!;

            AddStep("duck volume", () =>
            {
                duckOp1 = Game.MusicController.Duck(new DuckParameters
                {
                    DuckVolumeTo = 0.2,
                    DuckCutoffTo = AudioFilter.MAX_LOWPASS_CUTOFF,
                    DuckDuration = 500,
                });
            });

            AddStep("duck lowpass", () =>
            {
                duckOp2 = Game.MusicController.Duck(new DuckParameters
                {
                    DuckVolumeTo = 1,
                    DuckCutoffTo = 300,
                    DuckDuration = 500,
                });
            });

            AddStep("restore lowpass", () => duckOp2.Dispose());
            AddStep("restore volume", () => duckOp1.Dispose());
        }
    }
}
