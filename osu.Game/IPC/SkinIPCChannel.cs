// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.IPC
{
    public class SkinIPCChannel : IpcChannel<SkinImportMessage>, IImporter
    {
        private SkinDatabase skins;

        public SkinIPCChannel(IIpcHost host, SkinDatabase database = null)
            :base(host)
        {
            this.skins = database;

            MessageReceived += (msg) =>
            {
                Debug.Assert(skins != null);
                ImportAsync(msg.Path);
            };
        }

        public async Task ImportAsync(string path) 
        {
            if (skins == null)
            {
                await SendMessageAsync(new SkinImportMessage { Path = path });
                return;
            }

            skins.Import(path);
        }
    }

    public class SkinImportMessage {
        public string Path;
    }
}
