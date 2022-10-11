// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-ConversationSystem ===== //
// 
// Notes:
//
// ===================================================================================================== //

using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("EnhancedFramework.ConversationSystem.Editor")]
namespace EnhancedFramework.ConversationSystem {
    /// <summary>
    /// Contains multiple <see cref="ConversationNode"/> utilties,
    /// to make the connection between the editor and the runtime system.
    /// </summary>
    public static class ConversationNodeUtility {
        #region Content
        #if UNITY_EDITOR
        [SerializeReference] internal static ConversationNode copyBuffer = null;
        #endif

        /// <summary>
        /// Clipboard buffer for a <see cref="ConversationNode"/> reference (for editor only).
        /// </summary>
        public static ConversationNode CopyBuffer {
            get {
                #if UNITY_EDITOR
                return copyBuffer;
                #else
                return null;
                #endif
            }
        }
        #endregion
    }
}
