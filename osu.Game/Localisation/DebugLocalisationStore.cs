// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class DebugLocalisationStore : ILocalisationStore
    {
        public string Get(string lookup) => $@"[[{lookup.Substring(lookup.LastIndexOf('.') + 1)}]]";

        public Task<string> GetAsync(string lookup, CancellationToken cancellationToken = default) => Task.FromResult(Get(lookup));

        public Stream GetStream(string name) => throw new NotImplementedException();

        public IEnumerable<string> GetAvailableResources() => throw new NotImplementedException();

        public CultureInfo EffectiveCulture { get; } = CultureInfo.CurrentCulture;

        public void Dispose()
        {
        }
    }
}
