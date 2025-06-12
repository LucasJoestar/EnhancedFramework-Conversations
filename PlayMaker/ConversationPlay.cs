// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using HutongGames.PlayMaker;
using System;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.Conversations.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to play a <see cref="Conversations.Conversation"/>.
    /// </summary>
    [Tooltip("Plays a Conversation")]
    [ActionCategory(CategoryName)]
    public sealed class ConversationPlay : BaseConversationFSM {
        #region Global Members
        // -------------------------------------------
        // Variable - Closed
        // -------------------------------------------

        [Tooltip("The Conversation to play.")]
        [RequiredField, ObjectType(typeof(ConversationBehaviour))]
        public FsmObject Conversation = null;

        [Tooltip("Event to send when the Conversation is closed.")]
        public FsmEvent ClosedEvent;
        #endregion

        #region Behaviour
        private Action<Conversation, ConversationPlayer> onClosedCallback = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            Conversation = null;
            ClosedEvent  = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (GetConversation(out Conversation _conversation)) {

                onClosedCallback ??= OnClosed;

                _conversation.OnClosed += onClosedCallback;
                _conversation.CreatePlayer();
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (GetConversation(out Conversation _conversation)) {
                _conversation.OnClosed -= onClosedCallback;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private bool GetConversation(out Conversation _conversation) {

            if (Conversation.Value is ConversationBehaviour _behaviour) {
                _conversation = _behaviour.Conversation;
                return true;
            }

            _conversation = null;
            return false;
        }

        private void OnClosed(Conversation _conversation, ConversationPlayer _player) {
            Fsm.Event(ClosedEvent);
        }
        #endregion
    }
}
