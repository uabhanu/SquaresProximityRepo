using UnityEngine;

namespace Interfaces
{
    public interface IColourAdjuster
    {
        public Color GetColor();
        public void SetColor(Color color);
    }
}