using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Symcol.Pieces
{
    public class HitSoundBoard : Container
    {
        public int ButtonSize = 100;

        private SampleChannel nNormal;
        private SampleChannel sNormal;
        private SampleChannel dNormal;

        private SampleChannel nWhistle;
        private SampleChannel sWhistle;
        private SampleChannel dWhistle;

        private SampleChannel nFinish;
        private SampleChannel sFinish;
        private SampleChannel dFinish;

        private SampleChannel nClap;
        private SampleChannel sClap;
        private SampleChannel dClap;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            nNormal = audio.Sample.Get($@"Gameplay/normal-hitnormal");
            sNormal = audio.Sample.Get($@"Gameplay/soft-hitnormal");
            dNormal = audio.Sample.Get($@"Gameplay/drum-hitnormal");

            nWhistle = audio.Sample.Get($@"Gameplay/normal-hitwhistle");
            sWhistle = audio.Sample.Get($@"Gameplay/soft-hitwhistle");
            dWhistle = audio.Sample.Get($@"Gameplay/drum-hitwhistle");

            nFinish = audio.Sample.Get($@"Gameplay/normal-hitfinish");
            sFinish = audio.Sample.Get($@"Gameplay/soft-hitfinish");
            dFinish = audio.Sample.Get($@"Gameplay/drum-hitfinish");

            nClap = audio.Sample.Get($@"Gameplay/normal-hitclap");
            sClap = audio.Sample.Get($@"Gameplay/soft-hitclap");
            dClap = audio.Sample.Get($@"Gameplay/drum-hitclap");

            Children = new Drawable[]
            {
                //Noramal
                new SymcolButton
                {
                    ButtonName = "Normal",
                    ButtonLabel = 'N',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkRed,
                    ButtonColorBottom = Color4.Red,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(nNormal),
                    Position = new Vector2(-ButtonSize -  ButtonSize / 2 , -ButtonSize),
                    Bind = Key.Number1
                },
                new SymcolButton
                {
                    ButtonName = "Normal",
                    ButtonLabel = 'S',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkBlue,
                    ButtonColorBottom = Color4.Blue,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(sNormal),
                    Position = new Vector2(-ButtonSize -  ButtonSize / 2 , ButtonSize),
                    Bind = Key.A
                },
                new SymcolButton
                {
                    ButtonName = "Normal",
                    ButtonLabel = 'D',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkGreen,
                    ButtonColorBottom = Color4.Green,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(dNormal),
                    Position = new Vector2(-ButtonSize -  ButtonSize / 2, 0),
                    Bind = Key.Q
                },

                //Whistle
                new SymcolButton
                {
                    ButtonName = "Whistle",
                    ButtonLabel = 'N',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkRed,
                    ButtonColorBottom = Color4.Red,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(nWhistle),
                    Position = new Vector2(-ButtonSize / 2 , -ButtonSize),
                    Bind = Key.Number2
                },
                new SymcolButton
                {
                    ButtonName = "Whistle",
                    ButtonLabel = 'S',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkBlue,
                    ButtonColorBottom = Color4.Blue,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(sWhistle),
                    Position = new Vector2(-ButtonSize / 2 , ButtonSize),
                    Bind = Key.S
                },
                new SymcolButton
                {
                    ButtonName = "Whistle",
                    ButtonLabel = 'D',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkGreen,
                    ButtonColorBottom = Color4.Green,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(dWhistle),
                    Position = new Vector2(-ButtonSize / 2, 0),
                    Bind = Key.W
                },

                //Finish
                new SymcolButton
                {
                    ButtonName = "Finish",
                    ButtonLabel = 'N',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkRed,
                    ButtonColorBottom = Color4.Red,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(nFinish),
                    Position = new Vector2(ButtonSize / 2 , -ButtonSize),
                    Bind = Key.Number3
                },
                new SymcolButton
                {
                    ButtonName = "Finish",
                    ButtonLabel = 'S',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkBlue,
                    ButtonColorBottom = Color4.Blue,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(sFinish),
                    Position = new Vector2(ButtonSize * 0.5f , ButtonSize),
                    Bind = Key.D
                },
                new SymcolButton
                {
                    ButtonName = "Finish",
                    ButtonLabel = 'D',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkGreen,
                    ButtonColorBottom = Color4.Green,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(dFinish),
                    Position = new Vector2(ButtonSize * 0.5f, 0),
                    Bind = Key.E
                },

                //Clap
                new SymcolButton
                {
                    ButtonName = "Clap",
                    ButtonLabel = 'N',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkRed,
                    ButtonColorBottom = Color4.Red,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(nClap),
                    Position = new Vector2(ButtonSize * 1.5f , -ButtonSize),
                    Bind = Key.Number4
                },
                new SymcolButton
                {
                    ButtonName = "Clap",
                    ButtonLabel = 'S',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkBlue,
                    ButtonColorBottom = Color4.Blue,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(sClap),
                    Position = new Vector2(ButtonSize * 1.5f , ButtonSize),
                    Bind = Key.F
                },
                new SymcolButton
                {
                    ButtonName = "Clap",
                    ButtonLabel = 'D',
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkGreen,
                    ButtonColorBottom = Color4.Green,
                    ButtonSize = ButtonSize,
                    Action = () => playSample(dClap),
                    Position = new Vector2(ButtonSize * 1.5f, 0),
                    Bind = Key.R
                },
            };
        }

        private void playSample(SampleChannel sample)
        {
            sample.Play();
        }
    }
}
