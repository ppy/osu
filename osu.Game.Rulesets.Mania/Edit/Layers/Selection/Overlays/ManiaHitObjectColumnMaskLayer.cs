// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Screens.Edit.Screens.Compose.Layers;

namespace osu.Game.Rulesets.Mania.Edit.Layers.Selection.Overlays
{
    public class ManiaHitObjectColumnMaskLayer : HitObjectMaskLayer
    {
        public readonly Column Column;

        public ManiaHitObjectColumnMaskLayer(ManiaEditPlayfield playfield, HitObjectComposer composer, Column column)
            : base(playfield, composer)
        {
            Column = column;
        }

        public void CreateMasks() => AddMasks();

        protected override void AddMasks()
        {
            foreach (var obj in Column.HitObjects.Objects)
                AddMask(obj);
        }
    }
}
