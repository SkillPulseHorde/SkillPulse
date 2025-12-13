namespace Common.Shared.Auth.Constants;

public sealed record Role(string Name)
{
    public static implicit operator string(Role n) => n.Name;


    public static readonly Role Employee = new("Employee");

    public static readonly Role DepartmentManager = new("DepartmentManager");

    public static readonly Role ProductManager = new("ProductManager");

    public static readonly Role Hr = new("HR");
}