using OpenTK;
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
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko.UI.Drums
{
    class TaikoHalfDrum : Container
    {
        private Sprite outer;
        private Sprite inner;

        private Key outerKey;
        private Key innerKey;

        public override bool Contains(Vector2 screenSpacePos) => true;

        public TaikoHalfDrum(Key[] keys)
        {
            outerKey = keys[0];
            innerKey = keys[1];

            RelativeSizeAxes = Axes.Both;

            Children = new[]
            {
                outer = new Sprite()
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                },
                inner = new Sprite()
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,

                    RelativeSizeAxes = Axes.Both,

                    Scale = new Vector2(-1, 1),
                    Alpha = 0
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            outer.Texture = textures.Get($"Play/Taiko/taiko-drum-outer@2x");
            inner.Texture = textures.Get($"Play/Taiko/taiko-drum-inner@2x");
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == outerKey)
                outer.FadeTo(1f, 0);

            if (args.Key == innerKey)
                inner.FadeTo(1f, 0);

            return false;
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (args.Key == outerKey)
                outer.FadeTo(0f, 100);

            if (args.Key == innerKey)
                inner.FadeTo(0f, 100);

            return false;
        }
    }

    enum DrumSetType
    {
        Outer,
        Inner
    }
}
