using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Vitaru.UI
{
    class Character : Container
    {
        public int Health { get; set; }
        public Action Shoot;
        public int HitboxRadius { get; set; }

        

    }
}
