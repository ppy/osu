// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class QueueModePill : OnlinePlayComposite
    {
        private OsuTextFlowContainer textFlow;

        public QueueModePill()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new PillContainer
            {
                Child = textFlow = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 12))
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            QueueMode.BindValueChanged(onQueueModeChanged, true);
        }

        private void onQueueModeChanged(ValueChangedEvent<QueueMode> mode)
        {
            textFlow.Clear();
            textFlow.AddText(mode.NewValue.GetLocalisableDescription());
        }
    }
}
