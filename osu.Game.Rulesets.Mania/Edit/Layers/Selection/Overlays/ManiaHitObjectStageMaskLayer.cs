// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Screens.Edit.Screens.Compose.Layers;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Mania.Edit.Layers.Selection.Overlays
{
    public class ManiaHitObjectStageMaskLayer : HitObjectMaskLayer
    {
        public readonly List<ManiaHitObjectColumnMaskLayer> Columns;

        public ManiaHitObjectStageMaskLayer(ManiaEditPlayfield playfield, HitObjectComposer composer, ManiaStage s)
            : base(playfield, composer)
        {
            Columns = new List<ManiaHitObjectColumnMaskLayer>();
            foreach (var c in s.Columns)
                Columns.Add(new ManiaHitObjectColumnMaskLayer((ManiaEditPlayfield)Playfield, Composer, c));
        }

        public void CreateMasks() => AddMasks();

        protected override void AddMasks()
        {
            foreach (var c in Columns)
                c.CreateMasks();
        }
    }
}
