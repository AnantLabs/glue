using System;
using System.Collections;

namespace Glue.Lib.Mime
{
	/// <summary>
	/// Summary description for MimeMapping.
	/// </summary>
    public class MimeMapping
    {
        private static Hashtable mimeTypes;

        static MimeMapping()
        {
            mimeTypes = new Hashtable(190);
            AddMimeMapping(".323", "text/h323");
            AddMimeMapping(".acx", "application/internet-property-stream");
            AddMimeMapping(".ai", "application/postscript");
            AddMimeMapping(".aif", "audio/x-aiff");
            AddMimeMapping(".aifc", "audio/aiff");
            AddMimeMapping(".aiff", "audio/aiff");
            AddMimeMapping(".asf", "video/x-ms-asf");
            AddMimeMapping(".asr", "video/x-ms-asf");
            AddMimeMapping(".asx", "video/x-ms-asf");
            AddMimeMapping(".au", "audio/basic");
            AddMimeMapping(".avi", "video/x-msvideo");
            AddMimeMapping(".axs", "application/olescript");
            AddMimeMapping(".bas", "text/plain");
            AddMimeMapping(".bcpio", "application/x-bcpio");
            AddMimeMapping(".bin", "application/octet-stream");
            AddMimeMapping(".bmp", "image/bmp");
            AddMimeMapping(".c", "text/plain");
            AddMimeMapping(".cat", "application/vndms-pkiseccat");
            AddMimeMapping(".cdf", "application/x-cdf");
            AddMimeMapping(".cer", "application/x-x509-ca-cert");
            AddMimeMapping(".clp", "application/x-msclip");
            AddMimeMapping(".cmx", "image/x-cmx");
            AddMimeMapping(".cod", "image/cis-cod");
            AddMimeMapping(".cpio", "application/x-cpio");
            AddMimeMapping(".crd", "application/x-mscardfile");
            AddMimeMapping(".crl", "application/pkix-crl");
            AddMimeMapping(".crt", "application/x-x509-ca-cert");
            AddMimeMapping(".csh", "application/x-csh");
            AddMimeMapping(".css", "text/css");
            AddMimeMapping(".dcr", "application/x-director");
            AddMimeMapping(".der", "application/x-x509-ca-cert");
            AddMimeMapping(".dib", "image/bmp");
            AddMimeMapping(".dir", "application/x-director");
            AddMimeMapping(".disco", "text/xml");
            AddMimeMapping(".dll", "application/x-msdownload");
            AddMimeMapping(".dot", "application/msword");
            AddMimeMapping(".doc", "application/msword");
            AddMimeMapping(".dvi", "application/x-dvi");
            AddMimeMapping(".dxr", "application/x-director");
            AddMimeMapping(".eml", "message/rfc822");
            AddMimeMapping(".eps", "application/postscript");
            AddMimeMapping(".etx", "text/x-setext");
            AddMimeMapping(".evy", "application/envoy");
            AddMimeMapping(".exe", "application/octet-stream");
            AddMimeMapping(".fif", "application/fractals");
            AddMimeMapping(".flr", "x-world/x-vrml");
            AddMimeMapping(".gif", "image/gif");
            AddMimeMapping(".gtar", "application/x-gtar");
            AddMimeMapping(".gz", "application/x-gzip");
            AddMimeMapping(".h", "text/plain");
            AddMimeMapping(".hdf", "application/x-hdf");
            AddMimeMapping(".hlp", "application/winhlp");
            AddMimeMapping(".hqx", "application/mac-binhex40");
            AddMimeMapping(".hta", "application/hta");
            AddMimeMapping(".htc", "text/x-component");
            AddMimeMapping(".htm", "text/html");
            AddMimeMapping(".html", "text/html");
            AddMimeMapping(".htt", "text/webviewhtml");
            AddMimeMapping(".ico", "image/x-icon");
            AddMimeMapping(".ief", "image/ief");
            AddMimeMapping(".iii", "application/x-iphone");
            AddMimeMapping(".ins", "application/x-internet-signup");
            AddMimeMapping(".isp", "application/x-internet-signup");
            AddMimeMapping(".ivf", "video/x-ivf");
            AddMimeMapping(".jfif", "image/pjpeg");
            AddMimeMapping(".jpe", "image/jpeg");
            AddMimeMapping(".jpeg", "image/jpeg");
            AddMimeMapping(".jpg", "image/jpeg");
            AddMimeMapping(".js", "application/x-javascript");
            AddMimeMapping(".latex", "application/x-latex");
            AddMimeMapping(".lsf", "video/x-la-asf");
            AddMimeMapping(".lsx", "video/x-la-asf");
            AddMimeMapping(".m13", "application/x-msmediaview");
            AddMimeMapping(".m14", "application/x-msmediaview");
            AddMimeMapping(".m1v", "video/mpeg");
            AddMimeMapping(".m3u", "audio/x-mpegurl");
            AddMimeMapping(".man", "application/x-troff-man");
            AddMimeMapping(".mdb", "application/x-msaccess");
            AddMimeMapping(".me", "application/x-troff-me");
            AddMimeMapping(".mht", "message/rfc822");
            AddMimeMapping(".mhtml", "message/rfc822");
            AddMimeMapping(".mid", "audio/mid");
            AddMimeMapping(".mny", "application/x-msmoney");
            AddMimeMapping(".mov", "video/quicktime");
            AddMimeMapping(".movie", "video/x-sgi-movie");
            AddMimeMapping(".mp2", "video/mpeg");
            AddMimeMapping(".mp3", "audio/mpeg");
            AddMimeMapping(".mpa", "video/mpeg");
            AddMimeMapping(".mpe", "video/mpeg");
            AddMimeMapping(".mpeg", "video/mpeg");
            AddMimeMapping(".mpg", "video/mpeg");
            AddMimeMapping(".mpp", "application/vnd.ms-project");
            AddMimeMapping(".mpv2", "video/mpeg");
            AddMimeMapping(".ms", "application/x-troff-ms");
            AddMimeMapping(".mvb", "application/x-msmediaview");
            AddMimeMapping(".nc", "application/x-netcdf");
            AddMimeMapping(".nws", "message/rfc822");
            AddMimeMapping(".oda", "application/oda");
            AddMimeMapping(".ods", "application/oleobject");
            AddMimeMapping(".p10", "application/pkcs10");
            AddMimeMapping(".p12", "application/x-pkcs12");
            AddMimeMapping(".p7b", "application/x-pkcs7-certificates");
            AddMimeMapping(".p7c", "application/pkcs7-mime");
            AddMimeMapping(".p7m", "application/pkcs7-mime");
            AddMimeMapping(".p7r", "application/x-pkcs7-certreqresp");
            AddMimeMapping(".p7s", "application/pkcs7-signature");
            AddMimeMapping(".pbm", "image/x-portable-bitmap");
            AddMimeMapping(".pdf", "application/pdf");
            AddMimeMapping(".pfx", "application/x-pkcs12");
            AddMimeMapping(".pgm", "image/x-portable-graymap");
            AddMimeMapping(".pko", "application/vndms-pkipko");
            AddMimeMapping(".pma", "application/x-perfmon");
            AddMimeMapping(".pmc", "application/x-perfmon");
            AddMimeMapping(".pml", "application/x-perfmon");
            AddMimeMapping(".pmr", "application/x-perfmon");
            AddMimeMapping(".pmw", "application/x-perfmon");
            AddMimeMapping(".png", "image/png");
            AddMimeMapping(".pnm", "image/x-portable-anymap");
            AddMimeMapping(".pot", "application/vnd.ms-powerpoint");
            AddMimeMapping(".ppm", "image/x-portable-pixmap");
            AddMimeMapping(".pps", "application/vnd.ms-powerpoint");
            AddMimeMapping(".ppt", "application/vnd.ms-powerpoint");
            AddMimeMapping(".prf", "application/pics-rules");
            AddMimeMapping(".ps", "application/postscript");
            AddMimeMapping(".pub", "application/x-mspublisher");
            AddMimeMapping(".qt", "video/quicktime");
            AddMimeMapping(".ra", "audio/x-pn-realaudio");
            AddMimeMapping(".ram", "audio/x-pn-realaudio");
            AddMimeMapping(".ras", "image/x-cmu-raster");
            AddMimeMapping(".rgb", "image/x-rgb");
            AddMimeMapping(".rmi", "audio/mid");
            AddMimeMapping(".roff", "application/x-troff");
            AddMimeMapping(".rtf", "application/rtf");
            AddMimeMapping(".rtx", "text/richtext");
            AddMimeMapping(".scd", "application/x-msschedule");
            AddMimeMapping(".sct", "text/scriptlet");
            AddMimeMapping(".setpay", "application/set-payment-initiation");
            AddMimeMapping(".setreg", "application/set-registration-initiation");
            AddMimeMapping(".sh", "application/x-sh");
            AddMimeMapping(".shar", "application/x-shar");
            AddMimeMapping(".sit", "application/x-stuffit");
            AddMimeMapping(".snd", "audio/basic");
            AddMimeMapping(".spc", "application/x-pkcs7-certificates");
            AddMimeMapping(".spl", "application/futuresplash");
            AddMimeMapping(".src", "application/x-wais-source");
            AddMimeMapping(".sst", "application/vndms-pkicertstore");
            AddMimeMapping(".stl", "application/vndms-pkistl");
            AddMimeMapping(".stm", "text/html");
            AddMimeMapping(".sv4cpio", "application/x-sv4cpio");
            AddMimeMapping(".sv4crc", "application/x-sv4crc");
            AddMimeMapping(".swf", "application/x-shockwave-flash");
            AddMimeMapping(".t", "application/x-troff");
            AddMimeMapping(".tar", "application/x-tar");
            AddMimeMapping(".tcl", "application/x-tcl");
            AddMimeMapping(".tex", "application/x-tex");
            AddMimeMapping(".texi", "application/x-texinfo");
            AddMimeMapping(".texinfo", "application/x-texinfo");
            AddMimeMapping(".tgz", "application/x-compressed");
            AddMimeMapping(".tif", "image/tiff");
            AddMimeMapping(".tiff", "image/tiff");
            AddMimeMapping(".tr", "application/x-troff");
            AddMimeMapping(".trm", "application/x-msterminal");
            AddMimeMapping(".tsv", "text/tab-separated-values");
            AddMimeMapping(".txt", "text/plain");
            AddMimeMapping(".text", "text/plain");
            AddMimeMapping(".uls", "text/iuls");
            AddMimeMapping(".ustar", "application/x-ustar");
            AddMimeMapping(".vcf", "text/x-vcard");
            AddMimeMapping(".wav", "audio/wav");
            AddMimeMapping(".wcm", "application/vnd.ms-works");
            AddMimeMapping(".wdb", "application/vnd.ms-works");
            AddMimeMapping(".wks", "application/vnd.ms-works");
            AddMimeMapping(".wmf", "application/x-msmetafile");
            AddMimeMapping(".wps", "application/vnd.ms-works");
            AddMimeMapping(".wri", "application/x-mswrite");
            AddMimeMapping(".wrl", "x-world/x-vrml");
            AddMimeMapping(".wrz", "x-world/x-vrml");
            AddMimeMapping(".wsdl", "text/xml");
            AddMimeMapping(".xaf", "x-world/x-vrml");
            AddMimeMapping(".xbm", "image/x-xbitmap");
            AddMimeMapping(".xla", "application/vnd.ms-excel");
            AddMimeMapping(".xlc", "application/vnd.ms-excel");
            AddMimeMapping(".xlm", "application/vnd.ms-excel");
            AddMimeMapping(".xls", "application/vnd.ms-excel");
            AddMimeMapping(".xlt", "application/vnd.ms-excel");
            AddMimeMapping(".xlw", "application/vnd.ms-excel");
            AddMimeMapping(".xml", "text/xml");
            AddMimeMapping(".xof", "x-world/x-vrml");
            AddMimeMapping(".xpm", "image/x-xpixmap");
            AddMimeMapping(".xsd", "text/xml");
            AddMimeMapping(".xsl", "text/xml");
            AddMimeMapping(".xwd", "image/x-xwindowdump");
            AddMimeMapping(".z", "application/x-compress");
            AddMimeMapping(".zip", "application/x-zip-compressed");
            AddMimeMapping(".*", "application/octet-stream");	
        }

        private static void AddMimeMapping(string extension, string mimeType)
        {
            mimeTypes.Add(extension, mimeType);
        }

        public static string GetMimeMapping(string fileName)
        {
            int dot = fileName.LastIndexOf ('.');
            if (dot >= 0)
                return (string)mimeTypes[(string)fileName.Substring(dot).ToLower()];
            else
                return (string)mimeTypes[".*"];
        }

        public static string GetExtension(string mimeType)
        {
            int i = mimeType.IndexOf(';');
            if (i > 0)
                mimeType = mimeType.Substring(0, i).Trim();
            foreach (DictionaryEntry e in mimeTypes)
                if (string.Compare(mimeType, (string)e.Value, true) == 0)
                    return (string)e.Key;
            return null;
        }
    }
}
