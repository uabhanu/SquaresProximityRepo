using UnityEngine;

namespace Interfaces
{
    public interface ICoinPlacer
    {
        public void PlaceCoin(Vector2Int cellIndexAtMousePosition);
    }
}