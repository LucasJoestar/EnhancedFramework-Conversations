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
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="Conversations.Conversation"/> is being closed.
    /// </summary>
    [Tooltip("Sends an Event when a Conversation is being closed")]
    [ActionCategory(CategoryName)]
    public sealed class ConversationClosedEvent : BaseConversationFSM {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Conversation used by the event.")]
        [RequiredField, ObjectType(typeof(Conversation))]
        public FsmObject Conversation = null;

        [Tooltip("Event to send when the Conversation is being closed.")]
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

            if (Conversation.Value is Conversation _conversation) {

                onClosedCallback ??= OnClosed;
                _conversation.OnClosed += onClosedCallback;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Conversation.Value is Conversation _conversation) {
                _conversation.OnClosed -= onClosedCallback;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnClosed(Conversation _conversation, ConversationPlayer _player) {
            Fsm.Event(ClosedEvent);
        }
        #endregion
    }
}
