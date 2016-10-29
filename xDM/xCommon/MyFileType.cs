using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon
{
    public class MyFileType
    {
        static string types = "image:.jpg,.jpeg,.gif,.png,.bmp,.ico,.pcx,.tiff"
            + "|video:.mov,.mp4,.avi,.mpeg,.3gp,.mpg,.rm,.mpe,.wmv,.vob,.mkv,.rmvb,.asf,.divx"
            + "|audio:.mp3,.wav,.mod,.mid,.st3,.xt,.s3m,.far,.ra,mp1,.mp2,.aac,.m4a,.m4r"
            + "|zip:.zip,.rar,.cab,.arj,.lzh,.ace,.7z,.tar,.gzip,.gz,.gzi,.uue,.bz2,.jar,.z,.zz,.ha,.hbc,.hbe,.hbc2"
            + "|key:.p7b,.p7c,.spc,.p12,.pfx,.der,.cer,.crt,.pem"
            + "|text:.txt,.inf,.ini,.xml";

        static Dictionary<string, string> listtype = null;
        public static string GetFileType(string extension)
        {
            if (listtype == null)
            {
                listtype = new Dictionary<string, string>();
                var ts = types.Split('|');
                foreach (var t in ts)
                {
                    var ps = t.Split(':');
                    if (ps.Length == 2)
                    {
                        var type = ps[0];
                        var es = ps[1].Split(',');
                        foreach (var item in es)
                        {
                            if (!listtype.ContainsKey(item))
                            {
                                listtype.Add(item, type);
                            }
                        }
                    }
                }
            }
            if (listtype.ContainsKey(extension?.ToLower()))
            {
                return listtype[extension?.ToLower()];
            }
            return "other";
        }
        public enum FileType
        {
            image,
            video,
            audio,
            zip,
            other
        }
    }
}
