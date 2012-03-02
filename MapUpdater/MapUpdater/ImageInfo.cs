using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapUpdater
{
    public enum ImageType { Normal, FatIso }

    class ImageInfo
    {
        public string Output { get; set; }

        public bool Night { get; set; }

        public bool Cave { get; set; }

        public ImageType Type { get; set; }
    }
}
