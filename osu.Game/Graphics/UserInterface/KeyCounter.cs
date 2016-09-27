//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    public class KeyCounter : FlowContainer
    {
        private List<Count> Counts = new List<Count>();

        public bool counting = true;
        public bool IsCounting
        {
            get { return counting; }
            set
            {
                foreach (Count count in Counts)
                {
                    count.IsCounting = value;
                }
                counting = value;
            }
        }

        public void AddKey(Count count)
        {
            count.IsCounting = IsCounting;
            base.Add(count);
            Counts.Add(count);
        }

        public override void Load()
        {
            base.Load();
            Direction = FlowDirection.HorizontalOnly;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            foreach (Count count in Counts)
            {
                count.TriggerMouseDown(state, args);
            }
            return false;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            foreach (Count count in Counts)
            {
                count.TriggerMouseUp(state, args);
            }
            return false;
        }
    }
}
