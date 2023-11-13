using MediaInfo;
using MediaServer.Mappers;
using MediaServer.Models;

namespace MediaServer.Services
{
    public class MetaDataInitializerService : IHostedService
    {
        private const string DirectoryPath = @"D:\Filmek";
        private const string VideoDataFileName = "VideoData";

        private readonly MediaInfo.MediaInfo _mediaInfo;
        private readonly HashSet<string> _supportedExtensions;

        public MetaDataInitializerService()
        {
            _mediaInfo = new MediaInfo.MediaInfo();
            _supportedExtensions = new HashSet<string> { "webm", "avi", "mp4", "mkv", "flv" };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            await InitializeDataInfo($"{VideoDataFileName}.json");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task InitializeDataInfo(string outputPath)
        {
            var videos = await GenerateMetaData();
            await MyJsonSerializer.SerializeVideoData(outputPath, videos);
        }

        private async Task<IList<MetaDataModel>> GenerateMetaData()
        {
            var files = await GetFilesFromDirectoryAsync();
            var videos = new List<MetaDataModel>();

            foreach (var file in files)
            {
                _mediaInfo.Open(file);

                if (!IsSupportedExtension(GetParameter("FileExtension"))) continue;
                if (!IsTitleAllowed(GetParameter("FileName"))) continue;

                videos.Add(CreateVideoModelFromMediaInfo());
            }

            return videos;
        }

        private async Task<IEnumerable<string>> GetFilesFromDirectoryAsync()
        {
            return await Task.Run(() =>
            {
                var allFiles = Directory.EnumerateFiles(DirectoryPath, "*.*", SearchOption.AllDirectories);
                var filteredFiles = new List<string>();

                foreach (var file in allFiles)
                {
                    var directory = Path.GetDirectoryName(file);
                    if (!directory.Contains("sample", StringComparison.OrdinalIgnoreCase))
                    {
                        filteredFiles.Add(file);
                    }
                }

                return filteredFiles;
            });
        }

        private MetaDataModel CreateVideoModelFromMediaInfo()
        {
            return new MetaDataModel
            {
                FileName = GetParameter("FileName"),
                Length = GetParameter("Duration"),
                Extension = GetParameter("FileExtension"),
                Directory = GetParameter("FolderName")
            };
        }

        private string GetParameter(string parameter)
        {
            return _mediaInfo.Get(StreamKind.General, 0, parameter);
        }

        private bool IsSupportedExtension(string extension)
        {
            return _supportedExtensions.Contains(extension);
        }

        private bool IsTitleAllowed(string title)
        {
            var parts = title.Split('.');
            var filteredWords = new List<string> { "Sample" };
            return !parts.Any(part => filteredWords.Any(fw => part.IndexOf(fw, StringComparison.OrdinalIgnoreCase) != -1));
        }
    }
}
