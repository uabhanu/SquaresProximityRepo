using System.Collections;
using UnityEngine;

namespace Interfaces
{
    public interface IAIManager
    {
        public IEnumerator AnimateCoinEffect(SpriteRenderer coinRenderer , Color? capturingColor = null);
        public IEnumerator AIPlaceCoinCoroutine();
        public Vector2Int FindCellToPlaceCoinOn();
    }
}