namespace GlaiveRotations.Melee;

[RotationDesc(ActionID.TrickAttack)]
[SourceCode(Path = "main/DefaultRotations/Melee/NinExperimental.cs")]
[LinkDescription("https://i.imgur.com/YIL6Egz.png")]
public class NinExperimental : NIN_Base
{
    public override string GameVersion => "6.51";
    public override string RotationName => "Experimental Definition";

    public override string Description => "Experimental rotation to play with the solver to see what I can make it do";

    #region PrePull
    private static int HutonStartTime => 8;
    private static int SuitonStartTime => 6;
    #endregion

    #region Resources
    private static bool NoNinjutsu => AdjustId(ActionID.Ninjutsu) is ActionID.Ninjutsu or ActionID.RabbitMedium;
    private static INinAction _ninActionAim;
    #endregion

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration()
            .SetBool("UseHide", true, "Use Hide")
            .SetBool("AutoUnhide", true, "Use Unhide");
    }

    #region Call To Definitions
    protected override IAction CountDownAction(float remainTime)
    {
        PrePull(remainTime);
        return base.CountDownAction(remainTime);
    }

    [RotationDesc(ActionID.ForkedRaiju)]
    protected override bool MoveForwardGCD(out IAction act)
    {
        if (ForkedRaiju.CanUse(out act))
        {
            return true;
        }

        if (Shukuchi.CanUse(out act))
        {
            return true;
        }

        return base.MoveForwardGCD(out act);
    }
    protected override bool GeneralGCD(out IAction act)
    {
        if (Burst(out act))
        {
            return true;
        };

        if (AvoidOverCap(out act))
        {
            return true;
        }

        if (HutonRefresher(out act))
        {
            return true;
        }

        if (FillerAttacks(out act))
        {
            return true;
        }

        return base.GeneralGCD(out act);

    }

    #endregion

    #region Rotations
    private void PrePull(float remainTime)
    {
        // TODO: Figure out what this actually is
        var realInHuton = !HutonEndAfterGCD() || IsLastAction(false, Huton);
        // Suiton Prep
        // Cast Suiton -0.8 seconds
    }

    private bool Burst(out IAction act)
    {
        if (!Player.HasStatus(true, StatusID.Suiton))
        {
            SetNinjutsu(Suiton);
            if (Suiton.CanUse(out act))
            {
                return true;
            }
        }

        if (!IsBurst)
        {
            act = null;
            return false;
        }

        if (Kassatsu.CanUse(out act))
        {
            return true;
        }

        if (AeolianEdge.CanUse(out act))
        {
            return true;
        }

        if (TrickAttack.CanUse(out act, CanUseOption.OnLastAbility))
        {
            return true;
        }

        if (PhantomKamaitachi.CanUse(out act))
        {
            return true;
        }

        if (Bunshin.CanUse(out act))
        {
            return true;
        }

        if (Mug.CanUse(out act))
        {
            return true;
        }

        if (GustSlash.CanUse(out act))
        {
            return true;
        }

        //if (UseBurstMedicine(out act))
        //{
        //    return true;
        //}

        if (SpinningEdge.CanUse(out act))
        {
            return true;
        }

        if (DreamWithinADream.CanUse(out act))
        {
            return true;
        }


        // Prep nin?
        if (HyoshoRanryu.CanUse(out act))
        {
            return true;
        }

        if (Raiton.CanUse(out act))
        {
            return true;
        }

        if (TenChiJin.CanUse(out act))
        {
            return true;
        }

        if (FumaShurikenTen.CanUse(out act))
        {
            return true;
        }

        if (RaitonChi.CanUse(out act))
        {
            return true;
        }

        if (SuitonJin.CanUse(out act))
        {
            return true;
        }

        if (Meisui.CanUse(out act))
        {
            return true;
        }

        if (FleetingRaiju.CanUse(out act))
        {
            return true;
        }

        if (Bhavacakra.CanUse(out act))
        {
            return true;
        }

        if (FleetingRaiju.CanUse(out act))
        {
            return true;
        }

        if (Bhavacakra.CanUse(out act))
        {
            return true;
        }

        if (Raiton.CanUse(out act))
        {
            return true;
        }

        if (FleetingRaiju.CanUse(out act))
        {
            return true;
        }

        return false;
    }
    private bool AvoidOverCap(out IAction act)
    {
        if (Bhavacakra.CanUse(out act))
        {
            return true;
        }

        return false;
    }
    private bool HutonRefresher(out IAction act)
    {
        if (HutonTime >= 27)
        {
            if (ArmorCrush.CanUse(out act))
            {
                return true;
            }

            return false;
        }

        act = null;
        return false;
    }
    private bool FillerAttacks(out IAction act)
    {
        if (AeolianEdge.CanUse(out act))
        {
            return true;
        }

        if (GustSlash.CanUse(out act))
        {
            return true;
        }

        if (SpinningEdge.CanUse(out act))
        {
            return true;
        }

        act = null;
        return false;
    }
    #endregion

    private static void ClearNinjutsu()
    {
        if (_ninActionAim != null)
        {
            _ninActionAim = null;
        }
    }

    private static void SetNinjutsu(INinAction act)
    {
        if (act == null || AdjustId(ActionID.Ninjutsu) == ActionID.RabbitMedium)
        {
            return;
        }

        if (_ninActionAim != null && IsLastAction(false, Ten, Jin, Chi, FumaShurikenTen, FumaShurikenJin))
        {
            return;
        }

        if (_ninActionAim != act)
        {
            _ninActionAim = act;
        }
    }

    public override void DisplayStatus()
    {
        ImGui.Text(_ninActionAim?.ToString() ?? "No Aimed Ninjustus.");
        base.DisplayStatus();
    }
}
