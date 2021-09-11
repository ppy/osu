using System;
using System.Collections.Generic;

namespace M.DBus.Tray
{
    public class SingleEntry : Dictionary<string, object>
    {
        public string Name
        {
            get => (string)this["label"];
            set => this["label"] = value;
        }

        public bool Enabled
        {
            get => (bool)this["enabled"];
            set => this["enabled"] = value;
        }

        public bool Visible
        {
            get => (bool)this["visible"];
            set => this["visible"] = value;
        }

        public int ToggleState
        {
            get => (int)this["toggle-state"];
            set => this["toggle-state"] = value;
        }

        public string ToggleType
        {
            get => (string)this["toggle-type"];
            set => this["toggle-type"] = value;
        }

        public Action OnActive { get; set; }

        public SingleEntry()
        {
            this["label"] = string.Empty;
            this["enabled"] = true;
            this["visible"] = true;
            this["toggle-state"] = 0;
            this["toggle-type"] = "checkmark";
        }

        public IDictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>();

            foreach (var keyValuePair in this)
            {
                result[keyValuePair.Key] = keyValuePair.Value;
            }

            return result;
        }

        public override string ToString() => $"DBusMenuEntry '{Name}'";
    }
}
