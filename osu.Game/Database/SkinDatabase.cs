// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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

        internal void Import(string path)
        {
            throw new NotImplementedException();
        }
    }
}
