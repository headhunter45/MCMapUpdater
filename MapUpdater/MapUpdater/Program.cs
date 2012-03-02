using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;
using System.IO;
using FtpLib;
using System.Diagnostics;

namespace MapUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = args[0];
            YamlStream yaml = new YamlStream();
            yaml.Load(new StreamReader(File.OpenRead(filename)));

            YamlMappingNode mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            string c10tPath = "";
            List<ServerInfo> servers = new List<ServerInfo>();
            List<MapInfo> maps = new List<MapInfo>();

            foreach (KeyValuePair<YamlNode, YamlNode> entry in mapping.Children)
            {
                if(entry.Key.ToString() == "c10t_path"){
                    c10tPath = entry.Value.ToString();
                }else if(entry.Key.ToString() == "maps"){
                    if (entry.Value is YamlSequenceNode)
                    {
                        foreach (YamlNode serverNode in (entry.Value as YamlSequenceNode))
                        {
                            if (serverNode is YamlMappingNode)
                            {
                                servers.Add(ReadServerInfo(serverNode as YamlMappingNode));
                            }
                        }

                    }
                }
            }

            Console.WriteLine("config file = {0}", filename);
            Console.WriteLine("c10tPath = {0}", c10tPath);
            Console.WriteLine("num servers = {0}", servers.Count);
            foreach (ServerInfo server in servers)
            {
                string tmp = Environment.CurrentDirectory;
                DownloadFiles(server);
                Environment.CurrentDirectory = tmp;
                GenerateMapImages(server, c10tPath);
                UploadMapImages(server);
            }
        }

        private static void UploadMapImages(ServerInfo server)
        {
            string currentDirectory = Environment.CurrentDirectory;
            ConnectionInfo connection = server.WebConnection;

            if (connection.Type == ConnectionType.Ftp)
            {
                using (FtpConnection ftp = new FtpConnection(connection.Address, 21, connection.Username, connection.Password))
                {
                    ftp.Open();
                    ftp.Login();
                    ftp.SetCurrentDirectory(connection.ImagesFolder);
                    if (ftp.GetCurrentDirectory() != connection.ImagesFolder)
                    {
                        Console.WriteLine("ImagesFolder does not exist on server");
                    }

                    foreach(string file in Directory.GetFiles(Environment.CurrentDirectory, "*.png")){
                        ftp.PutFile(file);
                    }
                }
            }
        }

        private static void GenerateMapImages(ServerInfo server, string c10tPath)
        {
            Environment.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "MapUpdater";
            
            string worldName = server.World.Name;
            foreach(RegionInfo regionInfo in server.World.Regions){
                string regionSelection = regionInfo.Selection;
                foreach (ImageInfo imageInfo in regionInfo.Images)
                {
                    RenderImage(worldName, regionSelection, imageInfo, c10tPath);
                }
            }
        }

        private static void RenderImage(string worldName, string regionSelection, ImageInfo imageInfo, string c10tPath)
        {
            string worldFolder = Path.GetFullPath(Environment.CurrentDirectory) + Path.DirectorySeparatorChar + worldName;

            ProcessStartInfo startInfo = new ProcessStartInfo(c10tPath);
            startInfo.Arguments = String.Format("-w \"{0}\" -L {1} -o {2} {3} {4} {5}", worldFolder, regionSelection, imageInfo.Output, (imageInfo.Night?"-n":""), (imageInfo.Cave?"--cave":""), (imageInfo.Type == ImageType.Normal?"":imageInfo.Type == ImageType.FatIso?"--fatiso":""));
            
            Process p = new Process();
            p.StartInfo = startInfo;
            p.Start();
            p.WaitForExit();
        }

        private static void DownloadFiles(ServerInfo server)
        {
            string tempFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "MapUpdater";

            ConnectionInfo connection = server.MinecraftConnection;
            
            if (connection.Type == ConnectionType.Ftp)
            {
                using (FtpConnection ftp = new FtpConnection(connection.Address, 21, connection.Username, connection.Password))
                {
                    ftp.Open();
                    ftp.Login();
                    ftp.SetCurrentDirectory(connection.WorldsFolder);

                    if (ftp.GetCurrentDirectory() != connection.WorldsFolder)
                    {
                        Console.WriteLine("WorldsFolder does not exist on server");
                    }

                    DownloadDirectory(ftp, server.World.Name, tempFolder + Path.DirectorySeparatorChar + server.World.Name);
                }
            }
        }

        private static void DownloadDirectory(FtpConnection ftp, string p, string p_2)
        {
            string targetFolder = Path.GetFullPath(p_2);
            //string sourceFolder = ftp.GetCurrentDirectory() + '/' + p;

            DirectoryInfo localDir = new DirectoryInfo(targetFolder);
            if (localDir.Exists)
            {
                localDir.Delete(true);
            }
            
            localDir.Create();

            ftp.SetCurrentDirectory(p);
            ftp.SetLocalDirectory(localDir.FullName);
            foreach (var file in ftp.GetFiles())
            {
                string localFilename = localDir.FullName + Path.DirectorySeparatorChar + file.Name;
                if (File.Exists(localFilename))
                {
                    File.Delete(localFilename);
                }

                ftp.GetFile(file.Name, false);
            }

            foreach (var directory in ftp.GetDirectories())
            {
                Directory.CreateDirectory(directory.Name);
                DownloadDirectory(ftp, directory.Name, targetFolder + Path.DirectorySeparatorChar + directory.Name);
            }

            ftp.SetCurrentDirectory("..");
        }

        private static ServerInfo ReadServerInfo(YamlMappingNode serverNode)
        {
            ServerInfo server = new ServerInfo();

            
            foreach (KeyValuePair<YamlNode, YamlNode> entry in serverNode.Children)
            {
                if (entry.Key.ToString() == "connection")
                {
                    if (entry.Value is YamlMappingNode)
                    {
                        server.MinecraftConnection = ReadConnectionInfo(entry.Value as YamlMappingNode);
                    }
                }
                else if (entry.Key.ToString() == "webConnection")
                {
                    if (entry.Value is YamlMappingNode)
                    {
                        server.WebConnection = ReadConnectionInfo(entry.Value as YamlMappingNode);
                    }
                }
                else if (entry.Key.ToString() == "world")
                {
                    if (entry.Value is YamlMappingNode)
                    {
                        server.World = ReadWorldInfo(entry.Value as YamlMappingNode);
                    }
                }
            }

            return server;
        }

        private static WorldInfo ReadWorldInfo(YamlMappingNode yamlNode)
        {
            WorldInfo world = new WorldInfo();

            foreach (KeyValuePair<YamlNode, YamlNode> entry in yamlNode)
            {
                if (entry.Key.ToString() == "name")
                {
                    world.Name = entry.Value.ToString();
                }
                else if (entry.Key.ToString() == "regions" && entry.Value is YamlSequenceNode)
                {
                    foreach (YamlNode node in entry.Value as YamlSequenceNode)
                    {
                        if (node is YamlMappingNode)
                        {
                            world.Regions.Add(ReadRegionInfo(node as YamlMappingNode));
                        }
                    }
                }
            }

            return world;
        }

        private static RegionInfo ReadRegionInfo(YamlMappingNode yamlMappingNode)
        {
            RegionInfo region = new RegionInfo();

            foreach (KeyValuePair<YamlNode, YamlNode> entry in yamlMappingNode)
            {
                if (entry.Key.ToString() == "selection")
                {
                    region.Selection = entry.Value.ToString();
                }
                else if (entry.Key.ToString() == "images" && entry.Value is YamlSequenceNode)
                {
                    foreach (YamlNode imageNode in entry.Value as YamlSequenceNode)
                    {
                        if (imageNode is YamlMappingNode)
                        {
                            region.Images.Add(ReadImage(imageNode as YamlMappingNode));
                        }
                    }
                }
            }


            return region;
        }

        private static ImageInfo ReadImage(YamlMappingNode imageNode)
        {
            ImageInfo image = new ImageInfo();
            image.Type = ImageType.Normal;

            foreach (KeyValuePair<YamlNode, YamlNode> entry in imageNode.Children)
            {
                if (entry.Key.ToString() == "output")
                {
                    image.Output = entry.Value.ToString();
                }
                else if (entry.Key.ToString() == "night")
                {
                    image.Night = ReadBool(entry.Value);
                }
                else if (entry.Key.ToString() == "cave")
                {
                    image.Cave = ReadBool(entry.Value);
                }
                else if (entry.Key.ToString() == "type")
                {
                    string imageType = entry.Value.ToString().ToLower();

                    if (imageType == "normal")
                    {
                        image.Type = ImageType.Normal;
                    }
                    else if (imageType == "fatiso")
                    {
                        image.Type = ImageType.FatIso;
                    }
                }
            }

            return image;
        }

        private static bool ReadBool(YamlNode yamlNode)
        {
            try
            {
                return bool.Parse(yamlNode.ToString());
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static ConnectionInfo ReadConnectionInfo(YamlMappingNode connectionNode)
        {
            ConnectionInfo connection = new ConnectionInfo();

            connection.Type = ConnectionType.Ftp;

            foreach (KeyValuePair<YamlNode, YamlNode> entry in connectionNode.Children)
            {
                if (entry.Key.ToString() == "type")
                {
                    if (entry.Value.ToString() == "ftp")
                    {
                        connection.Type = ConnectionType.Ftp;
                    }
                    else
                    {
                        throw new Exception("Invalid connection type");
                    }
                }
                else if (entry.Key.ToString() == "address")
                {
                    connection.Address = entry.Value.ToString();
                }
                else if (entry.Key.ToString() == "username")
                {
                    connection.Username = entry.Value.ToString();
                }
                else if (entry.Key.ToString() == "password")
                {
                    connection.Password = entry.Value.ToString();
                }
                else if (entry.Key.ToString() == "worldsFolder")
                {
                    connection.WorldsFolder = entry.Value.ToString();
                }
                else if (entry.Key.ToString() == "imagesFolder")
                {
                    connection.ImagesFolder = entry.Value.ToString();
                }
            }

            return connection;
        }
    }
}
