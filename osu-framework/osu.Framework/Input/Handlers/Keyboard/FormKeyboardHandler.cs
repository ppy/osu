//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using OpenTK.Input;

namespace osu.Framework.Input.Handlers.Keyboard
{
    class FormKeyboardHandler : InputHandler, IKeyboardInputHandler
    {
        private Form form;

        public override bool IsActive => true;

        public override int Priority => 0;

        public FormKeyboardHandler(Form form)
        {
            this.form = form;
            form.KeyPreview = true;
            form.KeyDown += keyDown;
            form.KeyUp += keyUp;
            form.Deactivate += onDeactivate;
        }

        private void onDeactivate(object sender, EventArgs e)
        {
            PressedKeys.Clear();
        }

        private void keyUp(object sender, KeyEventArgs e)
        {
            Key tkKey = getTkKeyFrom(e.KeyCode);
            if (tkKey != Key.Unknown)
                PressedKeys.Remove(tkKey);
        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            Key tkKey = getTkKeyFrom(e.KeyCode);
            if (tkKey != Key.Unknown && !PressedKeys.Contains(tkKey))
                PressedKeys.Add(tkKey); 
        }

        private Key getTkKeyFrom(Keys keyCode)
        {
            Key key;

            switch (keyCode)
            {
                case Keys.Menu:
                    return Key.LAlt;
                case Keys.ShiftKey:
                    return Key.LShift;
                case Keys.ControlKey:
                    return Key.LControl;
                case Keys.D0: return Key.Number0;
                case Keys.D1: return Key.Number1;
                case Keys.D2: return Key.Number2;
                case Keys.D3: return Key.Number3;
                case Keys.D4: return Key.Number4;
                case Keys.D5: return Key.Number5;
                case Keys.D6: return Key.Number6;
                case Keys.D7: return Key.Number7;
                case Keys.D8: return Key.Number8;
                case Keys.D9: return Key.Number9;
                case Keys.OemQuestion: return Key.Slash;
                case Keys.Oem5: return Key.BackSlash;
                case Keys.Oemtilde: return Key.Tilde;
                case Keys.Return: return Key.Enter;
                case Keys.OemPeriod: return Key.Period;
                case Keys.Oemcomma: return Key.Comma;
                case Keys.Oem1: return Key.Semicolon;
                case Keys.Oem7: return Key.Quote;
                case Keys.OemOpenBrackets: return Key.BracketLeft;
                case Keys.Oem6: return Key.BracketRight;

            }


            if (!Enum.TryParse(keyCode.ToString(), out key))
                Debug.WriteLine($@"Failed to parse key {keyCode}");
            return key;
        }

        public override void Dispose()
        {
            form.KeyDown -= keyDown;
            form.KeyUp -= keyUp;
        }

        public override bool Initialize()
        {
            PressedKeys = new List<Key>();
            return true;
        }

        public override void UpdateInput(bool isActive)
        {
        }

        public List<Key> PressedKeys { get; set; }
    }
}
