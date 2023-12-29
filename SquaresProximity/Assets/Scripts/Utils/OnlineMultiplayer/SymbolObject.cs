namespace Utils.OnlineMultiplayer
{
    using Data;
    using System.Collections;
    using Unity.Netcode;
    using UnityEngine;
    
    public class SymbolObject : NetworkBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] SymbolData symbolData;
        public bool Clicked { get; private set; }
        public int SymbolIndex { get; private set; }
        
        private IEnumerator HideSymbolAnimDelay()
        {
            yield return new WaitForSeconds(0.3f);
            HideSymbol_ServerRpc();
        }
        
        [ClientRpc]
        private void SetPosition_ClientRpc(Vector3 newPosition)
        {
            transform.localPosition = newPosition;
        }
        
        private void SetSymbolSprite(int symbolIndex)
        {
            SymbolIndex = symbolIndex;
            spriteRenderer.sprite = symbolData.GetSymbolForIndex(SymbolIndex);
        }
        
        [ClientRpc]
        public void Clicked_ClientRpc(ulong clickerPlayerId)
        {
            if(this.NetworkManager.LocalClientId == clickerPlayerId)
                animator.SetTrigger("iClicked");
            else
            {
                animator.SetTrigger("theyClicked");
            }
        }
        
        [ServerRpc]
        public void ClickedSequence_ServerRpc(ulong clickerPlayerId)
        {
            Clicked = true;
            Clicked_ClientRpc(clickerPlayerId);
            StartCoroutine(HideSymbolAnimDelay());
        }
        
        [ServerRpc]
        public void HideSymbol_ServerRpc()
        {
            transform.localPosition += Vector3.forward * 500;
        }
        
        public void SetParentAndPosition_Server(NetworkObject parentObject , Vector3 newPosition)
        {
            NetworkObject.TrySetParent(parentObject, false);
            SetPosition_ClientRpc(newPosition);
        }
        
        [ClientRpc]
        public void SetSymbolIndex_ClientRpc(int symbolIndex)
        {
            SetSymbolSprite(symbolIndex);
        }

        public void SetSymbolIndex_Server(int symbolIndex)
        {
            SetSymbolSprite(symbolIndex);
            SetSymbolIndex_ClientRpc(symbolIndex);
        }
    }
}