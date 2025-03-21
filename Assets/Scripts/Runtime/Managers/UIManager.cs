using UnityEngine;
using DG.Tweening;
using Runtime.Utilities;
using TMPro;
using UnityEngine.UI;

namespace Runtime.Managers
{
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        [Header("Panels")] 
        [SerializeField] private GameObject startPanel;

        [Header("Buttons")] 
        [SerializeField] private Button restartLevelButton;
        [SerializeField] private Button coinButton;

        [Header("Texts")] 
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] public TextMeshProUGUI timerText;
        
        [Header("Private's")] 
        private Tween coinTween;

        [SerializeField] private LevelManager levelManager;

        private void Start()
        {
            if (levelManager == null)
            {
                levelManager = FindObjectOfType<LevelManager>();
            }
            
            CheckRemoteConfig();
            AddListeners();
            UpdateCoinText();
            levelText.text = $"LEVEL \n {PlayerPrefs.GetInt(PlayerPrefsKeys.FakeLevelIndexInt).ToString()}";
        }

        private void CheckRemoteConfig()
        {
            if (!RemoteConfigDummy.hasTimer)
            {
                timerText.gameObject.SetActive(false);
            }
            else if (RemoteConfigDummy.hasTimer)
            {
                timerText.gameObject.SetActive(true);
            }
        }

        public void UpdateCoinText()
        {
            coinText.text = CurrencyManager.Instance.GetCoinAmount().ToString();
        }


        #region Buttons

        private void AddListeners()
        {
            coinButton.onClick.AddListener(OnCoinButtonClicked);
            restartLevelButton.onClick.AddListener(OnRestartLevelButtonClicked);
        }

        private void OnCoinButtonClicked()
        {
            coinTween?.Kill();
            coinButton.transform.localScale = Vector3.one;
            coinTween = coinButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 10, 1);
        }

        private void OnRestartLevelButtonClicked()
        {
            levelManager.RestartLevel();
        }

        private void RemoveListeners()
        {
            coinButton.onClick.RemoveListener(OnCoinButtonClicked);
            restartLevelButton.onClick.RemoveListener(OnRestartLevelButtonClicked);
        }

        #endregion

        private void OnDisable()
        {
            RemoveListeners();
        }
    }
}