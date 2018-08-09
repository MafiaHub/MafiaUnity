using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MafiaUnity;
using System.IO;
using System;
using UnityEditor;

namespace MafiaUnity
{
    public class MafiaAnimation
    {
        public List<AnimationSequence> animationSequences;
    }

    public class ModelAnimationPlayer : MonoBehaviour
    {
        public bool isPlaying = false;
        public AnimationPlaybackMode playbackMode;
        public float blendBeginPercentage;
        public float blendEndPercentage;
        public float playbackCompletion;
        
        private Action onAnimationFinished = null;
        [SerializeField] public MafiaAnimation mafiaAnimation = new MafiaAnimation();

        [SerializeField] public MafiaAnimation pairAnimation;
        private const float frameStep = 1f / 25f;
        private float frameTime;

        public MafiaAnimation LoadAndSetAnimation(string animName)
        {
            mafiaAnimation = LoadAnimation(animName);
            return mafiaAnimation;
        }

        public MafiaAnimation LoadAnimation(string animName)
        {
            var animation = new MafiaAnimation();
            animation.animationSequences = new List<AnimationSequence>();

            Stream fs;

            try
            {
                fs = GameManager.instance.fileSystem.GetStreamFromPath(animName);
            }
            catch
            {
                return null;
            }

            using (var reader = new BinaryReader(fs))
            {
                Loader5DS animLoader = new Loader5DS();
                animLoader.load(reader);

                foreach (var seq in animLoader.sequences)
                {
                    var newAnimationSequence = new AnimationSequence();
                    newAnimationSequence.loaderSequence = seq;
                    animation.animationSequences.Add(newAnimationSequence);
                }
            }

            return animation;
        }

        public void SetAnimation(MafiaAnimation anim)
        {
            if (mafiaAnimation == anim)
                return;

            mafiaAnimation = anim;
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

            foreach (var animationSeq in mafiaAnimation.animationSequences)
                animationSeq.Reset();
        }

        public void BlendAnimation(MafiaAnimation anim)
        {
            if (anim == null || mafiaAnimation == null)
                return;

            pairAnimation = anim;
        }

        void Update()
        {
            if (!isPlaying)
                return;

            if (mafiaAnimation == null || mafiaAnimation.animationSequences == null)
                return;

            if (IsFinished())
            {
                if (mafiaAnimation != null && onAnimationFinished != null)
                    onAnimationFinished.Invoke();

                if (playbackMode == AnimationPlaybackMode.Repeat)
                    AnimReset();
                else if (playbackMode == AnimationPlaybackMode.Once)
                {
                    isPlaying = false;
                    return;
                }
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
        public Loader5DS.AnimationSequence loaderSequence;
        public int positionKeyFrameId;
        public int rotationKeyFrameId;
        public int scaleKeyFrameId;

        public void Reset()
        {
            positionKeyFrameId = 0;
            rotationKeyFrameId = 0;
            scaleKeyFrameId = 0;
        }

        public void NextFrame()
        {
            if (loaderSequence.hasMovement() && positionKeyFrameId + 1 < loaderSequence.positions.Count)
                positionKeyFrameId++;

            if (loaderSequence.hasRotation() && rotationKeyFrameId + 1 < loaderSequence.rotations.Count)
                rotationKeyFrameId++;

            if (loaderSequence.hasScale() && scaleKeyFrameId + 1 < loaderSequence.scales.Count)
                scaleKeyFrameId++;
        }

        public bool IsFinished()
        {
            if (loaderSequence == null)
                return true;

            bool movementResult = true;
            if (loaderSequence.positionFrames.Count > 0)
                movementResult = (positionKeyFrameId + 1 == loaderSequence.positionFrames.Count);

            bool rotationResult = true;
            if (loaderSequence.rotationFrames.Count > 0)
                rotationResult = (rotationKeyFrameId + 1 == loaderSequence.rotationFrames.Count);

            bool scaleResult = true;
            if (loaderSequence.scaleFrames.Count > 0)
                scaleResult = (scaleKeyFrameId + 1 == loaderSequence.scaleFrames.Count);

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

        public void Update(float deltaLerp, Transform rootObject)
        {
            if (loaderSequence == null)
                return;

            var boneTransform = rootObject.FindDeepChild(loaderSequence.objectName);

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

                    var anim = animPlayer.LoadAndSetAnimation("anims/" + animName + ".5ds");
                    animPlayer.AnimReset();
                    animPlayer.isPlaying = true;
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Reset Animation"))
            {
                var animPlayer = target as ModelAnimationPlayer;

                animPlayer.AnimReset();
            }
        }
    }
#endif
}