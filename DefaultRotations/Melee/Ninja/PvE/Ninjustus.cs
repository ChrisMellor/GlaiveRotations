using Dalamud.Logging;

namespace GlaiveRotations.Melee.Ninja.PvE;

public sealed partial class NinjaPvE
{
    private static void SetNinjutsu(INinAction act)
    {
        if (act == null || AdjustId(ActionID.Ninjutsu) == ActionID.RabbitMedium)
        {
            return;
        }

        if (_ninjustuAction != null && IsLastAction(false, Ten, Jin, Chi, FumaShurikenTen, FumaShurikenJin))
        {
            return;
        }

        if (_ninjustuAction != act)
        {
            _ninjustuAction = act;
        }
    }

    private static void ClearNinjutsu()
    {
        if (_ninjustuAction != null)
        {
            _ninjustuAction = null;
        }
    }

    private static bool ChoiceNinjutsu(out IAction act)
    {
        act = null;
        if (AdjustId(ActionID.Ninjutsu) != ActionID.Ninjutsu)
        {
            return false;
        }

        if (TimeSinceLastAction.TotalSeconds > 4.5)
        {
            ClearNinjutsu();
        }

        if (_ninjustuAction != null && WeaponRemain < 0.2)
        {
            return false;
        }

        //Kassatsu
        if (Player.HasStatus(true, StatusID.Kassatsu))
        {
            if (GokaMekkyaku.CanUse(out _))
            {
                SetNinjutsu(GokaMekkyaku);
                return false;
            }

            if (HyoshoRanryu.CanUse(out _))
            {
                SetNinjutsu(HyoshoRanryu);
                return false;
            }

            if (Katon.CanUse(out _))
            {
                SetNinjutsu(Katon);
                return false;
            }

            if (Raiton.CanUse(out _))
            {
                SetNinjutsu(Raiton);
                return false;
            }
        }
        else
        {
            //Buff
            if (Huraijin.CanUse(out act))
            {
                return true;
            }

            if (!HutonEndAfterGCD() && _ninjustuAction?.ID == Huton.ID)
            {
                ClearNinjutsu();
                return false;
            }

            if (Ten.CanUse(out _, CanUseOption.EmptyOrSkipCombo)
                && (!InCombat || !Huraijin.EnoughLevel)
                && Huton.CanUse(out _)
                && !IsLastAction(false, Huton))
            {
                SetNinjutsu(Huton);
                return false;
            }

            //Aoe
            if (Katon.CanUse(out _))
            {
                if (!Player.HasStatus(true, StatusID.Doton) && !IsMoving && !TenChiJin.WillHaveOneCharge(10))
                {
                    SetNinjutsu(Doton);
                }
                else
                {
                    SetNinjutsu(Katon);
                }

                return false;
            }

            //Vulnerable
            if (TrickAttack.WillHaveOneCharge(21) && Suiton.CanUse(out _))
            {
                SetNinjutsu(Suiton);
                return false;
            }

            //Single
            if (Ten.CanUse(out _, InTrickAttack
                                  && !Player.HasStatus(false, StatusID.RaijuReady)
                    ? CanUseOption.EmptyOrSkipCombo
                    : CanUseOption.None))
            {
                if (Raiton.CanUse(out _))
                {
                    SetNinjutsu(Raiton);
                    return false;
                }

                if (!Chi.EnoughLevel && FumaShuriken.CanUse(out _))
                {
                    SetNinjutsu(FumaShuriken);
                    return false;
                }
            }
        }

        if (IsLastAction(false, DotonChi, SuitonJin,
                RabbitMedium, FumaShuriken, Katon, Raiton,
                Hyoton, Huton, Doton, Suiton, GokaMekkyaku, HyoshoRanryu))
        {
            ClearNinjutsu();
        }

        return false;
    }

