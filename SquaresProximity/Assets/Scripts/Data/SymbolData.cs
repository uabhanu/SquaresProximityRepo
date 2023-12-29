namespace Data
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public class SymbolData : ScriptableObject
    {
        public int SymbolCount => AvailableSymbols.Count;
        public List<Sprite> AvailableSymbols;

        public Sprite GetSymbolForIndex(int index)
        {
            if(index < 0 || index >= SymbolCount) index = 0;
            
            return AvailableSymbols[index];
        }
    }
}