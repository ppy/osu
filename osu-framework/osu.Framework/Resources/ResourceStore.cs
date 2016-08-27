//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace osu.Framework.Resources
{
    public class ResourceStore<T> : IResourceStore<T>
    {
        private Dictionary<string, Action> actionList = new Dictionary<string, Action>();

        private List<IResourceStore<T>> stores = new List<IResourceStore<T>>();

        /// <summary>
        /// Initializes a resource store with no stores.
        /// </summary>
        public ResourceStore() { }

        /// <summary>
        /// Initializes a resource store with a single store.
        /// </summary>
        /// <param name="store">The store.</param>
        public ResourceStore(IResourceStore<T> store)
        {
            AddStore(store);
        }

        /// <summary>
        /// Initializes a resource store with a collection of stores.
        /// </summary>
        /// <param name="stores">The collection of stores.</param>
        public ResourceStore(IResourceStore<T>[] stores)
        {
            foreach (ResourceStore<T> store in stores)
                AddStore(store);
        }

        /// <summary>
        /// Notifies a bound delegate that the resource has changed.
        /// </summary>
        /// <param name="name">The resource that has changed.</param>
        protected virtual void NotifyChanged(string name)
        {
            Action action;
            if (!actionList.TryGetValue(name, out action))
                return;

            action?.Invoke();
        }

        /// <summary>
        /// Adds a resource store to this store.
        /// </summary>
        /// <param name="store">The store to add.</param>
        public virtual void AddStore(IResourceStore<T> store)
        {
            stores.Add(store);
        }

        /// <summary>
        /// Removes a store from this store.
        /// </summary>
        /// <param name="store">The store to remove.</param>
        public virtual void RemoveStore(IResourceStore<T> store)
        {
            stores.Remove(store);
        }

        /// <summary>
        /// Retrieves an object from the store.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="reloadFunction">The function to call when the store reloads the object data.</param>
        /// <returns>The object.</returns>
        public virtual T Get(string name)
        {
            object result = null;

            // Cache miss - get the resource
            foreach (IResourceStore<T> store in stores)
            {
                try
                {
                    result = store.Get(name);
                    if (result != null)
                        break;
                }
                catch { }
            }

            return (T)result;
        }

        public Stream GetStream(string name)
        {
            Stream result = null;

            // Cache miss - get the resource
            foreach (IResourceStore<T> store in stores)
            {
                try
                {
                    result = store.GetStream(name);
                    if (result != null)
                        break;
                }
                catch { }
            }

            return result;
        }

        /// <summary>
        /// Binds a reload function to an object held by the store.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="onReload">The reload function to bind.</param>
        public void BindReload(string name, Action onReload)
        {
            if (onReload == null)
                return;

            // Check if there's already a reload action bound
            if (actionList.ContainsKey(name))
                throw new ReloadAlreadyBoundException(name);

            actionList[name] = onReload;
        }

        public class ReloadAlreadyBoundException : Exception
        {
            public ReloadAlreadyBoundException(string resourceName)
                : base($"A reload delegate is already bound to the resource '{resourceName}'.")
            {
            }
        }
    }
}
