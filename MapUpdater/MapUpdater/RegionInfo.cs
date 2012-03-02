using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapUpdater
{
    class RegionInfo
    {
        public string Selection { get; set; }
        public List<ImageInfo> Images { get; set; }

        public RegionInfo()
        {
            Images = new List<ImageInfo>();
        }
    }
}
