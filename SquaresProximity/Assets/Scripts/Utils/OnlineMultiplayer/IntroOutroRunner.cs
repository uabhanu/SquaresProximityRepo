namespace Utils.OnlineMultiplayer
{
    using System;
    using UnityEngine;
    
    public class IntroOutroRunner : MonoBehaviour
    {
        private Action _onIntroComplete;
        private Action _onOutroComplete;
        
        [SerializeField] Animator animator;
        [SerializeField] InGameRunner inGameRunner;


        public void DoIntro(Action onIntroComplete)
        {
            _onIntroComplete = onIntroComplete;
            animator.SetTrigger("DoIntro");
        }

        public void DoOutro(Action onOutroComplete)
        {
            _onOutroComplete = onOutroComplete;
            animator.SetTrigger("DoOutro");
        }
        
        public void OnIntroComplete()
        {
            _onIntroComplete?.Invoke();
        }
       
        public void OnOutroComplete()
        {
            _onOutroComplete?.Invoke();
        }
    }
}