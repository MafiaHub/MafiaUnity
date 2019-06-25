using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MafiaUnity;
using System.IO;
using System;
using UnityEditor;
using System.Text.RegularExpressions;

namespace MafiaUnity
{
    public class MafiaAnimation
    {
        public List<Vector3> positionOffsets;
        public List<AnimationSequence> animationSequences;

        /// <summary>
        /// How many frames to skip at the beginning.
        /// </summary>
        public int startFrame;

        /// <summary>
        /// How many frames to skip at the end, leading to cut out animation.
        /// Note: endFrame doesn't actually present absolute index when to end an animation, but
        /// an offset from the animation end at which we consider animation finished. (count - endFrame) == lastFrameIndex
        /// </summary>
        public int endFrame;

        /// <summary>
        /// If used to blend with current animation, this specifies the duration of blending.
        /// </summary>
        public float blendDuration;

        public MafiaAnimation(int startFrame, int endFrame, float blendDuration)
        {
            this.startFrame = startFrame;
            this.endFrame = endFrame;
            this.blendDuration = blendDuration;
        }

        public void Reset()
        {
            if (animationSequences == null)
                return;

            foreach (var animationSeq in animationSequences)
                animationSeq.Reset();
        }
    }

    public class ModelAnimationPlayer : MonoBehaviour
    {
        public bool isPlaying = false;
        public AnimationPlaybackMode playbackMode;
        public float playbackCompletion;
        
        private Action onAnimationFinished = null;
        [SerializeField] public MafiaAnimation mafiaAnimation;

        [SerializeField] public MafiaAnimation pairAnimation;
        private const float frameStep = 1f / 25f;
        private float frameTime;
        private float blendTime;

        private Vector3 objectInitialPosition;      // starting GameObject position, applies when .tck is used

        public MafiaAnimation LoadAndSetAnimation(string animName, int startFrame = 0, int endFrame = 0)
        {
            mafiaAnimation = LoadAnimation(animName, startFrame, endFrame);
            return mafiaAnimation;
        }

        public MafiaAnimation LoadAnimation(string animName, int startFrame=0, int endFrame=0, float blendDuration = 0.25f)
        {
            var animation = new MafiaAnimation(startFrame, endFrame, blendDuration);
            animation.animationSequences = new List<AnimationSequence>();
            animation.positionOffsets = new List<Vector3>();

            Stream tckfs;

            try
            {
                Regex rgx = new Regex("5ds");
                string tckFileName = animName.Replace("5ds", "tck");
                tckfs = GameAPI.instance.fileSystem.GetStreamFromPath(tckFileName);
                // if .tck exists, load it
                MafiaFormats.TckLoader tckFile = new MafiaFormats.TckLoader();
                using (var reader = new BinaryReader(tckfs))
                {
                    tckFile.load(reader);
                    foreach (var chunk in tckFile.transforms)
                    {
                        animation.positionOffsets.Prepend(chunk.position);
                    }
                }
            }
            catch
            {}

            Stream fs;

            try
            {
                fs = GameAPI.instance.fileSystem.GetStreamFromPath(animName);
            }
            catch
            {
                return null;
            }

            using (var reader = new BinaryReader(fs))
            {
                MafiaFormats.Loader5DS animLoader = new MafiaFormats.Loader5DS();
                animLoader.load(reader);

                foreach (var seq in animLoader.sequences)
                {
                    var newAnimationSequence = new AnimationSequence();
                    newAnimationSequence.loaderSequence = seq;
                    newAnimationSequence.rootAnimation = animation;
                    animation.animationSequences.Add(newAnimationSequence);
                }
            }

            return animation;
        }

        public void SetAnimation(MafiaAnimation anim, int startFrame = 0, int endFrame = 0)
        {
            if (mafiaAnimation == anim)
                return;

            if (anim != null)
                anim.Reset();

            mafiaAnimation = anim;
            mafiaAnimation.startFrame = startFrame;
            mafiaAnimation.endFrame = endFrame;

            // save the positon of object at the beggining of animation
            this.objectInitialPosition = gameObject.transform.parent.position;
        }
        
