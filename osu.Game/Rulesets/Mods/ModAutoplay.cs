// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Replays;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModAutoplay : Mod, ICreateReplayData
    {
        public override string Name => "Autoplay";
        public override string Acronym => "AT";
        public override IconUsage? Icon => OsuIcon.ModAuto;
        public override ModType Type => ModType.Automation;
        public override LocalisableString Description => "Watch a perfect automated play through the song.";
        public override double ScoreMultiplier => 1;

        public override bool UserPlayable => false;
        public override bool ValidForMultiplayer => false;
        public override bool ValidForMultiplayerAsFreeMod => false;

        public override Type[] IncompatibleMods => new[] { typeof(ModCinema), typeof(ModRelax), typeof(ModAdaptiveSpeed), typeof(ModNoStop) };

        public override bool HasImplementation => GetType().GenericTypeArguments.Length == 0;

        public virtual ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods) => new ModReplayData(new Replay(), new ModCreatedUser { Username = @"autoplay" });
    }
}
