// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using HutongGames.PlayMaker;

namespace EnhancedFramework.Conversations.PlayMaker {
    /// <summary>
    /// Base <see cref="FsmStateAction"/> for a <see cref="Conversation"/>.
    /// </summary>
    public abstract class BaseConversationFSM : FsmStateAction {
        #region Global Members
        public const string CategoryName = "Conversation";
        #endregion
    }
}
