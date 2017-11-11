using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapUpdater
{
    class WorldInfo
    {
        public string Name { get; set; }
        public List<RegionInfo> Regions { get; set; }

        public WorldInfo()
        {
            Regions = new List<RegionInfo>();
        }
    }
}
