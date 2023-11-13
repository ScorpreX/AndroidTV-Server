using MediaServer.Mappers;
using MediaServer.Models;

namespace MediaServer.Repositories
{
    public class VideoRepository
    {
        private const string MetaDataFile = "VideoData.json";
        private readonly Lazy<Task<List<MetaDataModel>?>> _lazyMetaData;

        public VideoRepository()
        {
            _lazyMetaData = new Lazy<Task<List<MetaDataModel>?>>(() =>
               MyJsonSerializer.DeserializeVideoData<List<MetaDataModel>>($"{MetaDataFile}"));
        }


        public async Task<List<MetaDataModel>> GetMetaData()
        {
            var data = await _lazyMetaData.Value;
            if (data is null) data = new List<MetaDataModel>();

            return data;
        }
    }
}
