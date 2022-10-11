// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-ConversationSystem ===== //
// 
// Notes:
//
// ===================================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;

#if LOCALIZATION_ENABLED
using UnityEngine.Localization;
using DisplayName = EnhancedEditor.DisplayNameAttribute;
#endif

namespace EnhancedFramework.ConversationSystem {
    /// <summary>
    /// Base <see cref="ConversationNode"/> line class.
    /// <br/> Inherit from this to create your own lines.
    /// </summary>
    /// <typeparam name="T">This line content type.</typeparam>
    [Serializable]
    public abstract class ConversationLine<T> : ConversationNode {
        #region Global Members
        /// <summary>
        /// This line content.
        /// </summary>
        [Enhanced, Block] public T Line = Activator.CreateInstance<T>();

        [Space(10f)]

        /// <summary>
        /// Index of this line speaker (from <see cref="ConversationSettings.GetSpeakerAt(int)"/>).
        /// </summary>
        [SerializeField, Enhanced, DisplayName("Speaker"), Popup("Speakers")] protected int speakerIndex = 0;

        #if UNITY_EDITOR
        [Space(15f)]

        [SerializeField, TextArea(2, 7)] internal string comment = string.Empty;
        #endif

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        /// <summary>
        /// The required flags for this line to be available to play.
        /// </summary>
        public FlagValueGroup RequiredFlags = new FlagValueGroup();

        /// <summary>
        /// The flags to be modified when this line is played.
        /// </summary>
        public FlagValueGroup AfterFlags = new FlagValueGroup();

        // -----------------------

        public override int SpeakerIndex {
            get { return speakerIndex; }
        }

        public override bool IsAvailable {
            get { return RequiredFlags.IsValid(); }
        }
        #endregion

        #region Behaviour
        public override void Play(ConversationPlayer _player) {
            base.Play(_player);

            // Update flag values on play (safer than on exit).
            AfterFlags.SetValues();
        }
        #endregion

        #region Editor Utility
        protected internal override int GetEditorIcon(int _index, out string _iconName) {
            switch (_index) {
                case 0:
                    _iconName = "console.infoicon.sml";
                    break;

                case 1:
                    _iconName = "winbtn_win_close@2x";
                    break;

                default:
                    _iconName = string.Empty;
                    break;
            }

            return (nodes.Length == 0) ? 2 : 1;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="ConversationLine{T}"/> node class with a single text and an associated audio file.
    /// </summary>
    [Serializable, DisplayName("Text Line")]
    public class ConversationTextLine : ConversationLine<ConversationTextLine.Content> {
        /// <summary>
        /// Wrapper for the <see cref="ConversationTextLine"/> node line content.
        /// </summary>
        [Serializable]
        public class Content {
            #region Content
            /// <summary>
            /// The string text of this line
            /// </summary>
            [Enhanced, EnhancedTextArea(true)] public string Text = DefaultText;

            /// <summary>
            /// The audio file of this line.
            /// </summary>
            public AudioClip Audio = null;
            #endregion
        }

        #region Global Members
        public override string Text {
            get { return Line.Text; }
            set { Line.Text = value; }
        }

        /// <summary>
        /// The duration (in seconds) of this line.
        /// </summary>
        public virtual float Duration {
            get {
                if (ReferenceEquals(Line.Audio, null)) {
                    return Text.Length;
                }

                return Line.Audio.length;
            }
        }
        #endregion
    }

    #if LOCALIZATION_ENABLED
    /// <summary>
    /// <see cref="ConversationLine{T}"/> node class with a localized text and an associated localized audio file.
    /// </summary>
    [Serializable, DisplayName("Localized Line")]
    public class ConversationLocalizedLine : ConversationLine<ConversationLocalizedLine.Content> {
        /// <summary>
        /// Wrapper for the <see cref="ConversationLocalizedLine"/> node line content.
        /// </summary>
        [Serializable]
        public class Content {
            #region Content
            /// <summary>
            /// The localized text of this line.
            /// </summary>
            public LocalizedString Text = new LocalizedString();

            /// <summary>
            /// The localized audio file of this line.
            /// </summary>
            public LocalizedAsset<AudioClip> Audio = new LocalizedAsset<AudioClip>();
            #endregion
        }

        #region Global Members
        public override string Text {
            get { return Line.Text.GetLocalizedValue(); }
            set { Line.Text.SetLocalizedValue(value); }
        }

        /// <summary>
        /// The duration (in seconds) of this line.
        /// </summary>
        public virtual float Duration {
            get {
                AudioClip _audio = Line.Audio.LoadAsset();
                if (ReferenceEquals(_audio, null)) {
                    return Text.Length;
                }

                return _audio.length;
            }
        }
        #endregion
    }
    #endif
}