# Project Description
MapUpdater is a simple application that connects to one or more ftp servers, downloads your minecraft world data, generates images and uploads them to another ftp server.
c10t is the only renderer currently supported.
Look at "sample config.yml" and make a similar one.
Then create a scheduled task in windows that runs 
MapUpdater.exe "path to config.yml"

Maps are downloaded to 
%UserProfile%\AppData\Local\MapUpdater
Images are created there as well.

This is distributed under the MIT License and makes use of two other opensource libraries that are also under the MIT License.

FtpLib from [url:http://ftplib.codeplex.com/]

YamlDotNet from [url:http://www.aaubry.net/yamldotnet.aspx]


