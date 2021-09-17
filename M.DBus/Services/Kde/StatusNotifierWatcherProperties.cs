using System.Diagnostics.CodeAnalysis;
using Tmds.DBus;

namespace M.DBus.Services.Kde
{
    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class StatusNotifierWatcherProperties
    {
        public string[] RegisteredStatusNotifierItems
        {
            get => _RegisteredStatusNotifierItems;

            set => _RegisteredStatusNotifierItems = value;
        }

        public bool IsStatusNotifierHostRegistered
        {
            get => _IsStatusNotifierHostRegistered;

            set => _IsStatusNotifierHostRegistered = value;
        }

        public int ProtocolVersion
        {
            get => _ProtocolVersion;

            set => _ProtocolVersion = value;
        }

        private string[] _RegisteredStatusNotifierItems;

        private bool _IsStatusNotifierHostRegistered;

        private int _ProtocolVersion;
    }
}
