namespace LongLargo.Models;

public class PackProximitySettings(
    PackProximityRange packProximityRange,
    float timeNotInCombatBeforeFade,
    float distanceCombat,
    float distanceFadeInstant)
{
    public PackProximityRange PackProximityRange { get; set; } = packProximityRange;
    public float TimeNotInCombatBeforeFade { get; set; } = timeNotInCombatBeforeFade;
    public float DistanceCombat { get; set; } = distanceCombat;
    public float DistanceFadeInstant { get; set; } = distanceFadeInstant;
}