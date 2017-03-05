// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Ionic.Zip;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Skins;
using SQLite.Net;

namespace osu.Game.Database
{
    public class SkinDatabase
    {
        private SQLiteConnection connection { get; set; }
        private Storage storage;

        public SkinDatabase(Storage storage)
        {
            this.storage = storage;

            if (connection == null) {
                try
                {
                    connection = prepareConnection();
                }
                catch (Exception e)
                {
                    Logger.Error(e, @"Failed to initialise the skin database! Trying again with a clean database...");
                    storage.DeleteDatabase(@"skins");
                    connection = prepareConnection();
                }
            }
        }


        private SQLiteConnection prepareConnection() { 
            SQLiteConnection conn = storage.GetDatabase(@"skins");
            try
            {
                connection.CreateTable <SkinInfo>();
            }
            catch 
            {
                connection.Close();
                throw;
            }
            return conn;
        }

        private SkinInfo getSkin(string path)
        {
            string hash = null;

            if (File.Exists(path)) 
            {
                using (var md5 = MD5.Create())
                using (var input = storage.GetStream(path)) 
                {
                    hash = BitConverter.ToString(md5.ComputeHash(input)).Replace("-","").ToLowerInvariant();
                    input.Seek(0, SeekOrigin.Begin);
                    path = Path.Combine(@"skins", hash.Remove(1), hash.Remove(2), hash);
                    if (!storage.Exists(path))
                    {
                        using (var output = storage.GetStream(path, FileAccess.Write))
                            input.CopyTo(output);
                    }
                }  
            }

            SkinInfo info = new SkinInfo
            {
                // TODO
            };

        }

        public void Import(IEnumerable<string> paths) {
            foreach (string path in paths) 
            {
                getSkin(path);
            }
            throw new NotImplementedException();
        }

        public void Import(string path)
        {
            Import(new [] { path });
        }
    }
}
