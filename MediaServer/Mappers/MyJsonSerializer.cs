using MediaServer.DTOs;
using MediaServer.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MediaServer.Mappers
{
    public static class MyJsonSerializer
    {
        public static async Task SerializeVideoData<T>(string dataSource, T data)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            using (FileStream fs = new FileStream(dataSource, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(fs, data, jsonOptions);
            }
        }

        public static async Task<T?> DeserializeVideoData<T>(string dataSource) where T : class
        {
            if (!File.Exists(dataSource)) return null;

            try
            {
                using (FileStream fs = new FileStream(dataSource, FileMode.Open))
                {
                    return await JsonSerializer.DeserializeAsync<T>(fs);
                }
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static IList<MovieDto> AsMovieDto(this IList<MetaDataModel> videoDataModels)
        {
            int currentId = 0;
            return videoDataModels
           .Where(videoDataModel => videoDataModel is not null)
           .Select(videoDataModel => new MovieDto
           {
               Id = ++currentId,
               Title = NormalizeTitle(videoDataModel.FileName),
               Length = videoDataModel.Length,
               Extension = videoDataModel.Extension,
           })
           .ToList();
        }

        private static string NormalizeTitle(string title)
        {
            var parts = title.Split('.');
            var yearIndex = Array.FindIndex(parts, part => Regex.IsMatch(part, @"^\d{4}$"));

            if (yearIndex == -1)
                return string.Join(" ", parts).Replace('.', ' ');

            return string.Join(" ", parts.Take(yearIndex + 1));
        }
    }
}

