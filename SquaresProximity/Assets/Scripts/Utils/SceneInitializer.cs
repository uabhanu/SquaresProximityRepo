using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class SceneInitializer : MonoBehaviour
    {
        //Todo Create an interface IOnSceneLoadInit and use that instead of GameObject
        [SerializeField] private List<GameObject> sceneObjectsList;
        [SerializeField] private List<GameObject> uiObjectsList;
    
        private void Start()
        {
            foreach(var sceneObj in sceneObjectsList)
            {
                //Todo Use interface to spawn object and call Init() on them
                Instantiate(sceneObj);
            }
            
            #if UNITY_ANDROID || UNITY_IOS
                Instantiate(uiObjectsList[0]);
            #endif
            
            #if UNITY_STANDALONE || UNITY_WEBGL
                Instantiate(uiObjectsList[1]);
            #endif
        
            Destroy(gameObject);
        }
    }
}
