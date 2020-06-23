// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Tournament.Configuration
{
    public class TournamentStorageManager : IniConfigManager<StorageConfig>
    {
        protected override string Filename => "tournament.ini";

        public TournamentStorageManager(Storage storage)
            : base(storage)
        {
        }
    }

    public enum StorageConfig
    {
        CurrentTournament,
    }
}
