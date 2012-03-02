using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapUpdater
{
    enum ConnectionType { Ftp };

    class ConnectionInfo
    {
        public ConnectionType Type { get; set; }

        public string Address { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string WorldsFolder { get; set; }

        public string ImagesFolder { get; set; }
    }
}
