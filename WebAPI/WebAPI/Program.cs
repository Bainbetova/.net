using System.Text.RegularExpressions;

// initial data
List<Person> users = new List<Person>
{
    new() { Id = Guid.NewGuid().ToString(), Name = "Tom", Age = 37 },
    new() { Id = Guid.NewGuid().ToString(), Name = "Bob", Age = 41 },
    new() { Id = Guid.NewGuid().ToString(), Name = "Sam", Age = 24 }
};

var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;
    //string expressionForNumber = "^/api/users/([0 - 9]+)$";   // если id представляет число

    string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
    if (path == "/api/users" && request.Method=="GET")
    {
        await GetAllPeople(response);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
    {
        // get the id from the url
        string? id = path.Value?.Split("/")[3];
        await GetPerson(id, response);
    }
    else if (path == "/api/users" && request.Method == "POST")
    {
        await CreatePerson(response, request);
    }
    else if (path == "/api/users" && request.Method == "PUT")
    {
        await UpdatePerson(response, request);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split("/")[3];
        await DeletePerson(id, response);
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/index.html");
    }
});

app.Run();

// get all users
async Task GetAllPeople(HttpResponse response)
{
    await response.WriteAsJsonAsync(users);
}

// get a one user by id 
async Task GetPerson(string? id, HttpResponse response)
{
    // get a user by id
    Person? user = users.FirstOrDefault((u) => u.Id == id);
    // if user is found send him
    if (user != null)
        await response.WriteAsJsonAsync(user);
    // if not found, send a status code and an error message
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "User not found" });
    }
}

async Task DeletePerson(string? id, HttpResponse response)
{
    // get a user by id
    Person? user = users.FirstOrDefault((u) => u.Id == id);
    // if the user is found delete it
    if (user != null)
    {
        users.Remove(user);
        await response.WriteAsJsonAsync(user);
    }
    // if not found send a status code and an error message
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "User not found" });
    }
}

async Task CreatePerson(HttpResponse response, HttpRequest request)
{
    try
    {
        // get user data
        var user = await request.ReadFromJsonAsync<Person>();
        if (user != null)
        {
            // set id for the new user
            user.Id = Guid.NewGuid().ToString();
            // add a user to the list
            users.Add(user);
            await response.WriteAsJsonAsync(user);
        }
        else
        {
            throw new Exception("Incorrect data");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Incorrect data" });
    }
}

async Task UpdatePerson(HttpResponse response, HttpRequest request)
{
    try
    {
        // get user data
        Person? userData = await request.ReadFromJsonAsync<Person>();
        if (userData != null)
        {
            // get the user by id
            var user = users.FirstOrDefault(u => u.Id == userData.Id);
            // if the user is found, change his data and send it back to the client
            if (user != null)
            {
                user.Age = userData.Age;
                user.Name = userData.Name;
                await response.WriteAsJsonAsync(user);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "User not found" });
            }
        }
        else
        {
            throw new Exception("Incorrect data");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Incorrect data" });
    }
}
public class Person
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Age { get; set; }
}