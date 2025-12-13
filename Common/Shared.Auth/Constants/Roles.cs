namespace Common.Shared.Auth.Constants;

public static class Roles
{
    /// <summary>
    /// Hr роль
    /// </summary>
    public static readonly string[] OnlyHr =
    [
        Role.Hr
    ];

    /// <summary>
    /// Обе роли руководителей
    /// </summary>
    public static readonly string[] AllManagers =
    [
        Role.DepartmentManager,
        Role.ProductManager
    ];

    /// <summary>
    /// Все роли с управленческими правами
    /// </summary>
    public static readonly string[] AllManagersAndHr =
    [
        ..AllManagers,
        Role.Hr
    ];

    /// <summary>
    /// Все доступные роли
    /// </summary>
    public static readonly string[] AllRoles =
    [
        Role.Employee,
        ..AllManagersAndHr
    ];
}