using MediaServer.Models;
using MediaServer.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace MediaServer.Controllers
{
    [Route("gallery")]
    [ApiController]
    public class GalleryController : ControllerBase
    {
        private const string GALLERY_FOLDER = "Images";
        private IList<MetaDataModel> _metaDataList { get; set; }
        public GalleryController(VideoRepository videoRepository)
        {
            _metaDataList = Task.Run(videoRepository.GetMetaData).Result;
        }



        [HttpGet("{id}/{imageId}")]
        public IResult Get(int id, string imageId)
        {
            if (_metaDataList is null) return Results.NotFound();
            if(!isVideoExists(id)) return Results.NotFound();
            var video = GetVideoById(id);

            //string path = Path.Combine(video.Directory, $"{GALLERY_FOLDER}", $"{imageId}.png");
            string imageDirectory = ConstructPath(video.Directory, $"{GALLERY_FOLDER}");
            if(!IsPathExisit(imageDirectory)) return Results.NotFound();

            string imagePath = ConstructPath(imageDirectory, imageId);
            FileStream fileStream = TryOpenFile(imagePath);

            return Results.File(fileStream, "image/png");
        }


        private FileStream TryOpenFile(string fileName)
        {
            string extension = "png";
            string imageWithExtension = $"{fileName}.{extension}";
            try
            {
                return System.IO.File.OpenRead(imageWithExtension);
            }
            catch (FileNotFoundException)
            {
                var rootDirectory = @"D:\Filmek";
                var defaultImageCover = $"default_cover.{extension}";
                var path = Path.Combine(rootDirectory, defaultImageCover);
                return System.IO.File.OpenRead(path);
            }
        }

        private bool isVideoExists(int id)
        {
            if (id < 1 || id > _metaDataList.Count) return false;
            id -= 1;
            var video = _metaDataList[id];

            if (video is null) return false;
            if (video.Length is null) return false;
            if (video.Directory is null) return false;
            if (video.FileName is null) return false;

            return true;
        }

        private MetaDataModel GetVideoById(int id)
        {
            id -= 1;
            return _metaDataList[id];
        }

        private string ConstructPath(params string[] pathArguments)
        {
            return Path.Combine(pathArguments);
        }

        private bool IsPathExisit(string path)
        {
            return Path.Exists(path);
        }
    }
}
