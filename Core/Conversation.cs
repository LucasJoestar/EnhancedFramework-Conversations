// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
//  Use the [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Namespace", "Assembly", "Class")]
//  attribute to remove a managed reference error when renaming a script or an assembly.
//
// ================================================================================================ //

#if LOCALIZATION_PACKAGE
#define LOCALIZATION_ENABLED
#endif

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

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

[assembly: InternalsVisibleTo("EnhancedFramework.Conversations.Editor")]
namespace EnhancedFramework.Conversations {
    /// <summary>
    /// <see cref="Conversation"/> root node class.
    /// </summary>
    [Serializable, Ethereal]
    public sealed class ConversationRoot : ConversationNode {
        #region Global Members
        #if UNITY_EDITOR
        /// <summary>
        /// In the editor only, used to display and edit the conversation name.
        /// </summary>
        [SerializeField] internal Conversation conversation = null;

        // -----------------------

        public override string Text {
            get {
                if (conversation == null) {
                    return base.Text;
                }

                return conversation.name.Replace(conversation.name.GetPrefix(), string.Empty);
            }
            set {
                if (conversation == null) {
                    return;
                }

                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(conversation), $"{conversation.name.GetPrefix()}{value}");
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

            this.LogMessage($"Setup \'{Name}\', ready to be played", Conversation);
        }

        protected override void OnClose(Action _onNodeQuit = null) {
            base.OnClose(_onNodeQuit);

            this.LogMessage($"Closing \'{Name}\'", Conversation);
            CancelPlay();
        }
        #endregion

        #region Behaviour
        private static readonly int DelayerID = "ConversationDefaultPlayer".GetHashCode();

        // -----------------------

        public override void PlayCurrentNode() {
            base.PlayCurrentNode();

            this.LogMessage($"Playing node {CurrentNode.Guid} - \"{CurrentNode.Text}\"", Conversation);

            // Use a delay before playing the next node,
            // avoiding infinite loops on referenced links.
            Delayer.Call(DelayerID, .1f, PlayNextNode, false);
        }

        private void CancelPlay() {
            Delayer.CancelCall(DelayerID);
        }
        #endregion
    }

    /// <summary>
    /// Default <see cref="ConversationSettings"/> class, only containing an array of <see cref="string"/> for the speakers.
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
    [CreateAssetMenu(fileName = FilePrefix + "NewConversation", menuName = FrameworkUtility.MenuPath + "Conversation", order = FrameworkUtility.MenuOrder + 50)]
    public class Conversation : ScriptableObject
                                #if LOCALIZATION_ENABLED
                                , ILocalizable
                                #endif
    {
        public const string FilePrefix = "CNV_";

        #region Global Members
        [Section("Conversation")]

        [Tooltip("Node type to be used when creating a new default node in this conversation")]
        [SerializeField, DisplayName("Default Node")]
        private SerializedType<ConversationNode> defaultNodeType = new SerializedType<ConversationNode>(SerializedTypeConstraint.None, typeof(ConversationTextLine),
                                                                                                                                       #if LOCALIZATION_ENABLED
                                                                                                                                       typeof(ConversationLocalizedLine),
                                                                                                                                       #endif
                                                                                                                                       typeof(ConversationLink));

        [Tooltip("Node type to be used when creating a new link in this conversation")]
        [SerializeField, DisplayName("Default Link")]
        private SerializedType<ConversationLink> defaultLinkType = new SerializedType<ConversationLink>(SerializedTypeConstraint.BaseType, typeof(ConversationLink));

        [Space(5f)]

        [Tooltip("Class used to play this conversation, managing its behaviour")]
        [SerializeField, DisplayName("Conversation Player")]
        private SerializedType<ConversationPlayer> playerType = new SerializedType<ConversationPlayer>(SerializedTypeConstraint.None, typeof(ConversationDefaultPlayer));

        /// <summary>
        /// Node type to be used when creating a new default node in this conversation (must be derived from <see cref="ConversationNode"/>).
        /// </summary>
        public Type DefaultNodeType {
            get { return defaultNodeType.Type; }
            set { defaultNodeType.Type = value; }
        }

        /// <summary>
        /// Node type to be used when creating a new link in this conversation (must be derived from <see cref="ConversationLink"/>).
        /// </summary>
        public Type DefaultLinkType {
            get { return defaultLinkType.Type; }
            set { defaultLinkType.Type = value; }
        }

        /// <summary>
        /// Type class used to play this conversation, managing its behaviour (must be derived from <see cref="ConversationPlayer{T}"/>).
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

        [SerializeReference, Enhanced, Block] protected ConversationSettings settings = new ConversationDefaultSettings();

        /// <summary>
        /// <see cref="ConversationPlayer"/>-related settings of this conversation.
        /// </summary>
        public ConversationSettings Settings {
            get { return settings; }
        }

        /// <summary>
        /// Speaker names of this conversation.
        /// <br/> Especially used by property drawers.
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

        /// <summary>
        /// Indicates if this <see cref="Conversation"/> has any available node to play.
        /// </summary>
        public bool IsPlayable {
            get {
                foreach (var _node in root.nodes) {
                    if (_node.IsAvailable) {
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion

        #region Scriptable Object
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

            // ----- Local Method ----- \\

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

        #region Localization
        #if LOCALIZATION_ENABLED
        /// <inheritdoc cref="ILocalizable.GetLocalizationTables(Set{TableReference}, Set{TableReference})"/>
        public void GetLocalizationTables(Set<TableReference> _stringTables,  Set<TableReference> _assetTables) {
            root.GetLocalizationTables(_stringTables, _assetTables);
            settings.GetLocalizationTables(_stringTables, _assetTables);
        }
        #endif
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
        #endregion
    }
}
