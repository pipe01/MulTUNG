using MulTUNG.Packeting.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG
{
    public interface ISender
    {
        void Send(Packet packet);
    }
}
