//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.Resources
{
    public class NamespacedResourceStore<T> : ResourceStore<T>
    {
        public string Namespace;

        /// <summary>
        /// Initializes a resource store with a single store.
        /// </summary>
        /// <param name="store">The store.</param>
        public NamespacedResourceStore(IResourceStore<T> store, string ns) : base(store)
        {
            Namespace = ns;
        }

        public override T Get(string name)
        {
            return base.Get($@"{Namespace}/{name}");
        }
    }
}
