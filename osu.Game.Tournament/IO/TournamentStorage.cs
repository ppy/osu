// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO;
using osu.Game.Tournament.Configuration;

namespace osu.Game.Tournament.IO
{
    public class TournamentStorage : WrappedStorage
    {
        /// <summary>
        /// The storage where all tournaments are located.
        /// </summary>
        public readonly Storage AllTournaments;

        public readonly Bindable<string> CurrentTournament;

        protected TournamentConfigManager TournamentConfigManager { get; }

        public TournamentStorage(Storage storage)
            : base(storage.GetStorageForDirectory("tournaments"), string.Empty)
        {
            AllTournaments = UnderlyingStorage;

            TournamentConfigManager = new TournamentConfigManager(storage);

            CurrentTournament = TournamentConfigManager.GetBindable<string>(StorageConfig.CurrentTournament);

            ChangeTargetStorage(AllTournaments.GetStorageForDirectory(CurrentTournament.Value));

            Logger.Log("Using tournament storage: " + GetFullPath(string.Empty));

            CurrentTournament.BindValueChanged(updateTournament);
        }

        private void updateTournament(ValueChangedEvent<string> newTournament)
        {
            ChangeTargetStorage(AllTournaments.GetStorageForDirectory(newTournament.NewValue));
            Logger.Log("Changing tournament storage: " + GetFullPath(string.Empty));
        }

        public IEnumerable<string> ListTournaments() => AllTournaments.GetDirectories(string.Empty).OrderBy(directory => directory, StringComparer.CurrentCultureIgnoreCase);
    }
}
