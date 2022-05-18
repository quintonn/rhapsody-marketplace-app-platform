using QBic.Core.Models;

namespace Marketplace.Models
{
    public class FileItem : DynamicClass
    {
        public virtual string FileName { get; set; }
        public virtual string FileExtension { get; set; }
        public virtual string MimeType { get; set; }
        public virtual byte[] FileData { get; set; }
        public virtual UserItem UserItem { get; set; }
    }
}
