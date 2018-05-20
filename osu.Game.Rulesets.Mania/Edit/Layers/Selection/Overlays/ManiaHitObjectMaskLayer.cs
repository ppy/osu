// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Screens.Compose.Layers;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Mania.Edit.Layers.Selection.Overlays
{
    public class ManiaHitObjectMaskLayer : HitObjectMaskLayer
    {
        public readonly List<ManiaHitObjectStageMaskLayer> Stages;

        public ManiaHitObjectMaskLayer(ManiaEditPlayfield playfield, HitObjectComposer composer)
            : base(playfield, composer)
        {
            Stages = new List<ManiaHitObjectStageMaskLayer>();
            foreach (var s in ((ManiaEditPlayfield)Playfield).Stages)
                Stages.Add(new ManiaHitObjectStageMaskLayer((ManiaEditPlayfield)Playfield, Composer, s));
        }

        protected override void AddMasks()
        {
            foreach (var s in Stages)
                s.CreateMasks();
        }
    }
}
