using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapUpdater
{
    class ServerInfo
    {
        public ConnectionInfo MinecraftConnection { get; set; }
        public ConnectionInfo WebConnection { get; set; }
        public WorldInfo World { get; set; }
    }
}
