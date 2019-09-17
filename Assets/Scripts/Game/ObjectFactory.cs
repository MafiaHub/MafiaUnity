using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    public static class ObjectFactory
    {
        public static void SetUpPawnPhysics(GameObject go)
        {
            var rigidBody = go.AddComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            var collider = go.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, 1f, 0);
            collider.height = 2f;
        }

        public static GameObject CreatePlayer(string modelName)
        {
            var go = new GameObject("Main Player");
            var tommy = GameAPI.instance.modelGenerator.LoadObject(modelName, null);
            tommy.AddComponent<ModelAnimationPlayer>();
            tommy.transform.parent = go.transform;

            var playerController = go.AddComponent<PlayerController>();
            playerController.playerCamera = GameObject.Find("Main Camera");
            playerController.playerPawn = tommy;

            SetUpPawnPhysics(go);

            return go;
        }

        public static GameObject CreateIdlePawn(string objectName, string modelName)
        {
            var go = new GameObject(objectName);
            var pawn = GameAPI.instance.modelGenerator.LoadObject(modelName, null);
            pawn.AddComponent<ModelAnimationPlayer>();
            pawn.transform.parent = go.transform;

            var idlePawnController = go.AddComponent<IdlePawnController>();
            idlePawnController.pawn = pawn;

            SetUpPawnPhysics(go);

            return go;
        }
    }
}
