using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.UI.Drums
{
    public class TaikoDrumInner : FlowContainer
    {
        private TaikoHalfDrum rightHalf;
        private TaikoHalfDrum leftHalf;

        public TaikoDrumInner(Key[] keys)
        {
            Children = new Drawable[]
            {
                leftHalf = new TaikoHalfDrum(new[] { keys[0], keys[1] })
                {
                    Origin = Anchor.TopRight,

                    RelativeSizeAxes = Axes.Both,
                    Scale = new Vector2(-0.5f, 1)
                },
                rightHalf = new TaikoHalfDrum(new[] { keys[2], keys[3] })
                {
                    RelativeSizeAxes = Axes.Both,
                    Scale = new Vector2(0.5f, 1)
                },
            };
        }
    }
}
