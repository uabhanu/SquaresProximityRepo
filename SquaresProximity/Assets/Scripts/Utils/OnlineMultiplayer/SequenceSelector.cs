namespace Utils.OnlineMultiplayer
{
    using Data;
    using Random = UnityEngine.Random;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class SequenceSelector : NetworkBehaviour
    {
        private bool _canAnimateTargets = true;
        private bool _hasReceivedTargetSequence;
        private Dictionary<ulong , int> _targetSequenceIndexPerPlayerDictionary;
        private List<int> _fullSequenceServersList;
        private NetworkList<int> _targetSequencesNetworkList;
        private ulong _localId;
        
        [SerializeField] SymbolData symbolData;
        [SerializeField] Image[] targetSequenceOutputImagesArray;
        
        public const int SymbolCount = 140;

        public void Awake()
        {
            _targetSequencesNetworkList = new NetworkList<int>();
        }
        
        public void Update()
        {
            if(!_hasReceivedTargetSequence && _targetSequencesNetworkList.Count > 0)
            {
                for(int n = 0; n < _targetSequencesNetworkList.Count; n++) targetSequenceOutputImagesArray[n].sprite = symbolData.GetSymbolForIndex(_targetSequencesNetworkList[n]);
                
                _hasReceivedTargetSequence = true;
                ScaleTargetUi(_localId , 0);
            }
        }

        public override void OnNetworkSpawn()
        {
            if(IsHost) ChooseSymbols();
            
            _targetSequencesNetworkList.ResetDirty();
            _localId = NetworkManager.Singleton.LocalClientId;
            AddClient_ServerRpc(_localId);
        }

        private void ChooseSymbols()
        {
            int numSymbolTypes = 8;
            
            List<int> symbolsForThisGame = SelectSymbols(symbolData.AvailableSymbols.Count , numSymbolTypes);
            
            _targetSequencesNetworkList.Add(symbolsForThisGame[0]);
            _targetSequencesNetworkList.Add(symbolsForThisGame[1]);
            _targetSequencesNetworkList.Add(symbolsForThisGame[2]);

            int numTargetSequences = (int)(SymbolCount * 2 / 3f) / 3;
            
            for(; numTargetSequences >= 0; numTargetSequences--)
            {
                _fullSequenceServersList.Add(_targetSequencesNetworkList[2]);
                _fullSequenceServersList.Add(_targetSequencesNetworkList[1]);
                _fullSequenceServersList.Add(_targetSequencesNetworkList[0]);
            }
            
            for(int n = 3; n < numSymbolTypes - 1; n++) AddHalfRemaining(n , 2);
            
            AddHalfRemaining(numSymbolTypes - 1 , 1);
            return;

            void AddHalfRemaining(int symbolIndex , int divider)
            {
                int remaining = SymbolCount - _fullSequenceServersList.Count;
                
                for(int n = 0; n < remaining / divider; n++)
                {
                    int randomIndex = UnityEngine.Random.Range(0 , _fullSequenceServersList.Count);
                    _fullSequenceServersList.Insert(randomIndex , symbolsForThisGame[symbolIndex]);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void AddClient_ServerRpc(ulong id)
        {
            _targetSequenceIndexPerPlayerDictionary.Add(id , 0);
        }
        
        private static List<int> SelectSymbols(int numOptions , int targetCount)
        {
            List<int> list = new List<int>();
            
            for(int n = 0; n < targetCount; n++) list.Add(Random.Range(0 , numOptions));
            
            return list;
        }
        
        public bool ConfirmSymbolCorrect(ulong id , int symbolIndex)
        {
            int index = _targetSequenceIndexPerPlayerDictionary[id];
            
            if(symbolIndex != _targetSequencesNetworkList[index]) return false;
            
            if(++index >= _targetSequencesNetworkList.Count) index = 0;
            
            _targetSequenceIndexPerPlayerDictionary[id] = index;

            ScaleTargetUi_ClientRpc(id , index);
            
            return true;
        }

        [ClientRpc]
        private void ScaleTargetUi_ClientRpc(ulong id , int sequenceIndex)
        {
            ScaleTargetUi(id , sequenceIndex);
        }

        private void ScaleTargetUi(ulong id , int sequenceIndex)
        {
            if (NetworkManager.Singleton.LocalClientId == id)
                for (int i = 0; i < targetSequenceOutputImagesArray.Length; i++)
                    targetSequenceOutputImagesArray[i].transform.localScale =
                        Vector3.one * (sequenceIndex == i || !_canAnimateTargets ? 1 : 0.7f);
        }

        public int GetNextSymbol(int symbolObjectIndex)
        {
            return _fullSequenceServersList[symbolObjectIndex];
        }

        public void SetTargetsAnimatable()
        {
            _canAnimateTargets = true;
            ScaleTargetUi(_localId , 0);
        }
        
        private struct RectCut
        {
            public Rect rect;
            
            public int cutIndex;

            public bool isVertCut => cutIndex % 3 == 2;

            public RectCut(Rect rect , int cutIndex)
            {
                this.rect = rect;
                this.cutIndex = cutIndex;
            }

            public RectCut(float xMin , float xMax , float yMin , float yMax , int cutIndex)
            {
                rect = new Rect(xMin , yMin , xMax - xMin , yMax - yMin);
                this.cutIndex = cutIndex;
            }
        }
        
        public List<Vector2> GenerateRandomSpawnPoints(Rect bounds , float extent , int count = SymbolCount)
        {
            int numTries = 3;
            List<Vector2> points = new List<Vector2>();
            
            while(numTries > 0)
            {
                Queue<RectCut> rects = new Queue<RectCut>();
                points.Clear();
                rects.Enqueue(new RectCut(bounds , -1));
                
                while(rects.Count + points.Count < count && rects.Count > 0)
                {
                    RectCut currRect = rects.Dequeue();
                    
                    bool isLargeEnough = (currRect.isVertCut && currRect.rect.width > extent * 2) || (!currRect.isVertCut && currRect.rect.height > extent * 2);
                    
                    if(!isLargeEnough)
                    {
                        points.Add(currRect.rect.center);
                        continue;
                    }

                    float xMin = currRect.rect.xMin , xMax = currRect.rect.xMax , yMin = currRect.rect.yMin , yMax = currRect.rect.yMax;
                    
                    if(currRect.isVertCut)
                    {
                        float cutPosX = Random.Range(xMin + extent , xMax - extent);
                        rects.Enqueue(new RectCut(xMin , cutPosX , yMin , yMax , currRect.cutIndex + 1));
                        rects.Enqueue(new RectCut(cutPosX , xMax , yMin , yMax , currRect.cutIndex + 1));
                    }
                    else
                    {
                        float cutPosY = Random.Range(yMin + extent , yMax - extent);
                        rects.Enqueue(new RectCut(xMin , xMax , yMin , cutPosY , currRect.cutIndex + 1));
                        rects.Enqueue(new RectCut(xMin , xMax , cutPosY , yMax , currRect.cutIndex + 1));
                    }
                }

                while(rects.Count > 0) points.Add(rects.Dequeue().rect.center);

                if(points.Count >= count) return points;
                
                numTries--;
            }

            Debug.LogError("Failed to generate symbol spawn points. Defaulting to a simple grid of points.");
            points.Clear();
            int numPerLine = Mathf.CeilToInt(bounds.width / (extent * 1.5f));
            
            for(int n = 0; n < count; n++) points.Add(new Vector2(Mathf.Lerp(bounds.xMin , bounds.xMax , (n % numPerLine) / (numPerLine - 1f)) , n / numPerLine * extent * 1.5f));
            
            return points;
        }
    }
}