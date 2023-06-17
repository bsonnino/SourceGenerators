var person = new Person
{
    FirstName = "John",
    LastName = "Doe",
    Age = 20
};
Console.WriteLine(person.ToJson());

var person1 = PersonExtensions.FromJson("{ \"FirstName\": \"Mary\", \"LastName\": \"Jane\", \"Age\": 30}");
Console.WriteLine(person1.ToJson());

[Serializable]
public class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Age { get; set; }
}
