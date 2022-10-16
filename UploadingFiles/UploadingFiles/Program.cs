var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;

    response.ContentType = "text/html; charset=utf-8";

    if (request.Path == "/upload" && request.Method=="POST")
    {
        // getting a collection of uploaded files
        IFormFileCollection files = request.Form.Files;
        // path to the folder where the files will be stored
        var uploadPath = $"{Directory.GetCurrentDirectory()}/uploads";
        // creating a folder for storing files
        Directory.CreateDirectory(uploadPath);

        foreach (var file in files)
        {
            // folder path "uploads"
            string fullPath = $"{uploadPath}/{file.FileName}";

            // save the file to a folder "uploads"
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
        }
        await response.WriteAsync("Files uploaded successfully");
    }
    else
    {
        await response.SendFileAsync("html/index.html");
    }
});

app.Run();