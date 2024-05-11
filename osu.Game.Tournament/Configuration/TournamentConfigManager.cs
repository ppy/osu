// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Tournament.Configuration
{
    public class TournamentConfigManager : IniConfigManager<StorageConfig>
    {
        protected override string Filename => "tournament.ini";

        private const string default_tournament = "default";

        public TournamentConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(StorageConfig.CurrentTournament, default_tournament);
        }
    }

    public enum StorageConfig
    {
        CurrentTournament,
    }
}
