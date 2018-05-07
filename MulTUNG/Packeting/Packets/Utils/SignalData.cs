using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets.Utils
{
    public enum SignalData
    {
        CircuitUpdate,
        RequestWorld,
        WorldEnd,
        Ping,
        Pong,
        BeginTransfer,
        AckTransfer,
        EndTransfer,
    }
}
