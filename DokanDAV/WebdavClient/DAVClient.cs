using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;

namespace WebdavClient
{
    public enum DAVProtocol
    {
        HTTP,
        HTTPS
    }

    public enum DAVType
    {
        Folder,
        File
    }

    public class DAVClient
    {
        private DAVProtocol protocol;
        private string host;
        private int port;
        private String basePath;
        private String username;
        private String password;

        /*
         * Ok, a few general rules to use this class...
         * 
         * basePath MUST start with a SLASH, and MUST NOT end with a slash
         * 
         * Your relative paths, MUST start with a slash for every method :)
         * 
         */

        //TODO: closures
        /*
         * I feel like using closures but I don't know the language API well enough
         */

        public DAVClient(DAVProtocol protocol, String host, int port, String basePath, String username, String password)
        {
            this.protocol = protocol;
            this.host = host;
            this.port = port;
            this.basePath = basePath;
            this.username = username;
            this.password = password;
        }

        public bool CreateFolder(String path)
        {
            Uri uri = buildUri(path);
            int status;

            Console.WriteLine(uri);

            WebRequest request = HttpWebRequest.Create(uri);
            HttpWebResponse response;
            NetworkCredential credentials = new NetworkCredential(username, password);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
            request.Method = "MKCOL";

            using (response = (HttpWebResponse)request.GetResponse())
            {
                status = (int)response.StatusCode;
            }

            return status == 200 || status == 201;
        }

        public DAVFileInfo Info(string path)
        {
            List<DAVFileInfo> result = ParsePropfind(path, false);
            if (result.Count > 0)
            {
                return result.First();
            }
            return null;
        }

        private List<DAVFileInfo> ParsePropfind(string path, bool skipRoot)
        {
            Uri uri = buildUri(path);
            List<DAVFileInfo> result = new List<DAVFileInfo>();
            int status;
            string method = "PROPFIND";
            WebRequest request = HttpWebRequest.Create(uri);
            HttpWebResponse response;
            Stream stream;
            NetworkCredential credentials = new NetworkCredential(username, password);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
            request.Method = method;
            request.ContentType = "text/xml";

            StringBuilder propfind = new StringBuilder();
            propfind.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            propfind.Append("<propfind xmlns=\"DAV:\">");
            propfind.Append("  <allprop/>");
            propfind.Append("</propfind>");

            byte[] propfindBytes = Encoding.UTF8.GetBytes(propfind.ToString());

            request.ContentLength = propfind.Length;
            request.Headers.Add("Depth", "1");

            using (stream = request.GetRequestStream())
            {
                stream.Write(propfindBytes, 0, propfindBytes.Length);
            }

            try
            {
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    status = (int)response.StatusCode;
                    using (stream = response.GetResponseStream())
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(stream);

                        XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(xml.NameTable);

                        xmlNsManager.AddNamespace("d", "DAV:");

                        /*
                         * This is just for the jackrabbit implementation of Javanes Storage (my company's storage product)
                         * Please remove this if you want to use it elsewhere, I'm going to fix it someday, when I have
                         * the time
                         * xmlNsManager.AddNamespace("jn", "http://www.javanes.com/jackrabbit/jn");
                         */

                        foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                        {
                            XmlNode xmlNode = node.SelectSingleNode("d:href", xmlNsManager);
                            string filepath = Uri.UnescapeDataString(xmlNode.InnerXml);
                            string[] file = filepath.Split(new string[1] { basePath }, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (file.Length > 0)
                            {
                                DAVFileInfo finfo = new DAVFileInfo();
                                string currFilename = NormalizeFilename(file[file.Length - 1]);
                                if (skipRoot && currFilename == path) { continue; }
                                finfo.Name = GetNodeText(xmlNsManager, node, "d:propstat/d:prop/d:displayname");
                                finfo.DateCreated = DateTime.Parse(GetNodeText(xmlNsManager, node, "d:propstat/d:prop/d:creationdate"));
                                finfo.LastModified = DateTime.Parse(GetNodeText(xmlNsManager, node, "d:propstat/d:prop/d:getlastmodified"));

                                if (GetNodeText(xmlNsManager, node, "d:propstat/d:prop/d:iscollection").Equals("1"))
                                {
                                    finfo.Type = DAVType.Folder;
                                }
                                else
                                {
                                    finfo.Type = DAVType.File;
                                }

                                if (finfo.Type == DAVType.File)
                                {
                                    try
                                    {
                                        finfo.Length = int.Parse(GetNodeText(xmlNsManager, node, "d:propstat/d:prop/d:getcontentlength"));
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                        finfo.Length = 0;
                                    }
                                }

                                result.Add(finfo);
                            }
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

            return result;
        }

        public List<DAVFileInfo> List(string path)
        {
            return ParsePropfind(path, true);
        }

        private String GetNodeText(XmlNamespaceManager manager, XmlNode parent, String xpath)
        {
            return parent.SelectSingleNode(xpath, manager).InnerText;
        }

        public bool Delete(String path)
        {
            Uri uri = buildUri(path);
            int status;
            string method = "DELETE";
            WebRequest request = HttpWebRequest.Create(uri);
            HttpWebResponse response;
            NetworkCredential credentials = new NetworkCredential(username, password);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
            request.Method = method;

            using (response = (HttpWebResponse)request.GetResponse())
            {
                status = (int)response.StatusCode;
            }

            return status == 204;
        }

        public bool Upload(String localPath, String remotePath)
        {
            FileInfo fileInfo = new FileInfo(localPath);
            Uri uploadUri = buildUri(remotePath);

            int status;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uploadUri);
            HttpWebResponse response;
            Stream stream;

            NetworkCredential credentials = new NetworkCredential(username, password);

            request.Credentials = credentials;
            request.PreAuthenticate = true;
            request.Method = "PUT";
            request.ContentLength = fileInfo.Length;

            using (stream = request.GetRequestStream())
            {
                using(FileStream fs = File.Open(localPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] content = new byte[4096];
                    int bytesRead = 0;
                    do
                    {
                        bytesRead = fs.Read(content, 0, content.Length);
                        stream.Write(content, 0, bytesRead);
                    } while (bytesRead > 0);
                }
            }

            using (response = (HttpWebResponse)request.GetResponse())
            {
                status = (int)response.StatusCode;
            }

            return status == 201;
        }

        public bool Download(String localPath, String remotePath)
        {
            Uri downloadUri = buildUri(remotePath);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(downloadUri);

            NetworkCredential credentials = new NetworkCredential(username, password);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
            request.Method = "GET";

            int status;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                status = (int)response.StatusCode;

                using (Stream stream = response.GetResponseStream())
                {
                    using (FileStream fs = File.Open(localPath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] content = new byte[4096];
                        int bytesRead = 0;
                        do
                        {
                            bytesRead = stream.Read(content, 0, content.Length);
                            fs.Write(content, 0, bytesRead);
                        } while (bytesRead > 0);
                    }
                }
            }

            return true;
        }

        public bool Copy(String source, String destination)
        {
            Uri copyUri = buildUri(source);
            Uri destUri = buildUri(destination);
            string method = "COPY";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(copyUri);
            HttpWebResponse response;
            NetworkCredential credentials = new NetworkCredential(username, password);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
            request.Method = method;

            request.Headers.Add("Destination", destUri.AbsolutePath);
            
            int status;

            using (response = (HttpWebResponse)request.GetResponse())
            {
                status = (int)response.StatusCode;
            }

            return status == 201;
        }

        public bool Move(String source, String destination)
        {
            Uri moveUri = buildUri(source);
            Uri destUri = buildUri(destination);
            string method = "MOVE";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(moveUri);
            HttpWebResponse response;
            NetworkCredential credentials = new NetworkCredential(username, password);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
            request.Method = method;

            request.Headers.Add("Destination", destUri.AbsolutePath);

            int status;

            using (response = (HttpWebResponse)request.GetResponse())
            {
                status = (int)response.StatusCode;
            }

            return status == 201;
        }

        public bool Exists(string path)
        {
            int status;
            Uri uri = buildUri(path);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            HttpWebResponse response;
            NetworkCredential credentials = new NetworkCredential(username, password);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
            request.Method = "HEAD";

            try
            {
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    status = (int)response.StatusCode;
                }
            }
            catch
            {
                return false;
            }

            return status != 404;

        }