        public void OnAnimationFinish(Action finishAction)
        {
            if (finishAction != null)
                onAnimationFinished = finishAction;
        }

        public bool IsFinished()
        {
            if (mafiaAnimation == null || mafiaAnimation.animationSequences == null)
                return true;

            foreach (var animationSeq in mafiaAnimation.animationSequences)
                if (!animationSeq.IsFinished()) return false;

            return true;
        }

        public void AnimReset()
        {
            if (mafiaAnimation == null || mafiaAnimation.animationSequences == null)
                return;

            mafiaAnimation.Reset();
        }

        public void BlendAnimation(MafiaAnimation anim, int startFrame = 0, int endFrame = 0, float blendDuration = 0.25f)
        {
            if (anim == null)
                return;

            if (mafiaAnimation == anim)
                return;

            anim.Reset();

            if (mafiaAnimation == null)
            {
                mafiaAnimation = anim;
                mafiaAnimation.startFrame = startFrame;
                mafiaAnimation.endFrame = endFrame;
                return;
            }

            pairAnimation = anim;
            pairAnimation.blendDuration = blendDuration;
            pairAnimation.startFrame = startFrame;
            pairAnimation.endFrame = endFrame;
        }

        public void FinishAnimation()
        {
            if (mafiaAnimation != null && onAnimationFinished != null)
                onAnimationFinished.Invoke();

            if (playbackMode == AnimationPlaybackMode.Repeat)
                AnimReset();
            else if (playbackMode == AnimationPlaybackMode.Once)
                isPlaying = false;
        }

        void FixedUpdate()
        {
            if (GameAPI.instance.isPaused)
                return;
                
            if (!isPlaying)
                return;

            if (mafiaAnimation == null || mafiaAnimation.animationSequences == null)
                return;

            if (pairAnimation != null && pairAnimation.blendDuration > 0f && mafiaAnimation != pairAnimation && blendTime == 0f)
            {
                blendTime = pairAnimation.blendDuration;
            }

            if (blendTime > 0f && pairAnimation != null && pairAnimation.blendDuration > 0f)
            {
                foreach (var seq in pairAnimation.animationSequences)
                {
                    var bone = seq.FetchBoneTransform(transform);

                    if (bone == null)
                        continue;

                    var primarySeq = mafiaAnimation.animationSequences.Find(x => x.loaderSequence.objectName == seq.loaderSequence.objectName);

                    if (primarySeq == null)
                        continue;

                    seq.Reset();

                    var primaryTr = primarySeq.GetCurrentMotion();
                    var secondaryTr = seq.GetCurrentMotion();

                    float blendDelta = 1f - blendTime / pairAnimation.blendDuration;

                    if (seq.loaderSequence.hasMovement())
                    {
                        bone.localPosition = Vector3.Lerp(primaryTr.currentPosition, secondaryTr.currentPosition, blendDelta);
                    }

                    if (seq.loaderSequence.hasRotation())
                    {
                        bone.localRotation = Quaternion.Slerp(primaryTr.currentRotation, secondaryTr.currentRotation, blendDelta);
                    }

                    if (seq.loaderSequence.hasScale())
                    {
                        bone.localScale = Vector3.Lerp(primaryTr.currentScale, secondaryTr.currentScale, blendDelta);
                    }
                }

                blendTime -= Time.deltaTime;

                if (blendTime <= 0f)
                {
                    blendTime = 0f;
                    mafiaAnimation = pairAnimation;
                    pairAnimation = null;
                    AnimReset();
                }
                else return;
            }

            if (IsFinished())
            {
                FinishAnimation();

                if (playbackMode == AnimationPlaybackMode.Once)
                    return;
            }

            float frameDelta = frameTime / frameStep;

            foreach (var animationSeq in mafiaAnimation.animationSequences)
            {
                if (animationSeq == null)
                    return;

                animationSeq.Update(frameDelta, transform);

                if (frameTime > frameStep)
                    animationSeq.NextFrame();
            }
            
            if(frameTime > frameStep)
                frameTime = 0f;

            frameTime += Time.deltaTime;

            var currentFrameId = mafiaAnimation.animationSequences.Max(x => Mathf.Max(x.positionKeyFrameId, x.rotationKeyFrameId, x.scaleKeyFrameId));
            var lastFrameId = mafiaAnimation.animationSequences.Max(x => Mathf.Max(x.loaderSequence.positionFrames.Count, x.loaderSequence.rotationFrames.Count, x.loaderSequence.scaleFrames.Count));
            
            playbackCompletion = currentFrameId / (float)lastFrameId;
        }

