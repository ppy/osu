using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class MaskContainer : Container<HitObjectMask>
    {
        /// <summary>
        /// Invoked when any <see cref="HitObjectMask"/> is selected.
        /// </summary>
        public event Action<HitObjectMask> MaskSelected;

        /// <summary>
        /// Invoked when any <see cref="HitObjectMask"/> is deselected.
        /// </summary>
        public event Action<HitObjectMask> MaskDeselected;

        /// <summary>
        /// All the <see cref="HitObjectMask"/>s with <see cref="IsAlive"/> == true.
        /// </summary>
        public IEnumerable<HitObjectMask> AliveMasks => AliveInternalChildren.Cast<HitObjectMask>();

        public override void Add(HitObjectMask drawable)
        {
            base.Add(drawable);

            drawable.Selected += onMaskSelected;
            drawable.Deselected += onMaskDeselected;
        }

        public override bool Remove(HitObjectMask drawable)
        {
            var result = base.Remove(drawable);

            if (result)
            {
                drawable.Selected -= onMaskSelected;
                drawable.Deselected += onMaskDeselected;
            }

            return result;
        }

        private void onMaskSelected(HitObjectMask mask) => MaskSelected?.Invoke(mask);
        private void onMaskDeselected(HitObjectMask mask) => MaskDeselected?.Invoke(mask);
    }
}
