// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-ConversationSystem ===== //
// 
// Notes:
//
// ===================================================================================================== //

using System;
using UnityEngine;

namespace EnhancedFramework.ConversationSystem {
    /// <summary>
    /// The default behaviour used to determine the next node to play from a <see cref="Conversation"/>.
    /// </summary>
    public enum GetNextNodeBehaviour {
        PlayFirst,
        PlayLast,
        Random
    }

    /// <summary>
    /// Base class for all <see cref="Conversation"/>-related settings.
    /// <br/> You can inherit from <see cref="ConversationSettings{T}"/> for a quick implementation.
    /// </summary>
    [Serializable]
    public abstract class ConversationSettings {
        #region Global Members
        /// <summary>
        /// The default behaviour used to determine the next node to play using these settings.
        /// </summary>
        public GetNextNodeBehaviour NextNodeBehaviour = GetNextNodeBehaviour.PlayFirst;

        /// <summary>
        /// The total count of speakers in the conversation.
        /// </summary>
        public abstract int SpeakerCount { get; }
        #endregion

        #region Speaker
        /// <summary>
        /// Get the name of the speaker at a specific index.
        /// </summary>
        /// <param name="_index">Index of the speaker to get.</param>
        /// <returns>The name of the speaker.</returns>
        public abstract string GetSpeakerAt(int _index);
        #endregion
    }

    /// <summary>
    /// <see cref="ConversationSettings"/> class with a ready-to-use array of speakers.
    /// </summary>
    [Serializable]
    public abstract class ConversationSettings<T> : ConversationSettings {
        #region Global Members
        [Space(10f)]

        /// <summary>
        /// The speakers of the conversation.
        /// </summary>
        public T[] Speakers = new T[] { };

        public override int SpeakerCount {
            get { return Speakers.Length; }
        }
        #endregion
    }
}