        public enum AnimationPlaybackMode
        {
            Once,
            Repeat
        }
    }

    [Serializable]
    public class AnimationSequence
    {
        public MafiaFormats.Loader5DS.AnimationSequence loaderSequence;
        public int positionKeyFrameId;
        public int rotationKeyFrameId;
        public int scaleKeyFrameId;

        public MafiaAnimation rootAnimation;

        public void Reset()
        {
            int movementFrameSkip = (loaderSequence.positions.Count - rootAnimation.startFrame > 0) ? rootAnimation.startFrame : loaderSequence.positions.Count;
            int rotationFrameSkip = (loaderSequence.rotations.Count - rootAnimation.startFrame > 0) ? rootAnimation.startFrame : loaderSequence.rotations.Count;
            int scaleFrameSkip = (loaderSequence.scales.Count - rootAnimation.startFrame > 0) ? rootAnimation.startFrame : loaderSequence.scales.Count;

            positionKeyFrameId = movementFrameSkip;
            rotationKeyFrameId = rotationFrameSkip;
            scaleKeyFrameId = scaleFrameSkip;
        }

        public void NextFrame()
        {
            int movementFrameSkip = (loaderSequence.positions.Count - rootAnimation.endFrame > 0) ? rootAnimation.endFrame : loaderSequence.positions.Count;
            int rotationFrameSkip = (loaderSequence.rotations.Count - rootAnimation.endFrame > 0) ? rootAnimation.endFrame : loaderSequence.rotations.Count;
            int scaleFrameSkip = (loaderSequence.scales.Count - rootAnimation.endFrame > 0) ? rootAnimation.endFrame : loaderSequence.scales.Count;

            if (loaderSequence.hasMovement() && positionKeyFrameId + 1 + movementFrameSkip < loaderSequence.positions.Count)
                positionKeyFrameId++;

            if (loaderSequence.hasRotation() && rotationKeyFrameId + 1 + rotationFrameSkip < loaderSequence.rotations.Count)
                rotationKeyFrameId++;

            if (loaderSequence.hasScale() && scaleKeyFrameId + 1 + scaleFrameSkip < loaderSequence.scales.Count)
                scaleKeyFrameId++;
        }

        public bool IsFinished()
        {
            if (loaderSequence == null)
                return true;

            int movementFrameSkip = (loaderSequence.positions.Count - rootAnimation.endFrame > 0) ? rootAnimation.endFrame : loaderSequence.positions.Count;
            int rotationFrameSkip = (loaderSequence.rotations.Count - rootAnimation.endFrame > 0) ? rootAnimation.endFrame : loaderSequence.rotations.Count;
            int scaleFrameSkip = (loaderSequence.scales.Count - rootAnimation.endFrame > 0) ? rootAnimation.endFrame : loaderSequence.scales.Count;

            bool movementResult = true;
            if (loaderSequence.positionFrames.Count > 0)
                movementResult = (positionKeyFrameId + 1 + movementFrameSkip == loaderSequence.positionFrames.Count);

            bool rotationResult = true;
            if (loaderSequence.rotationFrames.Count > 0)
                rotationResult = (rotationKeyFrameId + 1 + rotationFrameSkip == loaderSequence.rotationFrames.Count);

            bool scaleResult = true;
            if (loaderSequence.scaleFrames.Count > 0)
                scaleResult = (scaleKeyFrameId + 1 + scaleFrameSkip == loaderSequence.scaleFrames.Count);

            return movementResult && rotationResult && scaleResult;
        }

        public class AnimationTransform
        {
            public Vector3 currentPosition;
            public Vector3 nextPosition;

