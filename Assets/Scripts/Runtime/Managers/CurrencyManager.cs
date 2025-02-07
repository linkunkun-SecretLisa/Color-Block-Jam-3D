using Runtime.Extensions;
using UnityEngine;  

namespace Runtime.Managers
{
    public class CurrencyManager : SingletonMonoBehaviour<CurrencyManager>
    {
        private int coinAmount;
        
        public int GetCoinAmount() => coinAmount;
        protected override void Awake()
        {
            base.Awake();
            
            coinAmount = PlayerPrefs.GetInt(PlayerPrefsKeys.CoinsInt);
        }
        
        public void IncressCoinAmount()
        {
            coinAmount += 100;
            PlayerPrefs.SetInt(PlayerPrefsKeys.CoinsInt, coinAmount);
            UIManager.Instance.UpdateCoinText();
        }
    }
}