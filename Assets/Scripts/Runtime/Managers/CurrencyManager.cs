using Runtime.Utilities;
using UnityEngine;  

namespace Runtime.Managers
{
    public class CurrencyManager : SingletonMonoBehaviour<CurrencyManager>
    {
        private int _coinAmount;
        
        public int GetCoinAmount() => _coinAmount;
        protected override void Awake()
        {
            base.Awake();
            
            _coinAmount = PlayerPrefs.GetInt(PlayerPrefsKeys.CoinsInt);
        }
        
        public void IncressCoinAmount()
        {
            _coinAmount += 100;
            PlayerPrefs.SetInt(PlayerPrefsKeys.CoinsInt, _coinAmount);
            UIManager.Instance.UpdateCoinText();
        }
    }
}