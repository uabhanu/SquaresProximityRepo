using UnityEngine;

namespace Interfaces
{
    public interface IColourAdjuster
    {
        public Color GetColor();
        public Color GetContrastingColor(Color color);
        public void SetColor(Color color);
    }
}