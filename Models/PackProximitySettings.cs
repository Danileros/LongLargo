namespace LongLargo.Models;

public class PackProximitySettings(
    EPackProximityRange ePackProximityRange,
    float timeNotInCombatBeforeFade,
    float distanceCombat,
    float distanceFadeInstant)
{
    public EPackProximityRange EPackProximityRange { get; set; } = ePackProximityRange;
    public float TimeNotInCombatBeforeFade { get; set; } = timeNotInCombatBeforeFade;
    public float DistanceCombat { get; set; } = distanceCombat;
    public float DistanceFadeInstant { get; set; } = distanceFadeInstant;
}