    private static bool DoNinjutsu(out IAction act)
    {
        act = null;

        //TenChiJin
        if (Player.HasStatus(true, StatusID.TenChiJin))
        {
            uint tenId = AdjustId(Ten.ID);
            uint chiId = AdjustId(Chi.ID);
            uint jinId = AdjustId(Jin.ID);

            //First
            if (tenId == FumaShurikenTen.ID
                && !IsLastAction(false, FumaShurikenJin, FumaShurikenTen))
            {
                //AOE
                if (Katon.CanUse(out _))
                {
                    if (FumaShurikenJin.CanUse(out act))
                    {
                        return true;
                    }
                }

                //Single
                if (FumaShurikenTen.CanUse(out act))
                {
                    return true;
                }
            }

            //Second
            else if (tenId == KatonTen.ID && !IsLastAction(false, KatonTen))
            {
                if (KatonTen.CanUse(out act, CanUseOption.MustUse))
                {
                    return true;
                }
            }
            //Others
            else if (chiId == RaitonChi.ID && !IsLastAction(false, RaitonChi))
            {
                if (RaitonChi.CanUse(out act, CanUseOption.MustUse))
                {
                    return true;
                }
            }
            else if (chiId == DotonChi.ID && !IsLastAction(false, DotonChi))
            {
                if (DotonChi.CanUse(out act, CanUseOption.MustUse))
                {
                    return true;
                }
            }
            else if (jinId == SuitonJin.ID && !IsLastAction(false, SuitonJin))
            {
                if (SuitonJin.CanUse(out act, CanUseOption.MustUse))
                {
                    return true;
                }
            }
        }

        //Keep Kassatsu in Burst.
        if (!Player.WillStatusEnd(3, true, StatusID.Kassatsu)
            && Player.HasStatus(true, StatusID.Kassatsu) && !InTrickAttack)
        {
            return false;
        }

        if (_ninjustuAction == null)
        {
            return false;
        }

        var id = AdjustId(ActionID.Ninjutsu);

        //Failed
        if ((uint)id == RabbitMedium.ID)
        {
            ClearNinjutsu();
            act = null;
            return false;
        }
        //First
        else if (id == ActionID.Ninjutsu && IsLastNinjustu(_ninjustuAction.Ninjutsu[0]))
        {
            //Can't use.
            if (!Player.HasStatus(true, StatusID.Kassatsu, StatusID.TenChiJin)
                && !Ten.CanUse(out _, CanUseOption.EmptyOrSkipCombo)
                && !IsLastAction(false, _ninjustuAction.Ninjutsu[0]))
            {
                return false;
            }

            act = _ninjustuAction.Ninjutsu[0];
            return true;
        }
        //Finished
        else if ((uint)id == _ninjustuAction.ID)
        {
            if (_ninjustuAction.CanUse(out act, CanUseOption.MustUse)) return true;
            if (_ninjustuAction.ID == Doton.ID && !InCombat)
            {
                act = _ninjustuAction;
                return true;
            }
        }
        //Second
        else if ((uint)id == FumaShuriken.ID && IsLastNinjustu(_ninjustuAction.Ninjutsu[1]))
        {
            if (_ninjustuAction.Ninjutsu.Length > 1
                && !IsLastAction(false, _ninjustuAction.Ninjutsu[1]))
            {
                act = _ninjustuAction.Ninjutsu[1];
                return true;
            }
        }
        //Third
        else if (((uint)id == Katon.ID || (uint)id == Raiton.ID || (uint)id == Hyoton.ID) && IsLastNinjustu(_ninjustuAction.Ninjutsu[2]))
        {
            if (_ninjustuAction.Ninjutsu.Length > 2
                && !IsLastAction(false, _ninjustuAction.Ninjutsu[2]))
            {
                act = _ninjustuAction.Ninjutsu[2];
                return true;
            }
        }

        return false;
    }

    private static bool IsLastNinjustu(IBaseAction action)
    {
        PluginLog.Information($"Action is: {action}");
        if (RecordActions != null && RecordActions.Any())
        {
            return RecordActions.First() != action;
        }

        return true;
    }
}