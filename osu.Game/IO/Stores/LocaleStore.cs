// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.IO.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Game.IO.Stores
{
    public class LocaleStore : IResourceStore<string>
    {
        private Dictionary<string, KeywordStore> stores = new Dictionary<string, KeywordStore>();

        private string locale = "";
        public string Locale
        {
            get
            {
                return locale;
            }
            set
            {
                if(stores.ContainsKey(locale))
                    stores[locale].Loaded = false;
                locale = value;
                stores[locale].Loaded = true;
            }
        }

        public IEnumerable<string> Locales => stores.Keys;

        public string Get(string name)
        {
            KeywordStore store;
            if (!stores.TryGetValue(Locale, out store))
                store = stores.Values.ToList()[0];
            return store.Get(name);
        }

        public Stream GetStream(string name)
        {
            throw new NotImplementedException();
        }

        public void AddStore(string name, KeywordStore store)
        {
            stores.Add(name, store);

            if (stores.Count == 1)
                Locale = name;
        }

        public void RemoveStore(string name) => stores.Remove(name);
    }
}
