using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Runtime.Utilities;
using Runtime.Data.ValueObject;
using Runtime.Data.UnityObject;

public class LevelManager : SingletonMonoBehaviour<LevelManager>
{
    [Header("关卡配置")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxLevel = 3;
    [SerializeField] private bool useRandomLevels = false;
    [SerializeField] private bool useCustomLevels = false;
    [SerializeField] private List<int> customLevelSequence = new List<int>();
    [Header("关卡数据")]
    [SerializeField] private LevelData currentLevelData;
    [SerializeField] private GameObject levelGeneratorObject;
    [SerializeField] private CD_GameColor colorDataAsset;
    [SerializeField] private CD_GamePrefab itemPrefabAsset;
    [HideInInspector][SerializeField] private float gridSpacing = 1f;

    // 添加当前关卡索引属性
    public int CurrentLevelIndex => currentLevel;

    private MonoBehaviour levelGenerator;

    protected override void Awake()
    {
        base.Awake();
        // 初始化关卡生成器
        if (levelGeneratorObject == null)
        {
            levelGeneratorObject = GameObject.FindObjectOfType<MonoBehaviour>()?.gameObject;
            if (levelGeneratorObject == null)
            {
                levelGeneratorObject = new GameObject("LevelGenerator");
            }
        }
        
        // 获取或添加生成器组件
        levelGenerator = levelGeneratorObject.GetComponent("RuntimeLevelGenerator") as MonoBehaviour;
        if (levelGenerator == null)
        {
            Debug.LogError("无法找到RuntimeLevelGenerator组件，请确保已正确引用");
        }
    }

    private void Start()
    {
        LoadCurrentLevel();
    }

    private void LoadCurrentLevel()
    {
        // 根据设置加载关卡数据
        // 使用LevelCreator加载关卡
        LoadLevelFromCreator();
    }

    private void LoadLevelFromCreator()
    {
        Debug.Log("LoadLevelFromCreator: " + currentLevel);
        // 从LevelCreator加载关卡数据
        string levelPath = $"Data/Levels/LevelData_{currentLevel}";    
        CD_LevelData levelData = Resources.Load<CD_LevelData>(levelPath);
        if (levelData != null)
        {
            currentLevelData = levelData.levelData;
            GenerateLevel();
        }
        else
        {
            Debug.LogError($"无法加载关卡数据: {levelPath}");
        }
    }

    private void GenerateLevel()
    {
        if (currentLevelData == null)
        {
            Debug.LogError("没有关卡数据可以生成");
            return;
        }

        // 使用RuntimeLevelGenerator生成关卡
        if (levelGenerator != null)
        {
            try
            {
                // 直接使用反射调用GenerateLevel方法  //todo:lkk  refact this
                System.Type type = levelGenerator.GetType();
                System.Reflection.MethodInfo methodInfo = type.GetMethod("GenerateLevel");
                
                if (methodInfo != null)
                {
                    methodInfo.Invoke(levelGenerator, new object[] { currentLevelData, colorDataAsset, itemPrefabAsset, gridSpacing });
                    Debug.Log("成功调用GenerateLevel方法");
                }
                else
                {
                    Debug.LogError("无法找到GenerateLevel方法");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("调用GenerateLevel方法失败: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("找不到RuntimeLevelGenerator组件");
        }
    }

    public void RestartLevel()
    {
        GenerateLevel();
    }

    public void NextLevel()
    {
        if (useRandomLevels)
        {
            currentLevel = Random.Range(1, maxLevel + 1);
        }
        else if (useCustomLevels && customLevelSequence.Count > 0)
        {
            int currentIndex = customLevelSequence.IndexOf(currentLevel);
            if (currentIndex < customLevelSequence.Count - 1)
            {
                currentLevel = customLevelSequence[currentIndex + 1];
            }
            else
            {
                currentLevel = customLevelSequence[0];
            }
        }
        else
        {
            currentLevel = (currentLevel % maxLevel) + 1;
        }

        LoadCurrentLevel();
    }

    public void PreviousLevel()
    {
        if (useCustomLevels && customLevelSequence.Count > 0)
        {
            int currentIndex = customLevelSequence.IndexOf(currentLevel);
            if (currentIndex > 0)
            {
                currentLevel = customLevelSequence[currentIndex - 1];
            }
            else
            {
                currentLevel = customLevelSequence[customLevelSequence.Count - 1];
            }
        }
        else
        {
            currentLevel = (currentLevel - 2 + maxLevel) % maxLevel + 1;
        }

        LoadCurrentLevel();
    }

    public void GoToLevel(int level)
    {
        if (level >= 1 && level <= maxLevel)
        {
            currentLevel = level;
            LoadCurrentLevel();
        }
        else{
            Debug.LogError("关卡索引超出范围");
        }
    }
}
