// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-ConversationSystem ===== //
// 
// Notes:
//
//  Use the [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Namespace", "Assembly", "Class")]
//  attribute to remove a managed reference error when renaming a script or an assembly.
//
// ===================================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

using DisplayName = EnhancedEditor.DisplayNameAttribute;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

using ArrayUtility = EnhancedEditor.ArrayUtility;

[assembly: InternalsVisibleTo("EnhancedFramework.ConversationSystem.Editor")]
namespace EnhancedFramework.ConversationSystem {
    /// <summary>
    /// <see cref="Conversation"/> root node class.
    /// </summary>
    [Serializable, Ethereal]
    public sealed class ConversationRoot : ConversationNode {
        #region Global Members
        #if UNITY_EDITOR
        /// <summary>
        /// Only in the editor, display and edit the conversation name.
        /// </summary>
        [SerializeField] internal Conversation conversation = null;

        // -----------------------

        public override string Text {
            get {
                if (conversation == null) {
                    return base.Text;
                }

                return conversation.name.Replace(Conversation.Prefix, string.Empty);
            }
            set {
                if (conversation == null) {
                    return;
                }

                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(conversation), $"{Conversation.Prefix}{value}");
            }
        }
        #endif

        public override string DefaultSpeaker {
            get { return "[ROOT]"; }
        }
        #endregion

        #region Behaviour
        public override void Play(ConversationPlayer _player) {
            base.Play(_player);

            // Automatically play the next node.
            _player.PlayNextNode();
        }
        #endregion

        #region Editor Utility
        internal protected override int GetEditorIcon(int _index, out string _iconName) {
            switch (_index) {
                case 0:
                    _iconName = "Profiler.Custom";
                    break;

                default:
                    _iconName = string.Empty;
                    break;
            }

            return 1;
        }

