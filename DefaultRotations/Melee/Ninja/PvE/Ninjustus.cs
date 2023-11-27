using Dalamud.Logging;
using System;
using System.Linq;

namespace GlaiveRotations.Melee.Ninja.PvE;

public sealed partial class NinjaPvE
{
    private static void SetNinjutsu(INinAction act)
    {
        if (act == null || AdjustId(ActionID.Ninjutsu) == ActionID.RabbitMedium)
        {
            return;
        }

        if (_ninjustuAction != null &&
            IsLastAction(false, Ten, Jin, Chi, FumaShurikenTen, FumaShurikenJin))
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

            //if (Ten.CanUse(out _, CanUseOption.EmptyOrSkipCombo)
            //    && (!InCombat || !Huraijin.EnoughLevel)
            //    && Huton.CanUse(out _)
            //    && !IsLastAction(false, Huton))
            //{
            //    SetNinjutsu(Huton);
            //    return false;
            //}

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
            if (IsBurstTime && Ten.CanUse(out _, InTrickAttack
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
        if ((uint)id == RabbitMedium.ID)
        {
            ClearNinjutsu();
            act = null;
            return false;
        }

        //First
        else if (id == ActionID.Ninjutsu)
        {
            //Can't use.
            if (!Player.HasStatus(true, StatusID.Kassatsu, StatusID.TenChiJin) &&
                !Ten.CanUse(out _, CanUseOption.EmptyOrSkipCombo))
            {
                return false;
            }
            LogMudraStep(_ninjustuAction.Ninjutsu[0], 0);

            if (IsPreviousActionNotRepeated(_ninjustuAction.Ninjutsu[0]))
            {
                act = new BaseAction((ActionID)_ninjustuAction.Ninjutsu[0].ID, ActionOption.Friendly);
                return true;
            }
        }
        //Finished
        if ((uint)id == _ninjustuAction.ID)
        {
            if (_ninjustuAction.CanUse(out act, CanUseOption.MustUse))
            {
                return true;
            }

            if (_ninjustuAction.ID == Doton.ID && !InCombat)
            {
                act = _ninjustuAction;
                return true;
            }
        }
        //Second
        if ((uint)id == FumaShuriken.ID)
        {
            if (_ninjustuAction.Ninjutsu.Length > 1 &&
                IsPreviousActionNotRepeated(_ninjustuAction.Ninjutsu[1]))
            {
                act = new BaseAction((ActionID)_ninjustuAction.Ninjutsu[1].ID, ActionOption.Friendly);
                LogMudraStep(_ninjustuAction.Ninjutsu[1], 1);

                return true;
            }
        }
        //Third
        if ((uint)id == Katon.ID || (uint)id == Raiton.ID || (uint)id == Hyoton.ID)
        {
            if (_ninjustuAction.Ninjutsu.Length > 2 &&
                IsPreviousActionNotRepeated(_ninjustuAction.Ninjutsu[2]))
            {
                act = new BaseAction((ActionID)_ninjustuAction.Ninjutsu[2].ID, ActionOption.Friendly);
                LogMudraStep(_ninjustuAction.Ninjutsu[2], 2);
                return true;
            }
        }

        return false;
    }

    private static bool IsPreviousActionNotRepeated(IAction skill)
    {
        var lastAction = RecordActions.FirstOrDefault()?.Action.RowId;

        LogLastUseCheck(skill, lastAction);

        return lastAction != skill.ID &&
               lastAction != skill.AdjustedID &&
               Math.Abs(WeaponElapsed - WeaponRemain) < 0.49f;
    }

    private static void LogLastUseCheck(IAction skill, uint? lastAction)
    {
        var answer = GetTimeStamp();
        PluginLog.Information($"************ Last Action Check *************");
        PluginLog.Information($"Action is {skill.Name}");
        PluginLog.Information($"Skill Id is {skill.ID}");
        PluginLog.Information($"Skill Adjusted Id is {skill.AdjustedID}");
        PluginLog.Information($"Last used Skill was: {RecordActions.FirstOrDefault()?.Action.Name}");
        PluginLog.Information($"Last used Skill Id was: {lastAction}");
        PluginLog.Information($"Timing - {answer}");
        PluginLog.Information($"----------- Last Action Check -------------");
    }

    private static void LogMudraStep(IAction step, int index)
    {
        var answer = GetTimeStamp();
        PluginLog.Information($"************ Setting up for {_ninjustuAction.Name} *************");
        PluginLog.Information($"Mudra #{index + 1} is {step.Name}");
        PluginLog.Information($"Mudra Id is {step.ID}");
        PluginLog.Information($"Mudra Adjusted Id is {step.AdjustedID}");
        PluginLog.Information($"Timing - {answer}");
        PluginLog.Information($"************ Leaving Set up for {_ninjustuAction.Name} *************");
    }

    private static string GetTimeStamp()
    {
        var timeSpan = TimeSpan.FromMilliseconds(StopWatch.ElapsedMilliseconds);
        var timeStamp = $"{timeSpan.Minutes:D2}m:{timeSpan.Seconds:D2}s:{timeSpan.Milliseconds:D3}ms";

        return timeStamp;
    }
}