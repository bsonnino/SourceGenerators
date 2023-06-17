using System.Text;

public static class JsonExtensions
{
    public static string ToJson<T>(this T element)
    {
        var properties = typeof(T).GetProperties();
        var builder = new StringBuilder();
        builder.Append("{");
        foreach (var property in properties)
        {
            var name = property.Name;
            var value = property.GetValue(element);
            builder.Append($"\"{name}\":\"{value}\",");
        }
        builder.Remove(builder.Length - 1, 1); // Remove trailing comma
        builder.Append("}");
        return builder.ToString();
    }
}