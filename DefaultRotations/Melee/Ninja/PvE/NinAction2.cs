namespace GlaiveRotations.Melee.Ninja.PvE;

public sealed partial class NinjaPvE
{
    public class NinAction2 : BaseAction, INinAction
    {
        /// <summary>
        /// 
        /// </summary>
        public IBaseAction[] Ninjutsu { get; }

        internal NinAction2(ActionID actionID, params IBaseAction[] ninjutsu) : base(actionID)
        {
            Ninjutsu = ninjutsu;
        }
    }
}