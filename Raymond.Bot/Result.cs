namespace Raymond.Bot;

public class Result<T>
{
    public bool Succeeded { get; set; }
    public T? Body { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public static Result<T> Failed(string message)
    {
        return new Result<T>
        {
            Succeeded = false,
            Message = message
        };
    }
    
    public static Result<T> Success(T body, string message = "")
    {
        return new Result<T>
        {
            Succeeded = true,
            Body = body,
            Message = message
        };
    }
}