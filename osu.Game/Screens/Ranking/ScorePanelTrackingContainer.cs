// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Ranking
{
    /// <summary>
    /// A <see cref="CompositeDrawable"/> which tracks the size of a <see cref="ScorePanel"/>, to which the <see cref="ScorePanel"/> can be added or removed.
    /// </summary>
    public partial class ScorePanelTrackingContainer : CompositeDrawable
    {
        /// <summary>
        /// The <see cref="ScorePanel"/> that created this <see cref="ScorePanelTrackingContainer"/>.
        /// </summary>
        public readonly ScorePanel Panel;

        internal ScorePanelTrackingContainer(ScorePanel panel)
        {
            Panel = panel;
            Attach();
        }

        /// <summary>
        /// Detaches the <see cref="ScorePanel"/> from this <see cref="ScorePanelTrackingContainer"/>, removing it as a child.
        /// This <see cref="ScorePanelTrackingContainer"/> will continue tracking any size changes.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the <see cref="ScorePanel"/> is already detached.</exception>
        public void Detach()
        {
            if (InternalChildren.Count == 0)
                throw new InvalidOperationException("Score panel container is not attached.");

            RemoveInternal(Panel, false);
        }

        /// <summary>
        /// Attaches the <see cref="ScorePanel"/> to this <see cref="ScorePanelTrackingContainer"/>, adding it as a child.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the <see cref="ScorePanel"/> is already attached.</exception>
        public void Attach()
        {
            if (InternalChildren.Count > 0)
                throw new InvalidOperationException("Score panel container is already attached.");

            AddInternal(Panel);
        }
    }
}
