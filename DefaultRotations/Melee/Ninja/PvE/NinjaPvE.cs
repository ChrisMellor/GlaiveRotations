using System.Diagnostics;
using System.Linq;

namespace GlaiveRotations.Melee.Ninja.PvE;

[RotationDesc(ActionID.Mug)]
public sealed partial class NinjaPvE : NIN_Base
{
    private static readonly Stopwatch StopWatch = new Stopwatch();

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

        var realInHuton = !HutonEndAfterGCD() || IsLastAction(false, Huton);
        if (realInHuton && _ninjustuAction == Huton)
        {
            ClearNinjutsu();
        }

        if (DoNinjutsu(out var act))
        {
            if (act == Suiton && remainTime >= CountDownAhead)
            {
                return null;
            }

            return act;
        }

        if (remainTime <= 6.5)
        {
            SetNinjutsu(Suiton);
        }
        else if (remainTime <= 10)
        {
            if (_ninjustuAction == null && Ten.IsCoolingDown && Hide.CanUse(out act))
            {
                return act;
            }

            if (!realInHuton)
            {
                SetNinjutsu(Huton);
            }
        }
        StopWatch.Reset();
        return base.CountDownAction(remainTime);
    }

    protected override bool GeneralGCD(out IAction act)
    {
        StopWatch.Start();
        var hasRaijuReady = Player.HasStatus(true, StatusID.RaijuReady);

        var evenTrick = !Bunshin.HasOneCharge &&
                        !InTrickAttack &&
                        InMug;

        var generalRules = NoNinjutsu &&
                           !hasRaijuReady &&
                           !Ten.CanUse(out _);

        var noDamageIncrease = TrickAttack.ElapsedAfter(2) &&
                               Mug.ElapsedAfter(2) && HutonTime <= 50;

        var isNotUsingNinjustu = Player.HasStatus(true, StatusID.PhantomKamaitachiReady) &&
                                 !Player.HasStatus(true, StatusID.Ninjutsu);

        var isCastingNinjustu = Player.HasStatus(true, StatusID.Ninjutsu);
        var isInMug = Mug.ElapsedAfter(0) &&
                      !Mug.ElapsedAfter(20);
        var isInTrickAttack = TrickAttack.ElapsedAfter(0) &&
                              !TrickAttack.ElapsedAfter(15);
        var needToRefreshHuton = HutonTime <= 50;

        if ((generalRules && evenTrick || noDamageIncrease) && isNotUsingNinjustu && PhantomKamaitachi.CanUse(out act))
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
        if (!Player.HasStatus(true, StatusID.Ninjutsu) && !Player.HasStatus(false, StatusID.Ninjutsu))
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

            if (TrickAttack.IsCoolingDown && !TrickAttack.WillHaveOneCharge(19))
            {
                if (Meisui.CanUse(out act))
                {
                    return true;
                }
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

        if (!CombatElapsedLess(5))
        {
            if (Bunshin.CanUse(out act))
            {
                return true;
            }
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

        if (!IsMoving && RecordActions.FirstOrDefault()?.Action.RowId != DreamWithinADream.ID && InTrickAttack)
        {
            if ((RecordActions.FirstOrDefault()?.Action.RowId == Raiton.ID ||
                 RecordActions.FirstOrDefault()?.Action.RowId == ForkedRaiju.ID ||
                 RecordActions.FirstOrDefault()?.Action.RowId == FleetingRaiju.ID) &&
                TenChiJin.CanUse(out act, CanUseOption.IgnoreClippingCheck))
            {
                return true;
            }
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