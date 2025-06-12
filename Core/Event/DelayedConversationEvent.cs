// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedFramework.Core;
using System;

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// <see cref="ConversationEvent"/> with an already implemented delay.
    /// </summary>
    [Serializable]
    public abstract class DelayedConversationEvent : ConversationEvent {
        #region Global Members
        /// <summary>
        /// Delay before playing this event, in second(s).
        /// </summary>
        public virtual float Delay {
            get { return 0f; }
        }

        public override bool IsPlaying {
            get { return delayedCall.IsValid; }
        }
        #endregion

        #region Behaviour
        private Action onDelayCompleteCallback = null;

        private ConversationPlayer delayPlayer = null;
        private DelayHandler delayedCall       = default;

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        protected override sealed bool OnPlay(ConversationPlayer _player) {
            // Immediate.
            if (Delay == 0f) {
                OnPlayed(_player);
            } else {
                // Delay.
                onDelayCompleteCallback ??= OnComplete;

                delayPlayer = _player;
                delayedCall = Delayer.Call(Delay, onDelayCompleteCallback);
            }

            return true;

            // ----- Local Method ----- \\

            void OnComplete() {
                OnPlayed(delayPlayer);
            }
        }

        protected override bool OnStop(ConversationPlayer _player, bool _isClosingConversation, Action _onComplete) {
            // Complete call.
            delayedCall.Complete();

            return base.OnStop(_player, _isClosingConversation, _onComplete);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <inheritdoc cref="OnPlay(ConversationPlayer)"/>
        protected abstract void OnPlayed(ConversationPlayer _player);
        #endregion
    }
}
