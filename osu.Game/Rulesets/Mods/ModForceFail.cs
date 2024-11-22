// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModForceFail : Mod, IForceFail
    {
        public override Type[] IncompatibleMods => new[] { typeof(ModNoFail), typeof(ModCinema) };

        [SettingSource("Restart on fail", "Automatically restarts when failed.")]
        public BindableBool Restart { get; } = new BindableBool();

        public virtual bool RestartOnFail => Restart.Value;

        public abstract bool ShouldFail(JudgementResult result);
    }
}
