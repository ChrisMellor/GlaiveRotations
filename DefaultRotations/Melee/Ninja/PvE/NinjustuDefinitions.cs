namespace GlaiveRotations.Melee.Ninja.PvE;

public sealed partial class NinjaPvE
{
    public static IBaseAction Ten2 { get; } = new BaseAction(AdjustId(ActionID.Ten), ActionOption.Friendly);
    public static IBaseAction Chi2 { get; } = new BaseAction(AdjustId(ActionID.Chi), ActionOption.Friendly);
    public static IBaseAction Jin2 { get; } = new BaseAction(AdjustId(ActionID.Jin), ActionOption.Friendly);
    public static INinAction FumaShuriken2 { get; }
    public static INinAction Katon2 { get; }
    public static INinAction Raiton2 { get; }
    public static INinAction Huton2 { get; }
    public static INinAction Doton2 { get; }
    public static INinAction Suiton2 { get; }
    public static INinAction GokaMekkyaku2 { get; }
    public static INinAction HyoshoRanryu2 { get; }

    static NinjaPvE()
    {
        Huton2 = new NinAction2(ActionID.Huton, Jin2, Chi2, Ten2)
        {
            ActionCheck = (_, _) => HutonEndAfterGCD()
        };

        Doton2 = new NinAction2(ActionID.Doton, Jin2, Ten2, Chi2)
        {
            StatusProvide = new[]
            {
                StatusID.Doton
            }
        };

        Suiton2 = new NinAction2(ActionID.Suiton, Ten2, Chi2, Jin2)
        {
            StatusProvide = new[]
            {
                StatusID.Suiton
            }
        };

        GokaMekkyaku2 = new NinAction2(ActionID.GokaMekkyaku, Chi2, Ten2);
        HyoshoRanryu2 = new NinAction2(ActionID.HyoshoRanryu, Ten2, Jin2);
        Raiton2 = new NinAction2(ActionID.Raiton, Ten2, Chi2);
        Katon2 = new NinAction2(ActionID.Katon, Chi2, Ten2);
        FumaShuriken2 = new NinAction2(ActionID.FumaShuriken, Ten2);
    }
}