using SavedObjects;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
    public class World
    {
        public List<SavedObjectV2> TopLevelObjects { get; } = new List<SavedObjectV2>();

        private World(List<SavedObjectV2> objs)
        {
            this.TopLevelObjects = objs;
        }

        public static World LoadFromFolder(string folderPath)
        {
            string regionPath = Path.Combine(folderPath, "regions", "world.tung");

            BinaryFormatter bin = new BinaryFormatter();
            List<SavedObjectV2> objs;

            using (Stream stream = File.OpenRead(regionPath))
            {
                 objs = (List<SavedObjectV2>)bin.Deserialize(stream);
            }

            return new World(objs);
        }
    }
}
