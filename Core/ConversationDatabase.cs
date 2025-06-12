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

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// <see cref="Conversation"/>-related game database.
    /// </summary>
    public sealed class ConversationDatabase : BaseDatabase<ConversationDatabase> {
        #region Global Members
        [Section("Conversation Database")]

        [Tooltip("All conversations in the database")]
        [SerializeField] private EnhancedCollection<Conversation> conversations = new EnhancedCollection<Conversation>();

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Used to search for any specific node type")]
        [SerializeField] private SerializedType<ConversationNode> searchType = new SerializedType<ConversationNode>(SerializedTypeConstraint.Null | SerializedTypeConstraint.Abstract);

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

            ref List<Conversation> _span = ref conversations.collection;
            for (int i = _span.Count; i-- > 0;) {

                Conversation _temp = _span[i];

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

            ref List<Conversation> _span = ref conversations.collection;
            for (int i = _span.Count; i-- > 0;) {
                _span[i].ResetForNextPlay();
            }
        }
        #endregion

        #region Database
        /// <summary>
        /// Set all <see cref="Conversation"/> in the database.
        /// </summary>
        /// <param name="_conversations">All conversations to include in the database.</param>
        internal void SetDatabase(IList<Conversation> _conversations) {
            conversations.ReplaceBy(_conversations);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Utility method used to search for specific node(s) in all game conversations,
        /// and logging an informative message foreach found matching node.
        /// </summary>
        /// <param name="_inherit">If true, also search for all types that inherit from given type.</param>
        [Button(ActivationMode.Always, SuperColor.Crimson)]
        public void SearchForNodes(bool _inherit = true) {

            Type _type = searchType.Type;

            ref List<Conversation> _span = ref conversations.collection;
            for (int i = _span.Count; i-- > 0;) {

                Conversation _conversation = _span[i];
                DoFindNode(_conversation, _conversation.Root);
            }

            // ----- Local Method ----- \\

            void DoFindNode(Conversation _conversation, ConversationNode _root) {

                ref ConversationNode[] _nodes = ref _root.nodes;
                for (int i = _nodes.Length; i-- > 0;) {

                    ConversationNode _innerNode = _nodes[i];

                    if ((_innerNode.GetType() == _type) || (_inherit && _inherit.GetType().IsSubclassOf(_type))) {
                        Debug.LogWarning($"Found Node => {_innerNode.Text} - {_conversation.name} [{_innerNode.GetType().Name}]");
                    }

                    if (!_root.ShowNodes) {
                        continue;
                    }

                    DoFindNode(_conversation, _innerNode);
                }
            }
        }
        #endregion
    }
}
