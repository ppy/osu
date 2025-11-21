// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using JetBrains.Annotations;
using Realms;
using Realms.Schema;

namespace osu.Game.Database
{
    /// <summary>
    /// This can arrive in <see cref="RealmAccess.RegisterForNotifications{T}"/> callbacks to imply that realm access has been reset.
    /// </summary>
    /// <remarks>
    /// Usually implies that the original database may return soon and the callback can usually be silently ignored.
    ///</remarks>
    public class RealmResetEmptySet<T> : IRealmCollection<T>
    {
        private IList<T> emptySet => Array.Empty<T>();

        [MustDisposeResource]
        public IEnumerator<T> GetEnumerator() => emptySet.GetEnumerator();

        [MustDisposeResource]
        IEnumerator IEnumerable.GetEnumerator() => emptySet.GetEnumerator();

        public int Count => emptySet.Count;
        public T this[int index] => emptySet[index];
        public int IndexOf(object? item) => item == null ? -1 : emptySet.IndexOf((T)item);
        public bool Contains(object? item) => item != null && emptySet.Contains((T)item);

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

        public IRealmCollection<T> Freeze() => throw new NotImplementedException();
        public IDisposable SubscribeForNotifications(NotificationCallbackDelegate<T> callback, KeyPathsCollection? keyPathCollection = null) => throw new NotImplementedException();

        public bool IsValid => throw new NotImplementedException();
        public Realm Realm => throw new NotImplementedException();
        public ObjectSchema ObjectSchema => throw new NotImplementedException();
        public bool IsFrozen => throw new NotImplementedException();
    }
}
