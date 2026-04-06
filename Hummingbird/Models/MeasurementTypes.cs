namespace Hummingbird.Models;

public static class MeasurementTypes
{
    public const string UniqueReading = "Medición única";
    public const string PreBreakfast = "Pre desayuno";
    public const string PostBreakfast = "Post desayuno";
    public const string PreLunch = "Pre almuerzo";
    public const string PostLunch = "Post almuerzo";
    public const string PreDinner = "Pre cena";
    public const string PostDinner = "Post cena";
    public const string Night = "Noche";

    public static readonly List<string> All =
    [
        UniqueReading,
        PreBreakfast,
        PostBreakfast,
        PreLunch,
        PostLunch,
        PreDinner,
        PostDinner,
        Night
    ];

    public static bool IsPreprandial(string type) =>
        type is PreBreakfast or PreLunch or PreDinner;

    public static bool IsPostprandial(string type) =>
        type is PostBreakfast or PostLunch or PostDinner;

    public static string GetMealName(string type) => type switch
    {
        PreBreakfast or PostBreakfast => "Desayuno",
        PreLunch or PostLunch => "Almuerzo",
        PreDinner or PostDinner => "Cena",
        _ => "Otro"
    };
}
