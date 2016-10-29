using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace xDM.xCommon
{
    public class MyWeb
    {

        /// <summary>
        /// 用TCP方式获取Html
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        static public string GetHtmlWithTCP(string URL)
        {
            string strHTML = "";//用来保存获得的HTML代码
            TcpClient clientSocket = new TcpClient();
            Uri URI = new Uri(URL);
            clientSocket.Connect(URI.Host, URI.Port);
            StringBuilder RequestHeaders = new StringBuilder();//用来保存HTML协议头部信息
            RequestHeaders.AppendFormat("{0} {1} HTTP/1.1\r\n", "GET", URI.PathAndQuery);
            RequestHeaders.AppendFormat("Connection:close\r\n");
            RequestHeaders.AppendFormat("Host:{0}\r\n", URI.Host);
            RequestHeaders.AppendFormat("Accept:*/*\r\n");
            RequestHeaders.AppendFormat("Accept-Language:zh-cn\r\n");
            RequestHeaders.AppendFormat("User-Agent:Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)\r\n\r\n");
            System.Text.Encoding encoding = System.Text.Encoding.Default;
            byte[] request = encoding.GetBytes(RequestHeaders.ToString());
            clientSocket.Client.Send(request);
            //获取要保存的网络流
            if (URI.Scheme.ToLower() == "http")
            {
                Stream readStream = clientSocket.GetStream();
                List<byte> bs = new List<byte>();
                int b = -1;
                while ((b = readStream.ReadByte()) != -1)
                {
                    bs.Add((byte)b);
                }
                byte[] bHtml = bs.ToArray();
                readStream.Close();
                clientSocket.Close();
                strHTML = System.Text.Encoding.Default.GetString(bHtml);
                return GetHtmlEncoding(strHTML).GetString(bHtml);
            }
            else if (URI.Scheme.ToLower() == "https")
            {
                SslStream sslStream = new SslStream(clientSocket.GetStream(), true
    , new RemoteCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors)
        =>
    {
        return sslPolicyErrors == SslPolicyErrors.None;
    }
        ), null);


                X509Store store = new X509Store(StoreName.My);
                sslStream.AuthenticateAsClient(URI.Host, store.Certificates, System.Security.Authentication.SslProtocols.Default, false);

                List<byte> bs = new List<byte>();
                int b = -1;
                while ((b = sslStream.ReadByte()) != -1)
                {
                    bs.Add((byte)b);
                }
                byte[] bHtml = bs.ToArray();
                sslStream.Close();
                clientSocket.Close();
                strHTML = System.Text.Encoding.Default.GetString(bHtml);
                return GetHtmlEncoding(strHTML).GetString(bHtml);
            }
            else return null;

        }

        /// <summary>
        /// 从HTML中获取获取charset
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static System.Text.Encoding GetHtmlEncoding(string html)
        {
            string charset = "";
            Regex regCharset = new Regex(@"content=[""'].*\s*charset\b\s*=\s*""?(?<charset>[^""']*)", RegexOptions.IgnoreCase);
            if (regCharset.IsMatch(html))
            {
                charset = regCharset.Match(html).Groups["charset"].Value;
            }
            if (charset.Equals(""))
            {
                regCharset = new Regex(@"<\s*meta\s*charset\s*=\s*[""']?(?<charset>[^""']*)", RegexOptions.IgnoreCase);
                if (regCharset.IsMatch(html))
                {
                    charset = regCharset.Match(html).Groups["charset"].Value;
                }
            }
            System.Text.Encoding encoding = System.Text.Encoding.Default;
            try
            {
                encoding = System.Text.Encoding.GetEncoding(charset);
            }
            catch
            {
                switch (charset.ToUpper())
                {
                    case "UTF8":
                        encoding = System.Text.Encoding.UTF8;
                        break;
                }
            }
            return encoding;
        }

        class CertPolicy : ICertificatePolicy
        {
            public bool CheckValidationResult(ServicePoint srvPoint,
        X509Certificate certificate, WebRequest request, int certificateProblem)
            {
                // You can do your own certificate checking.
                // You can obtain the error values from WinError.h.

                // Return true so that any certificate will work with this sample.
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cer"></param>
        /// <param name="protocolType"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static CookieContainer GetCookie(string postUrl, string postData, X509Certificate2 cer, SecurityProtocolType protocolType, out string content)
        {
            CookieContainer cookie = new CookieContainer();
            content = GetContent(cookie, postUrl, postData, cer, protocolType);
            return cookie;
        }

        /// <summary>
        /// post方式
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="postData"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static CookieContainer GetCookie(string postUrl, string postData, out string content)
        {
            CookieContainer cookie = new CookieContainer();
            content = GetContent(cookie, postUrl, postData);
            return cookie;
        }
        /// <summary>
        /// get方式
        /// </summary>
        /// <param name="getUrl"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static CookieContainer GetCookie(string getUrl, out string content)
        {
            CookieContainer cookie = new CookieContainer();
            content = GetContent(cookie, getUrl);
            return cookie;
        }
        public static CookieContainer GetCookie(string getUrl,X509Certificate2 cer, SecurityProtocolType protocolType, out string content)
        {
            CookieContainer cookie = new CookieContainer();
            content = GetContent(cookie, getUrl,cer,protocolType);
            return cookie;
        }
        /// <summary>
        /// get方式
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="getUrl"></param>
        /// <returns></returns>
        public static string GetContent(CookieContainer cookie, string getUrl)
        {
            string content;
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(getUrl);
            if (cookie != null)
            {
                httpRequest.CookieContainer = cookie;
            }
            httpRequest.KeepAlive = true;
            httpRequest.Referer = getUrl;
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            httpRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.Method = "GET";

            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            using (Stream responsestream = httpResponse.GetResponseStream())
            {

                using (StreamReader sr = new StreamReader(responsestream, System.Text.Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }
            }

            return content;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="getUrl"></param>
        /// <param name="cer"></param>
        /// <param name="protocolType"></param>
        /// <returns></returns>
        public static string GetContent(CookieContainer cookie, string getUrl,X509Certificate2 cer,SecurityProtocolType protocolType)
        {
            string content;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, cert, chain, errors) =>
            {
                return true;
            });
            ServicePointManager.SecurityProtocol = protocolType;

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(getUrl);
            if (cookie != null)
            {
                httpRequest.CookieContainer = cookie;
            }
            if (cer != null)
            {
                httpRequest.ClientCertificates.Add(cer);
            }
            httpRequest.Referer = getUrl;
            httpRequest.Method = "GET";
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            httpRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            //httpRequest.Headers.Set(HttpRequestHeader.Pragma, "no-cache");
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            using (Stream responsestream = httpResponse.GetResponseStream())
            {

                using (StreamReader sr = new StreamReader(responsestream, System.Text.Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }
            }

            return content;
        }

        /// <summary>
        /// post方式
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="postUrl"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static string GetContent(CookieContainer cookie, string postUrl,string postData)
        {
            string content;
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(postUrl);
            if (cookie != null)
            {
                httpRequest.CookieContainer = cookie;
            }
            httpRequest.Method = "Post";
            httpRequest.KeepAlive = true;
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            httpRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpRequest.ContentType = "application/x-www-form-urlencoded";//以上信息在监听请求的时候都有的直接复制过来
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postData);
            httpRequest.ContentLength = bytes.Length;
            Stream stream = httpRequest.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();//以上是POST数据的写入
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();//获得 服务端响应
            using (Stream responsestream = httpResponse.GetResponseStream())
            {

                using (StreamReader sr = new StreamReader(responsestream, System.Text.Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }
            }

            return content;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="postUrl"></param>
        /// <param name="postData"></param>
        /// <param name="cer"></param>
        /// <param name="protocolType"></param>
        /// <returns></returns>
        public static string GetContent(CookieContainer cookie, string postUrl, string postData, X509Certificate2 cer, SecurityProtocolType protocolType)
        {
            string content = string.Empty;
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(postUrl);
            if (cookie != null)
            {
                httpRequest.CookieContainer = cookie;
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, cert, chain, errors) =>
            {
                return true;
            });
            ServicePointManager.SecurityProtocol = protocolType;
            if (cer != null)
            {
                httpRequest.ClientCertificates.Add(cer);
            }
            httpRequest.Method = "Post";
            httpRequest.KeepAlive = true;
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            httpRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpRequest.ContentType = "application/x-www-form-urlencoded";//以上信息在监听请求的时候都有的直接复制过来
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postData);
            httpRequest.ContentLength = bytes.Length;
            Stream stream = httpRequest.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();//以上是POST数据的写入
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();//获得 服务端响应
            using (Stream responsestream = httpResponse.GetResponseStream())
            {

                using (StreamReader sr = new StreamReader(responsestream, System.Text.Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }
            }
            return content;
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="cookie">可为null</param>
        /// <param name="url">提交地址</param>
        /// <param name="file">文件</param>
        /// <param name="paramName">name属性值</param>
        /// <param name="nvc0">其他参数</param>
        /// <returns></returns>
        public static bool HttpUploadFile(CookieContainer cookie, string url, string file, string paramName, NameValueCollection nvc, out string content)
        {
            content = string.Empty;
            var nvc0 = nvc;
            if (nvc0 == null)
            {
                nvc0 = new NameValueCollection();
            }
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            if (cookie != null)
            {
                httpRequest.CookieContainer = cookie;
            }
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            httpRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            httpRequest.Method = "POST";
            httpRequest.KeepAlive = true;
            httpRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = httpRequest.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc0.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc0[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);
            var contentType = GetContentType(Path.GetExtension(file));
            var header = $"Content-Disposition: form-data; name=\"{paramName}\"; filename=\"{file}\"\r\nContent-Type: {contentType}\r\n\r\n";
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = httpRequest.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);

                content = reader2.ReadToEnd();
            }
            catch (Exception ex)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                return false;
            }
            finally
            {
                httpRequest = null;
            }
            return true;
        }

        #region static string AllContentType = ....
        static string AllContentType = "*=application/octet-stream|001=application/x-001|301=application/x-301|323=text/h323|906=application/x-906|907=drawing/907|a11=application/x-a11|acp=audio/x-mei-aac|ai=application/postscript|aif=audio/aiff|aifc=audio/aiff|aiff=audio/aiff|anv=application/x-anv|asa=text/asa|asf=video/x-ms-asf|asp=text/asp|asx=video/x-ms-asf|au=audio/basic|avi=video/avi|awf=application/vnd.adobe.workflow|biz=text/xml|bmp=application/x-bmp|bot=application/x-bot|c4t=application/x-c4t|c90=application/x-c90|cal=application/x-cals|cat=application/s-pki.seccat|cdf=application/x-netcdf|cdr=application/x-cdr|cel=application/x-cel|cer=application/x-x509-ca-cert|cg4=application/x-g4|cgm=application/x-cgm|cit=application/x-cit|class=java/*|cml=text/xml|cmp=application/x-cmp|cmx=application/x-cmx|cot=application/x-cot|crl=application/pkix-crl|crt=application/x-x509-ca-cert|csi=application/x-csi|css=text/css|cut=application/x-cut|dbf=application/x-dbf|dbm=application/x-dbm|dbx=application/x-dbx|dcd=text/xml|dcx=application/x-dcx|der=application/x-x509-ca-cert|dgn=application/x-dgn|dib=application/x-dib|dll=application/x-msdownload|doc=application/msword|dot=application/msword|drw=application/x-drw|dtd=text/xml|dwf=model/vnd.dwf|dwf=application/x-dwf|dwg=application/x-dwg|dxb=application/x-dxb|dxf=application/x-dxf|edn=application/vnd.adobe.edn|emf=application/x-emf|eml=message/rfc822|ent=text/xml|epi=application/x-epi|eps=application/x-ps|eps=application/postscript|etd=application/x-ebx|exe=application/x-msdownload|fax=image/fax|fdf=application/vnd.fdf|fif=application/fractals|fo=text/xml|frm=application/x-frm|g4=application/x-g4|gbr=application/x-gbr|gcd=application/x-gcd|gif=image/gif|gl2=application/x-gl2|gp4=application/x-gp4|hgl=application/x-hgl|hmr=application/x-hmr|hpg=application/x-hpgl|hpl=application/x-hpl|hqx=application/mac-binhex40|hrf=application/x-hrf|hta=application/hta|htc=text/x-component|htm=text/html|html=text/html|htt=text/webviewhtml|htx=text/html|icb=application/x-icb|ico=image/x-icon|ico=application/x-ico|iff=application/x-iff|ig4=application/x-g4|igs=application/x-igs|iii=application/x-iphone|img=application/x-img|ins=application/x-internet-signup|isp=application/x-internet-signup|ivf=video/x-ivf|java=java/*|jfif=image/jpeg|jpe=image/jpeg|jpe=application/x-jpe|jpeg=image/jpeg|jpg=image/jpeg|jpg=application/x-jpg|js=application/x-javascript|jsp=text/html|la1=audio/x-liquid-file|lar=application/x-laplayer-reg|latex=application/x-latex|lavs=audio/x-liquid-secure|lbm=application/x-lbm|lmsff=audio/x-la-lms|ls=application/x-javascript|ltr=application/x-ltr|m1v=video/x-mpeg|m2v=video/x-mpeg|m3u=audio/mpegurl|m4e=video/mpeg4|mac=application/x-mac|man=application/x-troff-man|math=text/xml|mdb=application/msaccess|mdb=application/x-mdb|mfp=application/x-shockwave-flash|mht=message/rfc822|mhtml=message/rfc822|mi=application/x-mi|mid=audio/mid|midi=audio/mid|mil=application/x-mil|mml=text/xml|mnd=audio/x-musicnet-download|mns=audio/x-musicnet-stream|mocha=application/x-javascript|movie=video/x-sgi-movie|mp1=audio/mp1|mp2=audio/mp2|mp2v=video/mpeg|mp3=audio/mp3|mp4=video/mp4|mpa=video/x-mpg|mpd=application/-project|mpe=video/x-mpeg|mpeg=video/mpg|mpg=video/mpg|mpga=audio/rn-mpeg|mpp=application/-project|mps=video/x-mpeg|mpt=application/-project|mpv=video/mpg|mpv2=video/mpeg|mpw=application/s-project|mpx=application/-project|mtx=text/xml|mxp=application/x-mmxp|net=image/pnetvue|nrf=application/x-nrf|nws=message/rfc822|odc=text/x-ms-odc|out=application/x-out|p10=application/pkcs10|p12=application/x-pkcs12|p7b=application/x-pkcs7-certificates|p7c=application/pkcs7-mime|p7m=application/pkcs7-mime|p7r=application/x-pkcs7-certreqresp|p7s=application/pkcs7-signature|pc5=application/x-pc5|pci=application/x-pci|pcl=application/x-pcl|pcx=application/x-pcx|pdf=application/pdf|pdx=application/vnd.adobe.pdx|pfx=application/x-pkcs12|pgl=application/x-pgl|pic=application/x-pic|pko=application-pki.pko|pl=application/x-perl|plg=text/html|pls=audio/scpls|plt=application/x-plt|png=image/png|png=application/x-png|pot=applications-powerpoint|ppa=application/vs-powerpoint|ppm=application/x-ppm|pps=application-powerpoint|ppt=applications-powerpoint|ppt=application/x-ppt|pr=application/x-pr|prf=application/pics-rules|prn=application/x-prn|prt=application/x-prt|ps=application/x-ps|ps=application/postscript|ptn=application/x-ptn|pwz=application/powerpoint|r3t=text/vnd.rn-realtext3d|ra=audio/vnd.rn-realaudio|ram=audio/x-pn-realaudio|ras=application/x-ras|rat=application/rat-file|rdf=text/xml|rec=application/vnd.rn-recording|red=application/x-red|rgb=application/x-rgb|rjs=application/vnd.rn-realsystem-rjs|rjt=application/vnd.rn-realsystem-rjt|rlc=application/x-rlc|rle=application/x-rle|rm=application/vnd.rn-realmedia|rmf=application/vnd.adobe.rmf|rmi=audio/mid|rmj=application/vnd.rn-realsystem-rmj|rmm=audio/x-pn-realaudio|rmp=application/vnd.rn-rn_music_package|rms=application/vnd.rn-realmedia-secure|rmvb=application/vnd.rn-realmedia-vbr|rmx=application/vnd.rn-realsystem-rmx|rnx=application/vnd.rn-realplayer|rp=image/vnd.rn-realpix|rpm=audio/x-pn-realaudio-plugin|rsml=application/vnd.rn-rsml|rt=text/vnd.rn-realtext|rtf=application/msword|rtf=application/x-rtf|rv=video/vnd.rn-realvideo|sam=application/x-sam|sat=application/x-sat|sdp=application/sdp|sdw=application/x-sdw|sit=application/x-stuffit|slb=application/x-slb|sld=application/x-sld|slk=drawing/x-slk|smi=application/smil|smil=application/smil|smk=application/x-smk|snd=audio/basic|sol=text/plain|sor=text/plain|spc=application/x-pkcs7-certificates|spl=application/futuresplash|spp=text/xml|ssm=application/streamingmedia|sst=application-pki.certstore|stl=application/-pki.stl|stm=text/html|sty=application/x-sty|svg=text/xml|swf=application/x-shockwave-flash|tdf=application/x-tdf|tg4=application/x-tg4|tga=application/x-tga|tif=image/tiff|tif=application/x-tif|tiff=image/tiff|tld=text/xml|top=drawing/x-top|torrent=application/x-bittorrent|tsd=text/xml|txt=text/plain|uin=application/x-icq|uls=text/iuls|vcf=text/x-vcard|vda=application/x-vda|vdx=application/vnd.visio|vml=text/xml|vpg=application/x-vpeg005|vsd=application/vnd.visio|vsd=application/x-vsd|vss=application/vnd.visio|vst=application/vnd.visio|vst=application/x-vst|vsw=application/vnd.visio|vsx=application/vnd.visio|vtx=application/vnd.visio|vxml=text/xml|wav=audio/wav|wax=audio/x-ms-wax|wb1=application/x-wb1|wb2=application/x-wb2|wb3=application/x-wb3|wbmp=image/vnd.wap.wbmp|wiz=application/msword|wk3=application/x-wk3|wk4=application/x-wk4|wkq=application/x-wkq|wks=application/x-wks|wm=video/x-ms-wm|wma=audio/x-ms-wma|wmd=application/x-ms-wmd|wmf=application/x-wmf|wml=text/vnd.wap.wml|wmv=video/x-ms-wmv|wmx=video/x-ms-wmx|wmz=application/x-ms-wmz|wp6=application/x-wp6|wpd=application/x-wpd|wpg=application/x-wpg|wpl=application/-wpl|wq1=application/x-wq1|wr1=application/x-wr1|wri=application/x-wri|wrk=application/x-wrk|ws=application/x-ws|ws2=application/x-ws|wsc=text/scriptlet|wsdl=text/xml|wvx=video/x-ms-wvx|xdp=application/vnd.adobe.xdp|xdr=text/xml|xfd=application/vnd.adobe.xfd|xfdf=application/vnd.adobe.xfdf|xhtml=text/html|xls=application/-excel|xls=application/x-xls|xlw=application/x-xlw|xml=text/xml|xpl=audio/scpls|xq=text/xml|xql=text/xml|xquery=text/xml|xsd=text/xml|xsl=text/xml|xslt=text/xml|xwd=application/x-xwd|x_b=application/x-x_b|x_t=application/x-x_t|docx=application/vnd.openxmlformats-officedocument.wordprocessingml.template|pptx=application/vnd.openxmlformats-officedocument.presentationml.presentation|ppsx=application/vnd.openxmlformats-officedocument.presentationml.slideshow|potx=application/vnd.openxmlformats-officedocument.presentationml.template|xlsx=application/vnd.openxmlformats-officedocument.spreadsheetml.sheet|xltx=application/vnd.openxmlformats-officedocument.spreadsheetml.template";
        #endregion
        static Dictionary<string, string> dicContentType = null;
        /// <summary>
        /// 根据扩展名返回相应的ContentType,未知扩展名返回 application/octet-stream
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static string GetContentType(string ext)
        {
            ext = ext + "";
            ext = ext.ToLower().Replace(".","");
            if (dicContentType == null)
            {
                var cts = AllContentType.Split('|');
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (var t in cts)
                {
                    var ts = t.Split('=');
                    if (ts == null || ts.Length != 2)
                    {
                        continue;
                    }
                    if (!dic.ContainsKey(ts[0]))
                    {
                        dic.Add(ts[0],ts[1]);
                    }
                }
                dicContentType = dic;
            }
            if (dicContentType.ContainsKey(ext))
            {
                return dicContentType[ext];
            }
            else
            {
                return "application/octet-stream";
            }
        }
    }
}
