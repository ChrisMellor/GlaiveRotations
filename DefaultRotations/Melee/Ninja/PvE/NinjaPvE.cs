namespace GlaiveRotations.Melee.Ninja.PvE;

[RotationDesc(ActionID.Mug)]
public sealed partial class NinjaPvE : NIN_Base
{
    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration()
            .SetBool(CombatType.PvE, "UseHide", true, "Use Hide")
            .SetBool(CombatType.PvE, "AutoUnhide", true, "Use Unhide");
    }

    protected override IAction CountDownAction(float remainTime)
    {
        if (remainTime > 10)
        {
            _firstTrickAttack = true;
            ClearNinjutsu();
        }

        var realInHuton = !HutonEndAfterGCD() || IsLastAction(false, Huton2);
        if (realInHuton && _ninjustuAction == Huton2)
        {
            ClearNinjutsu();
        }

        if (DoNinjutsu(out var act))
        {
            if (act == Suiton2 && remainTime > CountDownAhead)
            {
                return null;
            }

            return act;
        }

        if (remainTime <= 6)
        {
            SetNinjutsu(Suiton2);
        }
        else if (remainTime <= 10)
        {
            if (_ninjustuAction == null && Ten2.IsCoolingDown && Hide.CanUse(out act))
            {
                return act;
            }

            if (!realInHuton)
            {
                SetNinjutsu(Huton2);
            }
        }

        return base.CountDownAction(remainTime);
    }

    protected override bool GeneralGCD(out IAction act)
    {
        var hasRaijuReady = Player.HasStatus(true, StatusID.RaijuReady);

        var evenTrick = !Bunshin.HasOneCharge && !InTrickAttack && InMug;
        var generalRules = NoNinjutsu && !hasRaijuReady && !Ten2.CanUse(out _);
        var oddTrick = !Bunshin.HasOneCharge && InTrickAttack && !InMug;
        var noDamageIncrease = TrickAttack.ElapsedAfter(2) && Mug.ElapsedAfter(2) && HutonTime <= 50;

        var isNotUsingNinjustu = Player.HasStatus(true, StatusID.PhantomKamaitachiReady) && !Player.HasStatus(true, StatusID.Ninjutsu);
        if ((generalRules && (evenTrick || oddTrick) || noDamageIncrease) && isNotUsingNinjustu && PhantomKamaitachi.CanUse(out act))
        {
            return true;
        }

        if (ChoiceNinjutsu(out act))
        {
            return true;
        }

        if ((!InCombat || !CombatElapsedLess(9)) && DoNinjutsu(out act))
        {
            return true;
        }

        //No Ninjutsu
        if (NoNinjutsu)
        {
            if (!CombatElapsedLess(10) && FleetingRaiju.CanUse(out act) && !TenChiJin.WillHaveOneCharge(1))
            {
                return true;
            }

            if (hasRaijuReady)
            {
                return false;
            }

            if (Huraijin.CanUse(out act))
            {
                return true;
            }

            //AOE
            if (HakkeMujinsatsu.CanUse(out act))
            {
                return true;
            }

            if (DeathBlossom.CanUse(out act))
            {
                return true;
            }

            //Single
            if (!InTrickAttack && ArmorCrush.CanUse(out act))
            {
                return true;
            }

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

            //Range
            if (IsMoveForward && MoveForwardAbility(out act))
            {
                return true;
            }

            if (ThrowingDagger.CanUse(out act))
            {
                return true;
            }
        }

        if (Configs.GetBool("AutoUnhide"))
        {
            StatusHelper.StatusOff(StatusID.Hidden);
        }

        if (!InCombat && _ninjustuAction == null && Configs.GetBool("UseHide")
            && Ten.IsCoolingDown && Hide.CanUse(out act))
        {
            return true;
        }

        return base.GeneralGCD(out act);
    }

    [RotationDesc(ActionID.ForkedRaiju)]
    protected override bool MoveForwardGCD(out IAction act)
    {
        if (ForkedRaiju.CanUse(out act))
        {
            return true;
        }

        return base.MoveForwardGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        if (!NoNinjutsu || !InCombat)
        {
            return base.EmergencyAbility(nextGCD, out act);
        }

        if (TrickAttack.WillHaveOneCharge(8) && Kassatsu.CanUse(out act))
        {
            return true;
        }

        if (UseBurstMedicine(out act))
        {
            return true;
        }

        if (!CombatElapsedLess(4) && TrickAttack.WillHaveOneCharge(3) && Mug.CanUse(out act))
        {
            IsBurstTime = true;
            return true;
        }

        //Use Suiton
        if (!CombatElapsedLess(6))
        {
            if (_firstTrickAttack)
            {
                if (Mug.ElapsedAfter(1f) && TrickAttack.CanUse(out act, CanUseOption.OnLastAbility))
                {
                    _firstTrickAttack = false;
                    IsBurstTime = true;
                    return true;
                }
            }
            else
            {
                if (TrickAttack.CanUse(out act))
                {
                    IsBurstTime = true;
                    return true;
                }
            }

            if (TrickAttack.IsCoolingDown && !TrickAttack.WillHaveOneCharge(19)
                                          && Meisui.CanUse(out act))
            {
                return true;
            }
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction act)
    {
        IsBurstTime = HostileTargets.Any(x => x.HasStatus(true, StatusID.TrickAttack));

        act = null;
        if (!NoNinjutsu || !InCombat)
        {
            return false;
        }

        if (!CombatElapsedLess(5) && Bunshin.CanUse(out act))
        {
            return true;
        }

        if (InTrickAttack)
        {
            if (DreamWithinADream.CanUse(out act))
            {
                return true;
            }

            if (!DreamWithinADream.EnoughLevel)
            {
                if (Assassinate.CanUse(out act))
                {
                    return true;
                }
            }
        }

        if (!IsMoving && InTrickAttack && !Ten.ElapsedAfter(30) && TenChiJin.CanUse(out act))
        {
            return true;
        }

        if ((!InMug || InTrickAttack)
            && (!Bunshin.WillHaveOneCharge(10) || Player.HasStatus(true, StatusID.PhantomKamaitachiReady) || Mug.WillHaveOneCharge(2)))
        {
            if (HellfrogMedium.CanUse(out act))
            {
                return true;
            }

            if (Bhavacakra.CanUse(out act))
            {
                return true;
            }
        }

        return base.AttackAbility(out act);
    }
}