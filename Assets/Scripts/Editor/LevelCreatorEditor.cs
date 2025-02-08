using Runtime.Helpers;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(LevelCreatorScript))]
    public class LevelCreatorEditor : UnityEditor.Editor
    {
        private bool drawGrid = true;

        public override void OnInspectorGUI()
        {
            LevelCreatorScript levelCreatorScript = (LevelCreatorScript)target;

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.UpperLeft
            };

            
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("If you are going to change the width and height, first do Undraw Grid and then load grid again", MessageType.Info);
            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Level"))
            {
                levelCreatorScript.GenerateLevelData();
            }

            EditorGUILayout.LabelField("Save/Load/Reset Grid", titleStyle);

            EditorGUILayout.HelpBox("Don't Forget Save", MessageType.Warning);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save Grid"))
            {
                levelCreatorScript.SaveLevelData();
            }

            if (GUILayout.Button("Load Grid"))
            {
                levelCreatorScript.LoadLevelData();
                drawGrid = true;
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Reset Grid Data"))
            {
                levelCreatorScript.ResetGridData();
            }

            if (GUILayout.Button("Undraw Grid"))
            {
                drawGrid = false;
            }

            EditorGUILayout.LabelField("Grid", titleStyle);

            if (drawGrid && levelCreatorScript.GetCurrentLevelData() != null && levelCreatorScript.GetCurrentLevelData().Grids != null)
            {
                DrawGrid(levelCreatorScript);
            }
        }

        private void DrawGrid(LevelCreatorScript levelCreatorScript)
        {
            int rows = levelCreatorScript.GetRows();
            int columns = levelCreatorScript.GetColumns();

            for (int y = rows - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int x = 0; x < columns; x++)
                {
                    GUI.color = levelCreatorScript.GetGridColor(new Vector2Int(x, y));

                    if (GUILayout.Button($"{x}x{y}", GUILayout.Width(levelCreatorScript.gridSize), GUILayout.Height(levelCreatorScript.gridSize)))
                    {
                        levelCreatorScript.ToggleGridOccupancy(x, y);
                        levelCreatorScript.SetGridColor(x, y);
                    }

                    GUILayout.Space(5);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}