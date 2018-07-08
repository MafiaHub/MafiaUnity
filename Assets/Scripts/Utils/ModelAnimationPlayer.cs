using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MafiaUnity;
using System.IO;
using System;
using UnityEditor;

namespace MafiaUnity
{
    public class ModelAnimationPlayer : MonoBehaviour
    {
        public bool isPlaying = false;
        public AnimationPlaybackMode playbackMode;
        
        private int[] posFrameId;
        private int[] rotFrameId;
        private int[] scaleFrameId;
        private const float frameStep = 1f / 25f;
        private float frameTime;
        [SerializeField] private List<AnimationSequence> animationSequences;

        [Serializable]
        private class AnimationSequence
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
                if (loaderSequence.hasMovement() && positionKeyFrameId + 2 < loaderSequence.positions.Count)
                    positionKeyFrameId++;

                if (loaderSequence.hasRotation() && rotationKeyFrameId + 2 < loaderSequence.rotations.Count)
                    rotationKeyFrameId++;

                if (loaderSequence.hasScale() && scaleKeyFrameId + 2 < loaderSequence.scales.Count)
                    scaleKeyFrameId++;
            }

            public bool IsFinished()
            {
                if (loaderSequence == null)
                    return true;

                bool movementResult = true;
                if (loaderSequence.positionsFrames.Count > 0)
                    movementResult = (positionKeyFrameId + 2 == loaderSequence.positionsFrames.Count);

                bool rotationResult = true;
                if (loaderSequence.rotationFrames.Count > 0)
                    rotationResult = (rotationKeyFrameId + 2 == loaderSequence.rotationFrames.Count);

                bool scaleResult = true;
                if (loaderSequence.scalesFrames.Count > 0)
                    scaleResult = (scaleKeyFrameId + 2 == loaderSequence.scalesFrames.Count);

                return movementResult && rotationResult && scaleResult;
            }

            public void Update(float deltaLerp, Transform rootObject)
            {
                if (loaderSequence == null)
                    return;

                var boneTransform = rootObject.FindDeepChild(loaderSequence.objectName);

                if (boneTransform == null)
                    return;
                
                if (loaderSequence.hasMovement())
                {
                    boneTransform.localPosition = Vector3.Lerp(loaderSequence.positions[positionKeyFrameId],
                        loaderSequence.positions[positionKeyFrameId + 1], deltaLerp);
                }

                if (loaderSequence.hasRotation())
                {
                    var tmpRot = new Quaternion(loaderSequence.rotations[rotationKeyFrameId].y, 
                        loaderSequence.rotations[rotationKeyFrameId].z, 
                        loaderSequence.rotations[rotationKeyFrameId].w, 
                        -1 * loaderSequence.rotations[rotationKeyFrameId].x);

                    var tmpRot2 = new Quaternion(loaderSequence.rotations[rotationKeyFrameId + 1].y, 
                        loaderSequence.rotations[rotationKeyFrameId + 1].z, 
                        loaderSequence.rotations[rotationKeyFrameId + 1].w, 
                        -1 * loaderSequence.rotations[rotationKeyFrameId + 1].x);

                    boneTransform.localRotation = Quaternion.Slerp(tmpRot, tmpRot2, deltaLerp);
                }

                if (loaderSequence.hasScale())
                {
                    boneTransform.localScale = Vector3.Lerp(loaderSequence.scales[scaleKeyFrameId],
                        loaderSequence.scales[scaleKeyFrameId + 1], deltaLerp);
                }
            }
        }

        public bool LoadAnimation(string animName)
        {
            AnimReset();
            animationSequences = new List<AnimationSequence>();

            FileStream fs;

            try
            {
                fs = new FileStream(GameManager.instance.fileSystem.GetPath(animName), FileMode.Open);
            }
            catch
            {
                return false;
            }

            using (var reader = new BinaryReader(fs))
            {
                Loader5DS animLoader = new Loader5DS();
                animLoader.load(reader);

                foreach (var seq in animLoader.sequences)
                {
                    var newAnimationSequence = new AnimationSequence();
                    newAnimationSequence.loaderSequence = seq;
                    animationSequences.Add(newAnimationSequence);
                }
            }

            return true;
        }
        
        public bool IsFinished()
        {
            if (animationSequences == null)
                return true;

            foreach (var animationSeq in animationSequences)
                if (!animationSeq.IsFinished()) return false;

            return true;
        }

        public void AnimReset()
        {
            if (animationSequences == null)
                return;

            foreach (var animationSeq in animationSequences)
                animationSeq.Reset();
        }

        void Update()
        {
            if (!isPlaying)
                return;

            if (IsFinished())
            {
                if (playbackMode == AnimationPlaybackMode.Repeat)
                    AnimReset();
                else if (playbackMode == AnimationPlaybackMode.Once)
                {
                    isPlaying = false;
                    return;
                }
            }

            float frameDelta = frameTime / frameStep;

            foreach (var animationSeq in animationSequences)
            {
                animationSeq.Update(frameDelta, transform);

                if (frameTime > frameStep)
                    animationSeq.NextFrame();
            }

            if(frameTime > frameStep)
                frameTime = 0f;

            frameTime += Time.deltaTime;
        }

        public enum AnimationPlaybackMode
        {
            Once,
            Repeat
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

                    animPlayer.LoadAnimation("anims/" + animName + ".5ds");
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