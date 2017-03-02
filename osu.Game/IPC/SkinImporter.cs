// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Platform;

namespace osu.Game.IPC
{
    public class SkinImporter
    {
        private IpcChannel<SkinImportMessage> channel;
        // SkinDatabase database; ?

        public SkinImporter()
        {
        }
    }

    public class SkinImportMessage {
        string Path;
    }
}