            public Quaternion currentRotation;
            public Quaternion nextRotation;

            public Vector3 currentScale;
            public Vector3 nextScale;
        }

        public AnimationTransform GetCurrentMotion()
        {
            var motion = new AnimationTransform();

            if (loaderSequence.hasMovement() && loaderSequence.positions.Count > positionKeyFrameId + 1)
            {
                motion.currentPosition = loaderSequence.positions[positionKeyFrameId];
                motion.nextPosition = loaderSequence.positions[positionKeyFrameId + 1];
            }

            if (loaderSequence.hasRotation() && loaderSequence.rotations.Count > rotationKeyFrameId + 1)
            {
                motion.currentRotation = new Quaternion(loaderSequence.rotations[rotationKeyFrameId].y,
                        loaderSequence.rotations[rotationKeyFrameId].z,
                        loaderSequence.rotations[rotationKeyFrameId].w,
                        -1 * loaderSequence.rotations[rotationKeyFrameId].x);

                motion.nextRotation = new Quaternion(loaderSequence.rotations[rotationKeyFrameId + 1].y,
                    loaderSequence.rotations[rotationKeyFrameId + 1].z,
                    loaderSequence.rotations[rotationKeyFrameId + 1].w,
                    -1 * loaderSequence.rotations[rotationKeyFrameId + 1].x);
            }

            if (loaderSequence.hasScale() && loaderSequence.scales.Count > scaleKeyFrameId + 1)
            {
                motion.currentScale = loaderSequence.scales[scaleKeyFrameId];
                motion.nextScale = loaderSequence.scales[scaleKeyFrameId + 1];
            }

            return motion;
        }

        public Transform FetchBoneTransform(Transform rootObject)
        {
            if (loaderSequence == null)
                return null;

            Transform boneTransform = null;

            bool ok = boneTransforms.TryGetValue(loaderSequence.objectName, out boneTransform);

            if (!ok)
            {
                boneTransform = rootObject.FindDeepChild(loaderSequence.objectName);
                boneTransforms.Add(loaderSequence.objectName, boneTransform);
            }

            return boneTransform;
        }

        public void Update(float deltaLerp, Transform rootObject)
        {
            if (loaderSequence == null)
                return;

            Transform boneTransform = FetchBoneTransform(rootObject);

            if (boneTransform == null)
                return;

            var tr = GetCurrentMotion();

            if (loaderSequence.hasMovement())
            {
                boneTransform.localPosition = Vector3.Lerp(tr.currentPosition, tr.nextPosition, deltaLerp);
            }

            if (loaderSequence.hasRotation())
            {
                boneTransform.localRotation = Quaternion.Slerp(tr.currentRotation, tr.nextRotation, deltaLerp);
            }

            if (loaderSequence.hasScale())
            {
                boneTransform.localScale = Vector3.Lerp(tr.currentScale, tr.nextScale, deltaLerp);
            }
        }

        Dictionary<string, Transform> boneTransforms = new Dictionary<string, Transform>();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ModelAnimationPlayer))]
    public class ModelAnimationPlayerEditor : Editor
    {
        string animName = "!!!Skakani";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            {
                animName = GUILayout.TextField(animName);

                if (GUILayout.Button("Load Animation"))
                {
                    var animPlayer = target as ModelAnimationPlayer;

                    animPlayer.LoadAndSetAnimation("anims/" + animName + ".5ds");
                    animPlayer.AnimReset();
                    animPlayer.isPlaying = true;
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Load Pair Animation"))
            {
                var animPlayer = target as ModelAnimationPlayer;

                var anim = animPlayer.LoadAnimation("anims/" + animName + ".5ds");
                animPlayer.BlendAnimation(anim);
                animPlayer.isPlaying = true;
            }

            if (GUILayout.Button("Clear Pair Animation"))
            {
                var animPlayer = target as ModelAnimationPlayer;

                animPlayer.pairAnimation = null;
            }

            if (GUILayout.Button("Reset Animation"))
            {
                var animPlayer = target as ModelAnimationPlayer;

                animPlayer.AnimReset();
            }
        }
    }
#endif
}