// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core.Settings;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedFramework.Conversations.Editor {
    /// <summary>
    /// <see cref="Item"/>-related game database.
    /// </summary>
    public class ConversationDatabase : BaseDatabase<ConversationDatabase> {
        #region Global Members
        [Section("Conversation Database")]

        [Tooltip("All conversations in the database")]
        [SerializeField] private EnhancedCollection<Conversation> conversations = new EnhancedCollection<Conversation>();

        // -----------------------

        /// <summary>
        /// Total amount of <see cref="Conversation"/> in the database.
        /// </summary>
        public int ConversationCount {
            get { return conversations.Count; }
        }
        #endregion

        #region Conversation
        /// <summary>
        /// Finds the first <see cref="Conversation"/> in the database matching a given name.
        /// </summary>
        /// <param name="_name">Name of the <see cref="Conversation"/> to find.</param>
        /// <param name="_item"><see cref="Conversation"/> with the given name (null if none).</param>
        /// <returns>True if a <see cref="Conversation"/> with the given name could be successfully found, false otherwise.</returns>
        public bool FindConversation(string _name, out Conversation _conversation) {
            foreach (Conversation _temp in conversations) {

                if (_temp.name.RemovePrefix().ToLower().Equals(_name.RemovePrefix().ToLower(), StringComparison.Ordinal)) {
                    _conversation = _temp;
                    return true;
                }
            }

            _conversation = null;
            return false;
        }

        /// <summary>
        /// Resets all conversations in the database.
        /// </summary>
        public void ResetConversations() {

            foreach (Conversation _conversation in conversations) {
                _conversation.ResetForNextPlay();
            }
        }
        #endregion

        #region Database
        /// <summary>
        /// Set all <see cref="Conversation"/> in the database.
        /// </summary>
        /// <param name="_conversations">All conversations to include in the database.</param>
        internal void SetDatabase(IList<Conversation> _conversations) {
            conversations.Clear();
            conversations.AddRange(_conversations);
        }
        #endregion
    }
}
