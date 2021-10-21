// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

#nullable enable

namespace osu.Game.Database
{
    public interface IPostImports<out TModel>
        where TModel : class
    {
        /// <summary>
        /// Fired when the user requests to view the resulting import.
        /// </summary>
        public Action<IEnumerable<ILive<TModel>>>? PostImport { set; }
    }
}
