	var person = new Person { FirstName = "Frank", LastName = "Stein", Age = 50 };
	Console.WriteLine(person.ToJson());


public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}