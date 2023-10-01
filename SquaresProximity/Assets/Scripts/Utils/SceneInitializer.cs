using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class SceneInitializer : MonoBehaviour
    {
        [SerializeField] private List<GameObject> sceneObjectsList;
        [SerializeField] private List<GameObject> uiObjectsList;

        private void Start()
        {
            InstantiateSceneObjects();
            InstantiateUIObjects();
            Destroy(gameObject);
        }

        private void InstantiateSceneObjects()
        {
            foreach(var sceneObj in sceneObjectsList)
            {
                Instantiate(sceneObj);
            }
        }

        private void InstantiateUIObjects()
        {
            #if UNITY_ANDROID || UNITY_IOS
                Instantiate(uiObjectsList[0]);
            #endif

            #if UNITY_STANDALONE || UNITY_WEBGL
                Instantiate(uiObjectsList[1]);
            #endif
        }
    }
}