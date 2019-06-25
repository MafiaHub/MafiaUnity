using UnityEngine;

namespace MafiaUnity
{
    // Spawns player to pre-defined point. Make sure you set game path to use it.
    // This is a helper script and should be only used in editor for dev purposes!
    class PlayerSpawner : MonoBehaviour
    {
        void Update() 
        {
            if (!GameAPI.instance.GetInitialized())
                return;

            gameObject.name = "Main Player";

            var tommy = GameAPI.instance.modelGenerator.LoadObject("models/Tommy.4ds", null);
            tommy.AddComponent<ModelAnimationPlayer>();
            tommy.transform.parent = transform;
            tommy.transform.localPosition = Vector3.zero;

            var playerController = gameObject.AddComponent<PlayerController>();
            playerController.playerCamera = GameObject.Find("Main Camera");
            playerController.playerPawn = tommy;

            ObjectFactory.SetUpPawnPhysics(tommy);

            GameObject.DestroyImmediate(this);
        }
    }
}