        internal protected override int OnEditorContextMenu(int _index, out GUIContent _content, out Action _callback, out bool _enabled) {
            _content = null;
            _callback = null;
            _enabled = false;

            return 0;
        }
        #endregion
    }

    /// <summary>
    /// Default <see cref="ConversationPlayer"/> class, only sending logs about its current state.
    /// </summary>
    [Serializable, DisplayName("<None>")]
    public class ConversationDefaultPlayer : ConversationPlayer<ConversationDefaultSettings> {
        #region State
        protected override void OnSetup() {
            base.OnSetup();

            Conversation.Log($"{typeof(ConversationPlayer).Name} - Setup \'{Name}\', ready to play.");
        }

        protected override void OnClose(Action _onNodeQuit = null) {
            base.OnClose(_onNodeQuit);

            Conversation.Log($"{typeof(ConversationPlayer).Name} - Terminate playing \'{Name}\'.");
        }
        #endregion

        #region Behaviour
        public override void PlayCurrentNode() {
            base.PlayCurrentNode();

            Conversation.Log($"{typeof(ConversationPlayer).Name} - Playing node {CurrentNode.Guid} - \'{CurrentNode.Text}\'.");

            #if DOTWEEN_ENABLED
            DOVirtual.DelayedCall(.01f, PlayNextNode, false);
            #else
            PlayNextNode();
            #endif
        }
        #endregion
    }

    /// <summary>
    /// Default <see cref="ConversationSettings"/> class, only containing an array of string for the speakers.
    /// </summary>
    [Serializable, DisplayName("<Default>")]
    public class ConversationDefaultSettings : ConversationSettings<string> {
        #region Global Members
        /// <inheritdoc cref="ConversationDefaultSettings"/>
        public ConversationDefaultSettings() {
            Speakers = new string[] { "Player", "NPC" };
        }
        #endregion

        #region Speaker
        public override string GetSpeakerAt(int _index) {
            return Speakers[_index];
        }
        #endregion
    }

    /// <summary>
    /// <see cref="ScriptableObject"/> database for a conversation.
    /// </summary>
    [CreateAssetMenu(fileName = Prefix + "NewConversation", menuName = "Enhanced Editor/Conversation", order = 200)]
    public class Conversation : ScriptableObject
                                #if LOCALIZATION_ENABLED
                                , ILocalizable
                                #endif
    {
        public const string Prefix = "CNV_";

        #region Global Members
        [Section("Conversation")]

        [SerializeField, DisplayName("Default Node")]
        private SerializedType<ConversationNode> defaultNodeType = new SerializedType<ConversationNode>(SerializedTypeConstraint.None, typeof(ConversationTextLine),
                                                                                                                                       #if LOCALIZATION_ENABLED
                                                                                                                                       typeof(ConversationLocalizedLine),
                                                                                                                                       #endif
                                                                                                                                       typeof(ConversationLink));

        [SerializeField, DisplayName("Default Link")]
        private SerializedType<ConversationLink> defaultLinkType = new SerializedType<ConversationLink>(SerializedTypeConstraint.BaseType, typeof(ConversationLink));

        [Space(5f)]

        [SerializeField, DisplayName("Conversation Player")]
        private SerializedType<ConversationPlayer> playerType = new SerializedType<ConversationPlayer>(SerializedTypeConstraint.None, typeof(ConversationDefaultPlayer));

        /// <summary>
        /// The default type of node used for this conversation (must be derived from <see cref="ConversationNode"/>).
        /// </summary>
        public Type DefaultNodeType {
            get { return defaultNodeType.Type; }
            set { defaultNodeType.Type = value; }
        }

        /// <summary>
        /// The default type of link node used for this conversation (must be derived from <see cref="ConversationLink"/>).
        /// </summary>
        public Type DefaultLinkType {
            get { return defaultLinkType.Type; }
            set { defaultLinkType.Type = value; }
        }

        /// <summary>
        /// The type of player used for this conversation (must be derived from <see cref="ConversationPlayer{T}"/>).
        /// </summary>
        public Type PlayerType {
            get { return playerType.Type; }
            set {
                playerType.Type = value;

                var _settings = Activator.CreateInstance(GetSettingsType(value));
                settings = EnhancedUtility.CopyObjectContent(settings, _settings) as ConversationSettings;
            }
        }

        // -----------------------

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeReference, Block] protected ConversationSettings settings = new ConversationDefaultSettings();

        /// <summary>
        /// The settings of this conversation.
        /// </summary>
        public ConversationSettings Settings {
            get { return settings; }
        }

        /// <summary>
        /// The speakers of this conversation.
        /// <br/> Used by property drawers.
        /// </summary>
        public string[] Speakers {
            get {
                string[] _speakers = new string[settings.SpeakerCount];
                for (int i = 0; i < _speakers.Length; i++) {
                    _speakers[i] = settings.GetSpeakerAt(i);
                }

                return _speakers;
            }
        }

        // -----------------------

        [SerializeReference, HideInInspector] internal ConversationRoot root = new ConversationRoot();

        /// <summary>
        /// The root <see cref="ConversationNode"/> of this <see cref="Conversation"/>.
        /// </summary>
        public ConversationRoot Root {
            get { return root; }
        }
        #endregion

        #region Node Management
        /// <summary>
        /// Adds a new default node to this conversation, at a specific root node.
        /// </summary>
        /// <inheritdoc cref="AddNode(ConversationNode, Type)"/>
        public ConversationNode AddDefaultNode(ConversationNode _root) {
            return AddNode(_root, DefaultNodeType);
        }

        /// <summary>
        /// Adds a new specific type of <see cref="ConversationNode"/> to a specific root node from this conversation.
        /// </summary>
        /// <param name="_root">The root <see cref="ConversationNode"/> to add a new node to.</param>
        /// <param name="_nodeType">The type of node to create and add (must inherit from <see cref="ConversationNode"/>).</param>
        /// <returns>The newly created node.</returns>
        public ConversationNode AddNode(ConversationNode _root, Type _nodeType) {
            if (!_nodeType.IsSubclassOf(typeof(ConversationNode))) {
                return null;
            }

            ConversationNode _node = Activator.CreateInstance(_nodeType) as ConversationNode;
            _root.AddNode(_node);

            return _node;
        }

        /// <summary>
        /// Removes a specific <see cref="ConversationNode"/> from this conversation.
        /// </summary>
        /// <param name="_node">The <see cref="ConversationNode"/> to remove.</param>
        public void RemoveNode(ConversationNode _node) {
            DoRemoveNode(root);

            // ----- Local Methods ----- \\

            bool DoRemoveNode(ConversationNode _root) {
                foreach (ConversationNode _innerNode in _root.nodes) {
                    if (_innerNode == _node) {
                        ArrayUtility.Remove(ref _root.nodes, _innerNode);
                        return true;
                    }

                    if (DoRemoveNode(_innerNode)) {
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Creates and setup a new <see cref="ConversationPlayer"/> for this conversation.
        /// <br/> Use this to play its content.
        /// </summary>
        /// <returns>The newly created <see cref="ConversationPlayer"/> to play this conversation.</returns>
        public ConversationPlayer CreatePlayer() {
            var _player = Activator.CreateInstance(PlayerType) as ConversationPlayer;
            _player.Setup(this);

            return _player;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get this conversation <see cref="ConversationSettings"/> type (<see cref="ConversationPlayer{T}"/>-related).
        /// </summary>
        /// <param name="_player">The <see cref="ConversationPlayer{T}"/> type to get the associated settings.</param>
        /// <returns>This conversation settings type.</returns>
        private Type GetSettingsType(Type _player) {
            while (_player.BaseType != null) {
                _player = _player.BaseType;

                if (_player.IsGenericType && (_player.GetGenericTypeDefinition() == typeof(ConversationPlayer<>))) {
                    return _player.GetGenericArguments()[0];
                }
            }

            return null;
        }

        #if LOCALIZATION_ENABLED
        /// <summary>
        /// Get all localization <see cref="TableReference"/> used in this conversation.
        /// </summary>
        /// <param name="_stringTables"><see cref="StringTable"/> references collection to fill.</param>
        /// <param name="_assetTables"><see cref="AssetTable"/> references collection to fill.</param>
        public void GetLocalizationTables(Set<TableReference> _stringTables,  Set<TableReference> _assetTables) {
            root.GetLocalizationTables(_stringTables, _assetTables);
        }
        #endif
        #endregion

        #region Editor Utility
        #if UNITY_EDITOR
        private void Awake() {
            // Root conversation setup.
            root.conversation = this;

            RefreshValues();
        }

        private void OnValidate() {
            RefreshValues();
        }

        // -----------------------

        private void RefreshValues() {
            if (Application.isPlaying) {
                return;
            }

            // Settings type update.
            if (GetSettingsType(PlayerType) != settings.GetType()) {
                PlayerType = playerType;
            }
        }
        #endif
        #endregion
    }
}
