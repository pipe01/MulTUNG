using MulTUNG.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Packet_Log_Viewer
{
    public class PacketLog
    {
        public ObservableCollection<PacketLogEntry> Entries { get; private set; } = new ObservableCollection<PacketLogEntry>();

        public static PacketLog Load(string file)
        {
            var bin = new BinaryFormatter();
            List<PacketLogEntry> entries = new List<PacketLogEntry>();

            using (var fileStream = File.OpenRead(file))
            {
                while (true)
                {
                    try
                    {
                        var entry = (PacketLogEntry)bin.Deserialize(fileStream);

                        entries.Add(entry);
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            return new PacketLog
            {
                Entries = new ObservableCollection<PacketLogEntry>(entries)
            };
        }
    }
}
