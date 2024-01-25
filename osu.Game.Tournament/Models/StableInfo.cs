// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using Newtonsoft.Json;
using osu.Framework.Platform;
using osu.Game.Tournament.IO;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Holds the path to locate the osu! stable cutting-edge installation.
    /// </summary>
    [Serializable]
    public class StableInfo
    {
        /// <summary>
        /// Path to the IPC directory used by the stable (cutting-edge) install.
        /// </summary>
        public string? StablePath { get; set; }

        /// <summary>
        /// Fired whenever stable info is successfully saved to file.
        /// </summary>
        public event Action? OnStableInfoSaved;

        private const string config_path = "stable.json";

        private readonly Storage configStorage;

        public StableInfo(TournamentStorage storage)
        {
            configStorage = storage.AllTournaments;

            if (!configStorage.Exists(config_path))
                return;

            using (Stream stream = configStorage.GetStream(config_path, FileAccess.Read, FileMode.Open))
            using (var sr = new StreamReader(stream))
            {
                JsonConvert.PopulateObject(sr.ReadToEnd(), this);
            }
        }

        public void SaveChanges()
        {
            using (var stream = configStorage.CreateFileSafely(config_path))
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(JsonConvert.SerializeObject(this,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                    }));
            }

            OnStableInfoSaved?.Invoke();
        }
    }
}
