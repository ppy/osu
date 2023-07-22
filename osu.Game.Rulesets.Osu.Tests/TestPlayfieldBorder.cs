// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestPlayfieldBorder : OsuTestScene
    {
        public TestPlayfieldBorder()
        {
            Bindable<PlayfieldBorderStyle> playfieldBorderStyle = new Bindable<PlayfieldBorderStyle>();

            AddStep("add drawables", () =>
            {
                Child = new Container
                {
                    Size = new Vector2(400, 300),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new PlayfieldBorder
                        {
                            PlayfieldBorderStyle = { BindTarget = playfieldBorderStyle }
                        }
                    }
                };
            });

            AddStep("Set none", () => playfieldBorderStyle.Value = PlayfieldBorderStyle.None);
            AddStep("Set corners", () => playfieldBorderStyle.Value = PlayfieldBorderStyle.Corners);
            AddStep("Set full", () => playfieldBorderStyle.Value = PlayfieldBorderStyle.Full);
        }
    }
}
