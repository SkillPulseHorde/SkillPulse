namespace Common.Shared.Auth.Constants;

public static class Roles
{
    private const string Employee = "Employee";

    private const string DepartmentManager = "DepartmentManager";

    private const string ProductManager = "ProductManager";

    private const string Hr = "HR";

    /// <summary>
    /// Hr роль
    /// </summary>
    public static readonly string[] OnlyHr =
    [
        Hr
    ];

    /// <summary>
    /// Обе роли руководителей
    /// </summary>
    public static readonly string[] AllManagers =
    [
        DepartmentManager,
        ProductManager
    ];

    /// <summary>
    /// Все роли с управленческими правами
    /// </summary>
    public static readonly string[] AllManagersAndHr =
    [
        ..AllManagers,
        Hr
    ];

    /// <summary>
    /// Все доступные роли
    /// </summary>
    public static readonly string[] AllRoles =
    [
        Employee,
        ..AllManagersAndHr
    ];
}