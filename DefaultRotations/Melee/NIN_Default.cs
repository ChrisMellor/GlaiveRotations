namespace GlaiveRotations.Melee;

[RotationDesc(ActionID.Mug)]
[SourceCode(Path = "main/GlaiveRotations/Melee/NinDefault.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/nin/earlymug3.png")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/nin/nininfographicwindows.png")]
[LinkDescription("https://docs.google.com/spreadsheets/u/0/d/1BZZrqWMRrugCeiBICEgjCz2vRNXt_lRTxPnSQr24Em0/htmlview#",
    "Under the “Planner (With sample section)”")]
[YoutubeLink(ID = "Al9KlhA3Zvw")]
public sealed class NinDefault : NIN_Base
{
    private const int HutonPrePullTimer = 10;
    private const int SuitonPrePullTimer = 6;
    public override string GameVersion => "6.35";

    public override string RotationName => "Standard";

    private static INinAction _ninActionAim;
    private static bool _firstTrickAttack = true;
    private static bool _firstDawd = true;

    private static bool InTrickAttack => TrickAttack.IsCoolingDown && !TrickAttack.ElapsedAfter(15) && TrickAttack.ElapsedAfter(0.1f);
    private static bool InMug => Mug.IsCoolingDown && !Mug.ElapsedAfter(20) && Mug.ElapsedAfter(0.1f);
    private static bool NoNinjutsu => AdjustId(ActionID.Ninjutsu) is ActionID.Ninjutsu or ActionID.RabbitMedium;

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration()
            .SetBool("UseHide", true, "Use Hide")
            .SetBool("AutoUnhide", true, "Use Unhide");
    }

    protected override IAction CountDownAction(float remainTime)
    {
        if (remainTime > HutonPrePullTimer)
        {
            ClearNinjutsu();
        }

        var realInHuton = !HutonEndAfterGCD() || IsLastAction(false, Huton);
        if (realInHuton && _ninActionAim == Huton)
        {
            ClearNinjutsu();
        }

        if (DoNinjutsu(out var act))
        {
            if (act == Suiton && remainTime > CountDownAhead)
            {
                return null;
            }

            return act;
        }

        switch (remainTime)
        {
            case < SuitonPrePullTimer:
                SetNinjutsu(Suiton);
                break;
            case < HutonPrePullTimer when _ninActionAim == null && Ten.IsCoolingDown && Hide.CanUse(out act):
                return act;
            case < HutonPrePullTimer:
                {
                    if (!realInHuton)
                    {
                        SetNinjutsu(Huton);
                    }

                    break;
                }
        }

        return base.CountDownAction(remainTime);
    }

    #region Ninjutsu
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

        if (_ninActionAim != null && _ninActionAim != act)
        {
            _ninActionAim = act;
        }
    }

    private static void ClearNinjutsu()
    {
        _ninActionAim = null;
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

        if (_ninActionAim != null && WeaponRemain < 0.2)
        {
            return false;
        }

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
            if (Huraijin.CanUse(out act))
            {
                return true;
            }

            if (!HutonEndAfterGCD() && _ninActionAim?.ID == Huton.ID)
            {
                ClearNinjutsu();
                return false;
            }
            if (Ten.CanUse(out _, CanUseOption.EmptyOrSkipCombo)
               && (!InCombat || !Huraijin.EnoughLevel) && Huton.CanUse(out _)
               && !IsLastAction(false, Huton))
            {
                SetNinjutsu(Huton);
                return false;
            }

            if (Katon.CanUse(out _))
            {
                if (!Player.HasStatus(true, StatusID.Doton)
                    && !IsMoving
                    && !TenChiJin.WillHaveOneCharge(10))
                {
                    SetNinjutsu(Doton);
                }
                else
                {
                    SetNinjutsu(Katon);
                }

                return false;
            }

            if (IsBurst
                && TrickAttack.WillHaveOneCharge(18)
                && Suiton.CanUse(out _))
            {
                SetNinjutsu(Suiton);
                return false;
            }

            if (Ten.CanUse(out _/*, InTrickAttack
                 && !Player.HasStatus(false, StatusID.RaijuReady)
                    ? CanUseOption.EmptyOrSkipCombo
                    : CanUseOption.None*/))
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

        var actions = new IAction[]
        {
            DotonChi, SuitonJin, RabbitMedium, FumaShuriken, Katon, Raiton, Hyoton, Huton, Doton, Suiton, GokaMekkyaku, HyoshoRanryu
        };

        if (IsLastAction(false, actions))
        {
            ClearNinjutsu();
        }

        return false;
    }

    private static bool DoNinjutsu(out IAction act)
    {
        act = null;

        if (Player.HasStatus(true, StatusID.TenChiJin))
        {
            var tenId = AdjustId(Ten.ID);
            var chiId = AdjustId(Chi.ID);
            var jinId = AdjustId(Jin.ID);

            if (tenId == FumaShurikenTen.ID
                && !IsLastAction(false, FumaShurikenJin, FumaShurikenTen))
            {
                if (Katon.CanUse(out _))
                {
                    if (FumaShurikenJin.CanUse(out act))
                    {
                        return true;
                    }
                }
                if (FumaShurikenTen.CanUse(out act))
                {
                    return true;
                }
            }
            else if (tenId == KatonTen.ID
                     && !IsLastAction(false, KatonTen))
            {
                if (KatonTen.CanUse(out act, CanUseOption.MustUse))
                {
                    return true;
                }
            }
            else if (chiId == RaitonChi.ID
                     && !IsLastAction(false, RaitonChi))
            {
                if (RaitonChi.CanUse(out act, CanUseOption.MustUse))
                {
                    return true;
                }
            }
            else if (chiId == DotonChi.ID
                     && !IsLastAction(false, DotonChi))
            {
                if (DotonChi.CanUse(out act, CanUseOption.MustUse))
                {
                    return true;
                }
            }
            else if (jinId == SuitonJin.ID
                     && !IsLastAction(false, SuitonJin))
            {
                if (SuitonJin.CanUse(out act, CanUseOption.MustUse))
                {
                    return true;
                }
            }
        }

        if (!Player.WillStatusEnd(3, false, StatusID.Kassatsu)
            && Player.HasStatus(false, StatusID.Kassatsu)
            && !InTrickAttack)
        {
            return false;
        }

        if (_ninActionAim == null)
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

        if (id == ActionID.Ninjutsu)
        {
            if (!Player.HasStatus(true, StatusID.Kassatsu, StatusID.TenChiJin)
                && !Ten.CanUse(out _, CanUseOption.EmptyOrSkipCombo)
                && !IsLastAction(false, _ninActionAim.Ninjutsu[0]))
            {
                return false;
            }
            act = _ninActionAim.Ninjutsu[0];
            return true;
        }

        if ((uint)id == _ninActionAim.ID)
        {
            if (_ninActionAim.CanUse(out act, CanUseOption.MustUse))
            {
                return true;
            }

            if (_ninActionAim.ID == Doton.ID && !InCombat)
            {
                act = _ninActionAim;
                return true;
            }
        }
        else if ((uint)id == FumaShuriken.ID)
        {
            if (_ninActionAim.Ninjutsu.Length > 1
                && !IsLastAction(false, _ninActionAim.Ninjutsu[1]))
            {
                act = _ninActionAim.Ninjutsu[1];
                return true;
            }
        }
        else if ((uint)id == Katon.ID || (uint)id == Raiton.ID || (uint)id == Hyoton.ID)
        {
            if (_ninActionAim.Ninjutsu.Length > 2
                && !IsLastAction(false, _ninActionAim.Ninjutsu[2]))
            {
                act = _ninActionAim.Ninjutsu[2];
                return true;
            }
        }

        return false;
    }
    #endregion

    protected override bool GeneralGCD(out IAction act)
    {
        var hasRaijuReady = Player.HasStatus(true, StatusID.RaijuReady);
        var hasTenChiJinReady = Player.HasStatus(true, StatusID.TenChiJin);

        if ((InTrickAttack || InMug)
            && !hasRaijuReady
            && PhantomKamaitachi.CanUse(out act))
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

        if (NoNinjutsu)
        {
            if (!CombatElapsedLess(10) && FleetingRaiju.CanUse(out act) && !hasTenChiJinReady)
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

            if (HakkeMujinsatsu.CanUse(out act))
            {
                return true;
            }

            if (DeathBlossom.CanUse(out act))
            {
                return true;
            }

            if (ArmorCrush.CanUse(out act) && !InTrickAttack)
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

        if (!InCombat && _ninActionAim == null
                      && Configs.GetBool("UseHide")
                      && Ten.IsCoolingDown
                      && Hide.CanUse(out act))
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

        if (Kassatsu.CanUse(out act))
        {
            return true;
        }

        if (UseBurstMedicine(out act))
        {
            return true;
        }

        if (IsBurst && !CombatElapsedLess(5) && Mug.CanUse(out act))
        {
            return true;
        }

        if (CombatElapsedLess(SuitonPrePullTimer))
        {
            return base.EmergencyAbility(nextGCD, out act);
        }

        if (_firstTrickAttack)
        {
            _firstTrickAttack = false;
            if (TrickAttack.CanUse(out act, CanUseOption.OnLastAbility))
            {
                return true;
            }
        }
        else
        {
            if (TrickAttack.CanUse(out act))
            {
                return true;
            }
        }

        if (TrickAttack.IsCoolingDown
            && !TrickAttack.WillHaveOneCharge(19)
            && Meisui.CanUse(out act))
        {
            return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }


    protected override bool AttackAbility(out IAction act)
    {
        act = null;
        if (!NoNinjutsu || !InCombat)
        {
            return false;
        }

        if (!IsMoving && InTrickAttack && !Ten.ElapsedAfter(30) && TenChiJin.CanUse(out act))
        {
            return true;
        }

        if (!CombatElapsedLess(5) && Bunshin.CanUse(out act))
        {
            return true;
        }

        if (InTrickAttack)
        {
            if (!DreamWithinADream.EnoughLevel)
            {
                if (Assassinate.CanUse(out act))
                {
                    return true;
                }
            }
            else
            {
                if (_firstDawd)
                {
                    if (DreamWithinADream.CanUse(out act))
                    {
                        _firstDawd = false;
                        return true;
                    }
                }
                if (DreamWithinADream.CanUse(out act) && HyoshoRanryu.IsInCooldown)
                {
                    return true;
                }
            }
        }

        if ((InMug && !InTrickAttack) ||
            (Bunshin.WillHaveOneCharge(10)
             && !Player.HasStatus(false, StatusID.PhantomKamaitachiReady)
             && !Mug.WillHaveOneCharge(2)))
        {
            return base.AttackAbility(out act);
        }

        if (HellfrogMedium.CanUse(out act))
        {
            return true;
        }

        if (Bhavacakra.CanUse(out act))
        {
            return true;
        }

        return base.AttackAbility(out act);
    }

    public override void DisplayStatus()
    {
        ImGui.Text(_ninActionAim?.ToString() ?? "No Aimed Ninjustus.");
        base.DisplayStatus();
    }
}