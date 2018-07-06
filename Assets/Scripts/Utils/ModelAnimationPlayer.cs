using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenMafia;
using System.IO;
using System;

namespace OpenMafia
{
    public enum AnimationPlaybackMode
    {
        Once,
        Repeat
    }

    public class ModelAnimationPlayer : MonoBehaviour {

        private Dictionary<Loader5DS.AnimationSequence, GameObject> cachedSequences = new Dictionary<Loader5DS.AnimationSequence, GameObject>();
        private int[] posFrameId;
        private int[] rotFrameId;
        private int[] scaleFrameId;
        private const float frameStep = 1f / 25f;
        private float frameTime;
        public bool isPlaying = false;
        public AnimationPlaybackMode playbackMode;

        private class AnimationSequence
        {
            public GameObject boneGameObject;
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

            public void Update(float deltaLerp)
            {
                if (loaderSequence.hasMovement())
                {
                    boneGameObject.transform.localPosition = Vector3.Lerp(loaderSequence.positions[positionKeyFrameId],
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

                    boneGameObject.transform.localRotation = Quaternion.Slerp(tmpRot, tmpRot2, deltaLerp);
                }

                if (loaderSequence.hasScale())
                {
                    boneGameObject.transform.localScale = Vector3.Lerp(loaderSequence.scales[scaleKeyFrameId],
                        loaderSequence.scales[scaleKeyFrameId + 1], deltaLerp);
                }
            }
        }

        private List<AnimationSequence> animationSequeces = new List<AnimationSequence>();

        public bool LoadAnimation(string animName)
        {
            GameManager.instance.SetGamePath("D:/Mafia 1.2");
            FileStream fs;

            try
            {
                fs = new FileStream(GameManager.instance.fileSystem.GetCanonicalPath("anims/" + animName), FileMode.Open);
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
                    newAnimationSequence.boneGameObject = GameObject.Find(seq.objectName);
                    newAnimationSequence.loaderSequence = seq;
                    animationSequeces.Add(newAnimationSequence);
                }
            }

            return true;
        }

        void Start()
        {
            LoadAnimation("!!!Skakani.5DS");
        }


        bool IsFinished()
        {
            foreach (var animationSeq in animationSequeces)
                if (!animationSeq.IsFinished()) return false;

            return true;
        }

        void AnimReset()
        {
            foreach (var animationSeq in animationSequeces)
                animationSeq.Reset();
        }

        void Update()
        {
            if (IsFinished())
            {
                if (playbackMode == AnimationPlaybackMode.Repeat)
                    AnimReset();
                else if (playbackMode == AnimationPlaybackMode.Once)
                    return;
            }

            float deltaLerp = frameStep / frameTime;

            foreach (var animationSeq in animationSequeces)
            {
                animationSeq.Update(deltaLerp);

                if (frameTime > frameStep)
                    animationSeq.NextFrame();
            }

            if(frameTime > frameStep)
                frameTime = 0f;

            frameTime += Time.deltaTime;
        }
    }
}