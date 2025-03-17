using Runtime.Utilities;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// 关卡创建器的自定义编辑器
    /// 提供可视化界面用于创建和编辑游戏关卡
    /// </summary>
    [CustomEditor(typeof(LevelCreatorScript))]
    public class LevelCreatorEditor : UnityEditor.Editor
    {
        // 控制是否绘制网格的标志
        private bool drawGrid = true;

        /// <summary>
        /// 重写OnInspectorGUI方法，自定义Inspector界面
        /// </summary>
        public override void OnInspectorGUI()
        {
            // 获取当前编辑的LevelCreatorScript实例
            LevelCreatorScript levelCreatorScript = (LevelCreatorScript)target;

            // 创建标题样式
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.UpperLeft
            };

            // 绘制默认Inspector内容
            DrawDefaultInspector();
            // 显示提示信息
            EditorGUILayout.HelpBox("If you are going to change the width and height, first do Undraw Grid and then load grid again", MessageType.Info);
            EditorGUILayout.Space();

            // 生成关卡按钮
            if (GUILayout.Button("Generate Level"))
            {
                levelCreatorScript.GenerateLevelData();
            }

            // 保存/加载/重置网格区域标题
            EditorGUILayout.LabelField("Save/Load/Reset Grid", titleStyle);

            // 保存提醒
            EditorGUILayout.HelpBox("Don't Forget Save", MessageType.Warning);
            // 开始水平布局
            EditorGUILayout.BeginHorizontal();

            // 保存网格按钮
            if (GUILayout.Button("Save Grid"))
            {
                levelCreatorScript.SaveLevelData();
            }

            // 加载网格按钮
            if (GUILayout.Button("Load Grid"))
            {
                levelCreatorScript.LoadLevelData();
                drawGrid = true; // 加载后显示网格
            }

            // 结束水平布局
            EditorGUILayout.EndHorizontal();

            // 重置网格数据按钮
            if (GUILayout.Button("Reset Grid Data"))
            {
                levelCreatorScript.ResetGridData();
            }

            // 隐藏网格按钮
            if (GUILayout.Button("Undraw Grid"))
            {
                drawGrid = false;
            }

            // 网格区域标题
            EditorGUILayout.LabelField("Grid", titleStyle);

            // 如果需要绘制网格且关卡数据有效，则绘制网格
            if (drawGrid && levelCreatorScript.GetCurrentLevelData() != null && levelCreatorScript.GetCurrentLevelData().Grids != null)
            {
                DrawGrid(levelCreatorScript);
            }
        }

        /// <summary>
        /// 绘制可交互的网格界面
        /// </summary>
        /// <param name="levelCreatorScript">关卡创建器脚本实例</param>
        private void DrawGrid(LevelCreatorScript levelCreatorScript)
        {
            // 获取行数和列数
            int rows = levelCreatorScript.GetRows();
            int columns = levelCreatorScript.GetColumns();

            // 从上到下绘制网格（y轴反向遍历，使网格顶部对应y值较大的单元格）
            for (int y = rows - 1; y >= 0; y--)
            {
                // 开始水平布局
                EditorGUILayout.BeginHorizontal();
                // 添加弹性空间使网格居中
                GUILayout.FlexibleSpace();

                // 从左到右绘制每一行的单元格
                for (int x = 0; x < columns; x++)
                {
                    // 设置单元格颜色
                    GUI.color = levelCreatorScript.GetGridColor(new Vector2Int(x, y));

                    // 创建可点击的按钮，显示坐标，并设置大小
                    if (GUILayout.Button($"{x}x{y}", GUILayout.Width(levelCreatorScript.gridSize), GUILayout.Height(levelCreatorScript.gridSize)))
                    {
                        // 点击时切换单元格占用状态
                        levelCreatorScript.ToggleGridOccupancy(x, y);
                        // 更新单元格颜色
                        levelCreatorScript.SetGridColor(x, y);
                    }

                    // 单元格之间添加间距
                    GUILayout.Space(5);
                }

                // 添加弹性空间使网格居中
                GUILayout.FlexibleSpace();
                // 结束水平布局
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}