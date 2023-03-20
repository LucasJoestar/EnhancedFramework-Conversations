// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;

using Min = EnhancedEditor.MinAttribute;

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// <see cref="ConversationEvent"/> with an already implemented delay.
    /// </summary>
    [Serializable]
    public abstract class DelayedConversationEvent : ConversationEvent {
        #region Global Members
        [Tooltip("Delay before playing this event, in second(s)")]
        [SerializeField, Enhanced, ShowIf("ShowDelay"), Min(0f)] public float Delay = 0f;

        /// <summary>
        /// Override this to hide the delay field in the inspector.
        /// </summary>
        public virtual bool ShowDelay {
            get { return true; }
        }
        #endregion

        #region Behaviour
        private DelayHandler delayedCall = default;

        // -----------------------

        protected override sealed bool OnPlay(ConversationPlayer _player) {
            // Immediate.
            if (Delay == 0f) {
                OnPlayed(_player);
            } else {
                // Delay.
                delayedCall = Delayer.Call(Delay, () => OnPlayed(_player), false);
            }

            return true;
        }

        protected override bool OnStop(ConversationPlayer _player, bool _isClosingConversation, Action _onComplete) {
            // Complete call.
            delayedCall.Complete();

            return base.OnStop(_player, _isClosingConversation, _onComplete);
        }

        // -----------------------

        /// <inheritdoc cref="OnPlay(ConversationPlayer)"/>
        protected abstract void OnPlayed(ConversationPlayer _player);
        #endregion
    }
}
