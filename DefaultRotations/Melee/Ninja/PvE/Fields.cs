namespace GlaiveRotations.Melee.Ninja.PvE;

public sealed partial class NinjaPvE
{
    public override string GameVersion => "6.51";
    public override string RotationName => "Chris' Ninja Rotation";
    public override string Description => "My attempt at creating a rotation for Ninja. It's primarily aimed at raiding";
    public override CombatType Type => CombatType.PvE;
    private static INinAction _ninjustuAction;
    private static bool InTrickAttack => TrickAttack.IsCoolingDown && !TrickAttack.ElapsedAfter(17);
    private static bool InMug => Mug.IsCoolingDown && !Mug.ElapsedAfter(19);
    private static bool NoNinjutsu => AdjustId(ActionID.Ninjutsu) is ActionID.Ninjutsu or ActionID.RabbitMedium;
    private static bool _firstTrickAttack = true;
    private static bool IsBurstTime { get; set; } = false;
}