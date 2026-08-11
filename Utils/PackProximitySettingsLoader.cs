using LongLargo.Models;

namespace LongLargo.Utils;

public class PackProximitySettingsLoader
{
    public List<PackProximitySettings> LoadAll()
    {
        var length = (int)Enum.GetValues<PackProximityRange>().Max() + 1;
        var list = new List<PackProximitySettings>(length);
        
        list.Add(new PackProximitySettings(
            PackProximityRange.Default,
            60,
            50,
            150));
        list.Add(new PackProximitySettings(
            PackProximityRange.Short,
            40,
            20,
            150));
        list.Add(new PackProximitySettings(
            PackProximityRange.VeryShort,
            30,
            10,
            150));
        list.Add(new PackProximitySettings(
            PackProximityRange.Medium,
            50,
            35,
            150));
        list.Add(new PackProximitySettings(
            PackProximityRange.Long,
            60,
            60,
            150));
        list.Add(new PackProximitySettings(
            PackProximityRange.VeryLong,
            60,
            70,
            150));
        return list;
    }
}