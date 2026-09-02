using System.Globalization;

namespace LongLargo.Utils;

public static class PlaylistHelper
{
    public static string GetTrackName(string path)
    {
        return Path.GetFileNameWithoutExtension(path).ToLower(CultureInfo.CurrentCulture);
    }
}