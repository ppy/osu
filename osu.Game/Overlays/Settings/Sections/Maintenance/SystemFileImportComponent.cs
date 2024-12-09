// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Platform;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class SystemFileImportComponent : Component
    {
        private readonly OsuGame game;
        private readonly GameHost host;

        private ISystemFileSelector? selector;

        public SystemFileImportComponent(OsuGame game, GameHost host)
        {
            this.game = game;
            this.host = host;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selector = host.CreateSystemFileSelector(game.HandledExtensions.ToArray());

            if (selector != null)
                selector.Selected += f => Schedule(() => startImport(f.FullName));
        }

        public bool PresentIfAvailable()
        {
            if (selector == null)
                return false;

            selector.Present();
            return true;
        }

        private void startImport(string path)
        {
            Task.Factory.StartNew(async () =>
            {
                await game.Import(path).ConfigureAwait(false);
            }, TaskCreationOptions.LongRunning);
        }
    }
}
