using Microsoft.AspNetCore.Mvc;
using MediaServer.Repositories;
using MediaServer.Models;
using MediaServer.DTOs;
using MediaServer.Mappers;
using System.IO;
using System.IO.Pipes;


namespace MediaServer.Controllers;

[ApiController]
[Route("videos")]
public class VideoController : ControllerBase
{
    private IList<MetaDataModel> _metaDataList { get; set; }

    public VideoController(VideoRepository videoRepository)
    {
        _metaDataList = Task.Run(videoRepository.GetMetaData).Result;
    }

    [HttpGet]
    public IList<MovieDto> GetAllMovies()
    {
        return _metaDataList.AsMovieDto();
    }

    [HttpGet("{id}")]
    public IResult Get(int id)
    {
        if (_metaDataList is null) return Results.NotFound();
        if (!isVideoExists(id)) return Results.NotFound();

        var video = GetVideoById(id);
        var mime = GetMimeType(video.Extension);
        string fileNameWithExtension = $"{video.FileName}.{video.Extension}";

        string videoPath = Path.Combine(video.Directory, fileNameWithExtension);

        if (!System.IO.File.Exists(videoPath)) return Results.NotFound();

        var filestream = System.IO.File.OpenRead(videoPath);
        return Results.File(filestream, contentType: mime, enableRangeProcessing: true, fileDownloadName: $"{fileNameWithExtension}");
    }


    [HttpGet("{id}/cover")]
    public IResult GetPoster(int id)
    {
        if (!isVideoExists(id)) return Results.NotFound();
        var video = GetVideoById(id);
        var posterImage = $"cover";

        FileStream fileStream = TryOpenFile(video.Directory, posterImage);

        return Results.File(fileStream, "image/png");
    }


    private MetaDataModel GetVideoById(int id)
    {
        id -= 1;
        return _metaDataList[id];
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


    private FileStream TryOpenFile(string directory, string fileName)
    {
        try
        {
            var path = ConstructFilePath(directory, fileName);
            return System.IO.File.OpenRead(path);
        }
        catch (FileNotFoundException)
        {
            var rootDirectory = @"D:\Filmek";
            var defaultImageCover = "default_cover";
            var path = ConstructFilePath(rootDirectory, defaultImageCover);
            return System.IO.File.OpenRead(path);
        }
    }


    private string ConstructFilePath(string directory, string fileName)
    {
        string extension = $"png";
        return Path.Combine(directory, $"{fileName}.{extension}");
    }

    private string GetMimeType(string extension)
    {
        string mime =
            _extensions.TryGetValue(extension,
            out var mimeType) ? mimeType : "application/octet-stream";

        return mime;
    }

    private Dictionary<string, string> _extensions = new()
        {
            {"mp4", "video/mp4" },
            {"mkv", "video/x-matroska" },
            {"avi", "video/x-msvideo" },
            {"mpeg", "video/mpeg" },
            {"webm", "video/webm" },
            {"flv", "video/x-flv" },
        };
}