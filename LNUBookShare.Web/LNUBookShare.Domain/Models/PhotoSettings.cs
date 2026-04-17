namespace LNUBookShare.Domain.Models
{
    public class PhotoSettings
    {
        public long MaxFileSizeBytes { get; set; }
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    }
}