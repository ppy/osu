// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Ranking
{
    public class ScorePanelTrackingContainer : CompositeDrawable
    {
        public readonly ScorePanel Panel;

        public ScorePanelTrackingContainer(ScorePanel panel)
        {
            Panel = panel;
            Attach();
        }

        public void Detach()
        {
            if (InternalChildren.Count == 0)
                throw new InvalidOperationException("Score panel container is not attached.");

            RemoveInternal(Panel);
        }

        public void Attach()
        {
            if (InternalChildren.Count > 0)
                throw new InvalidOperationException("Score panel container is already attached.");

            AddInternal(Panel);
        }
    }
}
