// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using Tmds.DBus;

namespace Mvis.Plugin.CloudMusicSupport.DBus
{
    [DBusInterface("io.matrix_feather.mvis.lyric")]
    public interface ILyricDBusObject : IDBusObject
    {
        Task<string> GetCurrentLineRawAsync();
        Task<string> GetCurrentLineTranslatedAsync();
    }

    public class LyricDBusObject : ILyricDBusObject
    {
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mvis/lyric");
        public ObjectPath ObjectPath => PATH;

        public LyricPlugin Plugin { get; set; }

        public Task<string> GetCurrentLineRawAsync()
            => Task.FromResult(Plugin.Disabled.Value ? "-" : Plugin.CurrentLine.Content);

        public Task<string> GetCurrentLineTranslatedAsync()
            => Task.FromResult(Plugin.Disabled.Value ? "-" : Plugin.CurrentLine.TranslatedString);
    }
}
