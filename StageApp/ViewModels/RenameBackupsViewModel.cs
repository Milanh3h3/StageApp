namespace StageApp.ViewModels
{
    public class RenameBackupsViewModel
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public DateTime LastModified { get; set; }
        public long FileSize { get; set; } //bytes
    }
}
