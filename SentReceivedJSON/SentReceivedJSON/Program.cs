using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    if (request.Path == "/api/user")
    {
        var responseText = "Некорректные данные";   // default message content

        if (request.HasJsonContentType())
        {
            // defining serialization/deserialization parameters
            var jsonoptions = new JsonSerializerOptions();
            // adding a json code converter to a Person type object
            jsonoptions.Converters.Add(new PersonConverter());
            // deserializing data using the PersonConverter
            var person = await request.ReadFromJsonAsync<Person>(jsonoptions);
            if (person != null)
                responseText = $"Name: {person.Name}  Age: {person.Age}";
        }
        await response.WriteAsJsonAsync(new { text = responseText });
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/index.html");
    }
});

app.Run();

public record Person(string Name, int Age);
public class PersonConverter : JsonConverter<Person>
{
    // deserialization json to Person
    // Utf8JsonReader - an object that reads data from json
    // Type - the type to convert
    // JsonSerializerOptions - additional serialization parameters
    public override Person Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var personName = "Undefined";
        var personAge = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();
                switch (propertyName?.ToLower())
                {
                    // if the property is age and it contains a number
                    case "age" when reader.TokenType == JsonTokenType.Number:
                        personAge = reader.GetInt32();  // reading a number from json
                        break;
                    // if the property age and it contains a string
                    case "age" when reader.TokenType == JsonTokenType.String:
                        string? stringValue = reader.GetString();
                        // trying to convert a string to a number
                        if (int.TryParse(stringValue, out int value))
                        {
                            personAge = value;
                        }
                        break;
                    case "name":    // if the property Name/name
                        string? name = reader.GetString();
                        if (name!=null)
                            personName = name;
                        break;
                }
            }
        }
        return new Person(personName, personAge);
    }
    // serialization Person to json
    // Utf8JsonWriter - an object that writes data to json
    // Person - the object to be serialized
    // JsonSerializerOptions - additional serialization parameters
    public override void Write(Utf8JsonWriter writer, Person person, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", person.Name);
        writer.WriteNumber("age", person.Age);

        writer.WriteEndObject();
    }
}