using Lidgren.Network;
using MulTUNG.Packeting.Packets;

namespace MulTUNG
{
    public interface ISender
    {
        void Send(Packet packet, NetDeliveryMethod delivery = NetDeliveryMethod.ReliableOrdered);
    }
}
