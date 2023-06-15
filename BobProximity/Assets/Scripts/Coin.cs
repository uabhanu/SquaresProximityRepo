using TMPro;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private PlayerController _playerController;
    private TMP_Text _coinValueLabelTMP;

    private void Start()
    {
        _coinValueLabelTMP = GetComponentInChildren<TMP_Text>();
        _playerController = FindObjectOfType<PlayerController>();
        _coinValueLabelTMP.text = _playerController.CoinValue.ToString();
    }
}