using System;
using Runtime.Enums;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Managers
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        public GameStates GameStates { get; private set; }

        public void SetGameStateLevelComplete()
        {
            GameStates = GameStates.LevelComplete;
            Debug.Log("Level Completed :  " + LevelManager.Instance.CurrentLevelIndex) ;
            CurrencyManager.Instance.IncressCoinAmount();
            TimerManager.Instance.StopTimer();
            LevelManager.Instance.GoToLevel(LevelManager.Instance.CurrentLevelIndex + 1);
        }

        public void SetGameStateLevelFail()
        {
            GameStates = GameStates.LevelFail;
            Debug.Log("Level Failed");
        }
    }
}