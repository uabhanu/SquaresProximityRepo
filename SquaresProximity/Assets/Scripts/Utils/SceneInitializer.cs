using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class SceneInitializer : MonoBehaviour
    {
        //Todo Create an interface IOnSceneLoadInit and use that instead of GameObject
        [SerializeField] private List<GameObject> sceneObjectsList;
    
        private void Start()
        {
            foreach(var sceneObj in sceneObjectsList)
            {
                //Todo Use interface to spawn object and call Init() on them
                Instantiate(sceneObj);
            }
        
            Destroy(gameObject);
        }
    }
}
