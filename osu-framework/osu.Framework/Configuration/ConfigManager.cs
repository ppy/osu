//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;

namespace osu.Framework.Configuration
{
    public class ConfigManager<T> : IDisposable
        where T : struct
    {
        public string Filename = @"game.ini";

        bool hasUnsavedChanges;

        Dictionary<T, IBindable> configStore = new Dictionary<T, IBindable>();

        public ConfigManager()
        {
            InitialiseDefaults();
            Load();
        }

        protected virtual void InitialiseDefaults()
        {

        }

        public BindableDouble Set(T lookup, double value)
        {
            BindableDouble bindable = GetBindable<double>(lookup) as BindableDouble;

            if (bindable == null)
            {
                bindable = new BindableDouble(value);
                addBindable(lookup, bindable);
            }
            else
            {
                bindable.Value = value;
            }

            return bindable;
        }

        private void addBindable(T lookup, IBindable bindable)
        {
            configStore[lookup] = bindable;
            bindable.ValueChanged += delegate { hasUnsavedChanges = true; };
        }

        public BindableInt Set(T lookup, int value)
        {
            BindableInt bindable = GetBindable<int>(lookup) as BindableInt;

            if (bindable == null)
            {
                bindable = new BindableInt(value);
                addBindable(lookup, bindable);
            }
            else
            {
                bindable.Value = value;
            }

            return bindable;
        }

        public Bindable<U> Set<U>(T lookup, U value) where U : IComparable
        {
            Bindable<U> bindable = GetBindable<U>(lookup);

            if (bindable == null)
                bindable = set(lookup, value);
            else
                bindable.Value = value;

            return bindable;
        }

        private Bindable<U> set<U>(T lookup, U value) where U : IComparable
        {
            Bindable<U> bindable = new Bindable<U>(value);
            addBindable(lookup, bindable);
            return bindable;
        }

        public U Get<U>(T lookup) where U : IComparable
        {
            return GetBindable<U>(lookup).Value;
        }

        public Bindable<U> GetBindable<U>(T lookup) where U : IComparable
        {
            IBindable obj;

            if (configStore.TryGetValue(lookup, out obj))
            {
                Bindable<U> bindable = obj as Bindable<U>;
                return bindable;
            }

            return set(lookup, default(U));
        }

        public void Load()
        {
            if (!File.Exists(Filename)) return;

            string[] lines = File.ReadAllLines(Filename);

            foreach (string line in lines)
            {
                int equalsIndex = line.IndexOf('=');

                if (line.Length == 0 || line[0] == '#' || equalsIndex < 0) continue;

                string key = line.Substring(0, equalsIndex).Trim();
                string val = line.Remove(0, equalsIndex + 1).Trim();

                T lookup;

                if (!Enum.TryParse(key, out lookup))
                    continue;

                IBindable b;

                if (!configStore.TryGetValue(lookup, out b))
                    continue;

                b.Parse(val);
            }
        }

        public bool Save()
        {
            if (!hasUnsavedChanges) return true;

            try
            {
                using (Stream stream = new SafeWriteStream(Filename))
                using (StreamWriter w = new StreamWriter(stream))
                {
                    foreach (KeyValuePair<T, IBindable> p in configStore)
                        w.WriteLine(@"{0} = {1}", p.Key, p.Value);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Save();

                disposedValue = true;
            }
        }

        ~ConfigManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