        public int GetSize(String path)
        {
            int result;
            Uri uri = buildUri(path);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            HttpWebResponse response;
            NetworkCredential credentials = new NetworkCredential(username, password);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
            request.Method = "HEAD";


            using (response = (HttpWebResponse)request.GetResponse())
            {
                try
                {
                    result = int.Parse(response.GetResponseHeader("Content-Length"));
                }
                catch
                {
                    result = 0;
                }
            }

            return result;
        }

        public DAVType GetType(String path)
        {
            DAVType result;
            int status;
            Uri uri = buildUri(path);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            HttpWebResponse response;
            NetworkCredential credentials = new NetworkCredential(username, password);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
            request.Method = "HEAD";


            using (response = (HttpWebResponse)request.GetResponse())
            {
                status = (int)response.StatusCode;

                if (status == 200 || status == 201)
                {
                    try
                    {
                        if (int.Parse(response.GetResponseHeader("Content-Length")) == 0)
                        {
                            result = DAVType.Folder;
                        }
                        else
                        {
                            result = DAVType.File;
                        }
                    }
                    catch
                    {
                        result = DAVType.Folder;
                    }

                }
                else
                {
                    throw new FileNotFoundException();
                }

            }

            
            return result;
        }

        private string NormalizeFilename(string filename)
        {
            if (filename == "/")
            {
                return "/";
            }
            else
            {
                return filename.Substring(0, filename.Length - 1);
            }
        }

        private Uri buildUri(String path)
        {
            String newPath = String.Format("{0}://{1}:{2}{3}{4}",
                protocol.ToString().ToLower(),
                host,
                port,
                basePath,
                path);
            return new Uri(newPath);
        }

    }
}
