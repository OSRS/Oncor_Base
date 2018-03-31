using Osrs.Net.Http;

namespace Pnnl.Oncor.Rest.FileTransfer
{
    internal static class MetaInfo
    {
        internal static bool SupportedUploadType(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.ToLowerInvariant().Trim();
                if (!string.IsNullOrEmpty(fileName))
                {
                    return fileName.EndsWith(".xlsx") || fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg");
                }
            }
            return false;
        }

        internal static string GetFileName(HttpContext context)
        {
            string fName = null;
            if (context.Request.Headers.ContainsKey("file-name"))
                fName = context.Request.Headers["file-name"];
            if (fName != null)
                fName = fName.Trim();

            if (fName != null && fName.Length > 0)
                return fName;

            return null;
        }
    }
}
