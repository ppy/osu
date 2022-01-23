// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Realms;
using Realms.Schema;

#nullable enable

namespace osu.Game.Database
{
    public class EmptyRealmSet<T> : IRealmCollection<T>
    {
        private static List<T> emptySet => new List<T>();

        public IEnumerator<T> GetEnumerator()
        {
            return emptySet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)emptySet).GetEnumerator();
        }

        public int Count => emptySet.Count;

        public T this[int index] => emptySet[index];

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public int IndexOf(object item)
        {
            return emptySet.IndexOf((T)item);
        }

        public bool Contains(object item)
        {
            return emptySet.Contains((T)item);
        }

        public IRealmCollection<T> Freeze()
        {
            throw new NotImplementedException();
        }

        public IDisposable SubscribeForNotifications(NotificationCallbackDelegate<T> callback)
        {
            throw new NotImplementedException();
        }

        public bool IsValid => throw new NotImplementedException();

        public Realm Realm => throw new NotImplementedException();

        public ObjectSchema ObjectSchema => throw new NotImplementedException();

        public bool IsFrozen => throw new NotImplementedException();
    }
}
