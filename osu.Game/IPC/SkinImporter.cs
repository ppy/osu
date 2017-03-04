// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.IPC
{
    public class SkinImporter
    {
        private IpcChannel<SkinImportMessage> channel;
        private SkinDatabase skins;

        public SkinImporter(GameHost host, SkinDatabase database = null)
        {
            this.skins = database;

            channel = new IpcChannel<SkinImportMessage>(host);
            channel.MessageReceived += messageReceived;
        }

        public async Task ImportAsync(string path) 
        {            if (skins != null)
            {
                skins.Import(path);
            }
            else 
            {
                await channel.SendMessageAsync(new SkinImportMessage{Path = path});
            }
        }

        private void messageReceived(SkinImportMessage msg) 
        {
            Debug.Assert(skins != null);

            ImportAsync(msg.Path);
        }
    }

    public class SkinImportMessage {
        public string Path;
    }
}
