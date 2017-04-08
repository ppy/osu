// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.IO.Stores;
using System;
using System.Collections.Generic;
using System.IO;

namespace osu.Game.IO.Stores
{
    public class KeywordStore : IResourceStore<string>
    {
        private ResourceStore<byte[]> store;
        private string filename;
        private Dictionary<string, string> dictionary;

        private bool loaded;
        public bool Loaded
        {
            get
            {
                return loaded;
            }
            set
            {
                if (loaded == value) return;
                loaded = value;
                if (loaded)
                    load();
                else
                    dictionary = null;
            }
        }

        public KeywordStore(ResourceStore<byte[]> store, string filename)
        {
            this.store = store;
            this.filename = filename;
        }

        public string Get(string name)
        {
            return dictionary[name];
        }

        public Stream GetStream(string name)
        {
            throw new NotImplementedException("We don't support a stream for now"); 
            //var stream = new StreamWriter(Stream.Null);
            //stream.Write(Get(name));
            //return stream.BaseStream;
        }

        private void load()
        {
            dictionary = new Dictionary<string, string>();
            using (StreamReader reader = new StreamReader(store.GetStream(filename)))
            {
                string[] data = reader.ReadLine().Split('=');
                dictionary.Add(data[0], data[1]);
            }
        }
    }
}
