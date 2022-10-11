// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-ConversationSystem ===== //
// 
// Notes:
//
// ===================================================================================================== //

using EnhancedEditor.Editor;
using UnityEditor;

namespace EnhancedFramework.ConversationSystem.Editor {
    /// <summary>
    /// Custom <see cref="Conversation"/> editor, used to draw various parameters from the <see cref="ConversationEditorWindow"/>.
    /// </summary>
    [CustomEditor(typeof(Conversation), true), CanEditMultipleObjects]
    public class ConversationEditor : UnityObjectEditor {
        #region Editor Content
        private Conversation conversation = null;

        // -----------------------

        protected override void OnEnable() {
            base.OnEnable();

            conversation = target as Conversation;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            // While drawing the window inspector, constantly repaint to correctly display it.
            if (ConversationEditorWindow.DrawInsepector(conversation)) {
                Repaint();
            } else {
                base.OnInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}
