using System;
using System.Collections.Generic;

namespace M.DBus.Tray
{
    public class SimpleEntry
    {
        public string Name { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public bool Visible { get; set; } = true;

        public int ToggleState { get; set; } = 0;

        public string ToggleType { get; set; } = "checkmark";

        public Action OnActive { get; set; }

        public override string ToString() => $"DBusMenuEntry '{Name}'";

        public virtual void AfterCast(IDictionary<string, object> dictionary)
        {
        }
    }
}
