// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using EnhancedEditor.Editor;
using EnhancedFramework.Core;
using UnityEditor;
using UnityEngine;

namespace EnhancedFramework.Conversations.Editor {
    /// <summary>
    /// Custom <see cref="ConversationNode"/> drawer, used to display the class content when using the <see cref="SerializeReference"/> attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConversationNode), true)]
    public class ConversationNodePropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private const float Margins = 4f;

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            SerializedProperty _next = _property.Copy();
            SerializedProperty _current = _property.FindPropertyRelative("guid");
            SerializedProperty _nextVisible = _current.Copy();

            _next.Next(false);
            _nextVisible.NextVisible(false);

            float _yMin = _position.yMin;

            // We only want to display the node properties, so:
            //
            //  • Register the next node property (not visible) using _next - used to exit the loop.
            //  • Register the next visible node property using _nextVisible - used to discard hidden properties.
            //  • Use the _current property to iterate over all node properties (visible and not visible),
            //  to display the one that should be and exit when reaching the next node property.
            //
            //  A simple "isVisible" property from the SerializedProperty would simplify this mess.

            EditorGUI.PropertyField(_position, _current);
            IncreasePosition(Margins);

            string _section = ObjectNames.NicifyVariableName(_property.managedReferenceFullTypename.Split('.').Last());
            EnhancedEditorGUI.Section(_position, EnhancedEditorGUIUtility.GetLabelGUI(_section));

            IncreasePosition(Margins);

            while (_current.Next(false) && !SerializedProperty.EqualContents(_current, _next)) {
                if (!SerializedProperty.EqualContents(_current, _nextVisible)) {
                    continue;
                }

                _nextVisible.NextVisible(false);

                _position.height = EditorGUI.GetPropertyHeight(_current, true);
                EditorGUI.PropertyField(_position, _current, true);

                IncreasePosition();
            }

            return _position.yMin - EditorGUIUtility.standardVerticalSpacing - _yMin;

            // ----- Local Method ----- \\

            void IncreasePosition(float _space = 0f) {
                _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing + _space;
            }
        }
        #endregion
    }
}
