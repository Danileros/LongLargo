using LongLargo.Models;

namespace LongLargo.Utils;

public class PackProximitySettingsLoader
{
    public List<PackProximitySettings> LoadAll()
    {
        var length = (int)Enum.GetValues<EPackProximityRange>().Max() + 1;
        var list = new List<PackProximitySettings>(length);
        
        list.Add(new PackProximitySettings(
            EPackProximityRange.Default,
            30,
            50,
            150));
        list.Add(new PackProximitySettings(
            EPackProximityRange.Short,
            25,
            20,
            150));
        list.Add(new PackProximitySettings(
            EPackProximityRange.VeryShort,
            20,
            10,
            150));
        list.Add(new PackProximitySettings(
            EPackProximityRange.Medium,
            30,
            35,
            150));
        list.Add(new PackProximitySettings(
            EPackProximityRange.Long,
            35,
            60,
            150));
        list.Add(new PackProximitySettings(
            EPackProximityRange.VeryLong,
            40,
            70,
            150));
        return list;
    }
}