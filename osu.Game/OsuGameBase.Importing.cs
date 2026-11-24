// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Database;

namespace osu.Game
{
    public partial class OsuGameBase
    {
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly List<ICanAcceptFiles> fileImporters = new List<ICanAcceptFiles>();

#pragma warning disable RS0030

        private void runOnImporters(Action<List<ICanAcceptFiles>> mutateFunc)
        {
            semaphoreSlim.Wait();

            try
            {
                mutateFunc.Invoke(fileImporters);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private T runOnImporters<T>(Func<List<ICanAcceptFiles>, T> mutateFunc)
        {
            semaphoreSlim.Wait();

            try
            {
                return mutateFunc.Invoke(fileImporters);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task<T> runOnImportersAsync<T>(Func<List<ICanAcceptFiles>, T> mutateFunc)
        {
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);

            try
            {
                return mutateFunc.Invoke(fileImporters);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

#pragma warning restore RS0030

        /// <summary>
        /// Register a global handler for file imports. Most recently registered will have precedence.
        /// </summary>
        /// <param name="handler">The handler to register.</param>
        public void RegisterImportHandler(ICanAcceptFiles handler) => runOnImporters(importers => importers.Insert(0, handler));

        /// <summary>
        /// Unregister a global handler for file imports.
        /// </summary>
        /// <param name="handler">The previously registered handler.</param>
        public void UnregisterImportHandler(ICanAcceptFiles handler) => runOnImporters(importers => importers.Remove(handler));

        public async Task Import(params string[] paths)
        {
            if (paths.Length == 0)
                return;

            var filesPerExtension = paths.GroupBy(p => Path.GetExtension(p).ToLowerInvariant());
            var importers = await runOnImportersAsync(importers => importers.ToArray()).ConfigureAwait(false);

            foreach (var groups in filesPerExtension)
            {
                foreach (var importer in importers)
                {
                    if (importer.HandledExtensions.Contains(groups.Key))
                        await importer.Import(groups.ToArray()).ConfigureAwait(false);
                }
            }
        }

        public virtual async Task Import(ImportTask[] tasks, ImportParameters parameters = default)
        {
            var tasksPerExtension = tasks.GroupBy(t => Path.GetExtension(t.Path).ToLowerInvariant());
            await Task.WhenAll(tasksPerExtension.Select(taskGroup => Task.Run(async () =>
            {
                var importer = await runOnImportersAsync(importers =>
                    importers.FirstOrDefault(i => i.HandledExtensions.Contains(taskGroup.Key))).ConfigureAwait(false);

                if (importer == null)
                    return;

                await importer.Import(taskGroup.ToArray(), parameters).ConfigureAwait(false);
            }))).ConfigureAwait(false);
        }

        public IEnumerable<string> HandledExtensions => runOnImporters(importers => importers.SelectMany(i => i.HandledExtensions).ToArray());
    }
}
