using MediaServer.Hubs;
using MediaServer.Repositories;
using MediaServer.Services;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddHostedService<MetaDataInitializerService>();
//builder.Services.AddSingleton<MetaDataInitializerService>();
builder.Services.AddSingleton<VideoRepository>();


builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.MapControllers();
app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAuthorization();
//app.UseCookiePolicy();
app.MapHub<MessagingHub>("/message");
app.Run();