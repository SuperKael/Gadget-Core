using System;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// A simple-to-use component for animating a sprite sheet displayed by a <see cref="Renderer"/> on the same <see cref="GameObject"/>.
    /// Exhibits similar behavior to the built-in <see cref="AnimIcon"/> used by the base game for items with a tier value of 3,
    /// but is far more flexible than <see cref="AnimIcon"/>.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class SimpleSpriteAnimator : MonoBehaviour
    {
        /// <summary>
        /// The number of sprites in a row of the sprite sheet.
        /// </summary>
        public int SpriteSheetWidth
        {
            get => spriteSheetWidth;
            set
            {
                spriteSheetWidth = Math.Max(value, 1);
                frameCount = spriteSheetWidth * spriteSheetHeight;
                currentFrame %= frameCount;
                UpdateScale();
            }
        }
        private int spriteSheetWidth = 1;

        /// <summary>
        /// The number of sprites in a column of the sprite sheet.
        /// </summary>
        public int SpriteSheetHeight
        {
            get => spriteSheetHeight;
            set
            {
                spriteSheetHeight = Math.Max(value, 1);
                frameCount = spriteSheetWidth * spriteSheetHeight;
                currentFrame %= frameCount;
                UpdateScale();
            }
        }
        private int spriteSheetHeight = 1;

        /// <summary>
        /// The length in seconds of a single frame.
        /// </summary>
        public float FrameTime
        {
            get => frameTime;
            set
            {
                frameTime = value;
                ResetStartTime();
            }
        }
        private float frameTime = 1;

        /// <summary>
        /// The total number of frames in the sprite sheet. This will be set to SpriteSheetWidth * SpriteSheetHeight whenever either
        /// of those values change, but you can manually set it if the last row/column of the sprite sheet
        /// is not completely filled with sprites.
        /// </summary>
        public int FrameCount
        {
            get => frameCount;
            set
            {
                frameCount = Mathf.Clamp(value, 1, spriteSheetWidth * spriteSheetHeight);
                UpdateOffset();
            }
        }
        private int frameCount = 1;

        /// <summary>
        /// The sequence that sprites from the sprite sheet are displayed in.
        /// RowMajor will have the sprites from the first row displayed in order, then the second row, etc.
        /// ColumnMajor will have the sprites from the first column displayed in order, then the second column, etc.
        /// </summary>
        public SpriteSequence Sequence { get; set; } = SpriteSequence.RowMajor;

        /// <summary>
        /// The current frame of the sprite sheet animation that is displayed. You can assign to this
        /// to change the currently-displayed frame.
        /// </summary>
        public int CurrentFrame
        {
            get => currentFrame;
            set
            {
                currentFrame = value % FrameCount;
                ResetStartTime();
                UpdateOffset();
            }
        }
        private int currentFrame = 1;

        private Renderer renderer;
        private float startTime;
        private float pauseTime = -1;

        /// <summary>
        /// Initializes this animator with various settings for the animation. Note that the animator will still run
        /// even if you do not call this, this is just a convenience method to set these values in a single statement.
        /// </summary>
        /// <param name="frameTime">The length in seconds of a single frame.</param>
        /// <param name="spriteSheetWidth">The number of sprites in a row of the sprite sheet.</param>
        /// <param name="spriteSheetHeight">The number of sprites in a column of the sprite sheet.</param>
        /// <param name="frameCount">The total number of frames in the sprite sheet.</param>
        /// <param name="sequence">The sequence that sprites from the sprite sheet are displayed in.</param>
        public void Initialize(float frameTime, int spriteSheetWidth, int spriteSheetHeight = 1, int frameCount = -1, SpriteSequence sequence = SpriteSequence.RowMajor)
        {
            this.frameTime = frameTime;
            this.spriteSheetWidth = spriteSheetWidth;
            this.spriteSheetHeight = spriteSheetHeight;
            this.frameCount = frameCount > 0 ? frameCount : spriteSheetWidth * SpriteSheetHeight;
            Sequence = sequence;
        }

        /// <summary>
        /// Enables the animator. This is equivalent to setting the enabled property to true.
        /// </summary>
        public void Enable()
        {
            this.enabled = true;
        }

        /// <summary>
        /// Disables the animator, and restores the rendered material to a state appropriate for displaying
        /// a non-animated sprite. If you wish to pause the animation without restoring the
        /// renderer, use <see cref="Pause"/>.
        /// </summary>
        public void Disable()
        {
            renderer.material.mainTextureScale = new Vector2(1f, 1f);
            renderer.material.mainTextureOffset = new Vector2(0f, 0f);
            this.enabled = false;
        }

        /// <summary>
        /// Pauses the animator on its current frame. Use <see cref="Resume"/> to make it continue animating from where it stopped,
        /// or <see cref="Enable"/> to make it start the animation over again from the beginning.
        /// </summary>
        public void Pause()
        {
            pauseTime = Time.time - startTime;
            this.enabled = false;
        }

        /// <summary>
        /// Resumes the animation after having been previously paused with <see cref="Pause"/>. If it was not previously paused,
        /// this is equivalent to <see cref="Enable"/>
        /// </summary>
        public void Resume()
        {
            this.enabled = true;
            if (pauseTime >= 0)
            {
                startTime = Time.time - pauseTime;
                pauseTime = -1;
                UpdateOffset();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void Awake()
        {
            renderer = GetComponent<Renderer>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void OnEnable()
        {
            ResetStartTime();
            UpdateScale();
            UpdateOffset();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void Update()
        {
            int oldFrame = currentFrame;
            currentFrame = (int) ((Time.time - startTime) / FrameTime % FrameCount);
            if (currentFrame != oldFrame) UpdateOffset();
        }

        private void UpdateScale()
        {
            if (enabled)
                renderer.material.mainTextureScale = new Vector2(1f / SpriteSheetWidth, 1f / SpriteSheetHeight);
        }

        private void UpdateOffset()
        {
            if (enabled)
                renderer.material.mainTextureOffset = new Vector2(
                    (float)(Sequence == SpriteSequence.RowMajor ? currentFrame % SpriteSheetWidth : currentFrame / SpriteSheetHeight) / SpriteSheetWidth, 
                    (float)(Sequence == SpriteSequence.ColumnMajor ? currentFrame % SpriteSheetHeight : currentFrame / SpriteSheetWidth) / SpriteSheetHeight);
        }

        private void ResetStartTime()
        {
            startTime = Time.time - currentFrame * FrameTime;
        }

        /// <summary>
        /// The sequence that sprites from the sprite sheet are displayed in.
        /// RowMajor will have the sprites from the first row displayed in order, then the second row, etc.
        /// ColumnMajor will have the sprites from the first column displayed in order, then the second column, etc.
        /// </summary>
        public enum SpriteSequence
        {
            /// <summary>
            /// Will have the sprites from the first row displayed in order, then the second row, etc.
            /// </summary>
            RowMajor,
            /// <summary>
            /// Will have the sprites from the first column displayed in order, then the second column, etc.
            /// </summary>
            ColumnMajor
        }
    }
}
