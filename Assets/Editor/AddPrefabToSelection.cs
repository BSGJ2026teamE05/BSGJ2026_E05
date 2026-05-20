using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AddPrefabToSelection : EditorWindow
{
    // 複数のPrefabを保持するリスト
    private List<GameObject> prefabsToInstantiate = new List<GameObject> { null };
    private Vector2 scrollPosition;

    [MenuItem("Tools/選択オブジェクトにPrefabを一括挿入")]
    public static void ShowWindow()
    {
        GetWindow<AddPrefabToSelection>("Prefabの一括挿入");
    }

    private void OnGUI()
    {
        GUILayout.Label("親にしたいオブジェクトをヒエラルキーで選択した状態で実行してください", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        // Prefabの登録エリア（スクロール可能にする）
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));

        for (int i = 0; i < prefabsToInstantiate.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Prefabの選択枠
            prefabsToInstantiate[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab {i + 1}", prefabsToInstantiate[i], typeof(GameObject), false);

            // 削除ボタン
            if (GUILayout.Button("削除", GUILayout.Width(50)))
            {
                prefabsToInstantiate.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // 枠を追加するボタン
        if (GUILayout.Button("Prefabの登録枠を追加"))
        {
            prefabsToInstantiate.Add(null);
        }

        EditorGUILayout.Space();

        // 実行ボタン
        if (GUILayout.Button("一括で子要素にする"))
        {
            // 有効なPrefabだけを抽出
            List<GameObject> validPrefabs = prefabsToInstantiate.FindAll(p => p != null);

            if (validPrefabs.Count == 0)
            {
                EditorUtility.DisplayDialog("エラー", "Prefabが一つも登録されていません。", "OK");
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;

            foreach (GameObject parent in selectedObjects)
            {
                // 登録されたすべてのPrefabをループ処理
                foreach (GameObject prefab in validPrefabs)
                {
                    GameObject newChild = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                    if (newChild != null)
                    {
                        newChild.transform.SetParent(parent.transform);

                        // 位置・回転・前回の修正（元のサイズ維持）を適用
                        newChild.transform.localPosition = prefab.transform.localPosition;
                        newChild.transform.localRotation = Quaternion.identity;
                        newChild.transform.localScale = prefab.transform.localScale;

                        Undo.RegisterCreatedObjectUndo(newChild, "Add Prefab to Child");
                    }
                }
            }

            Debug.Log($"{selectedObjects.Length}個のオブジェクトに、それぞれ{validPrefabs.Count}個のPrefabを挿入しました！");
        }
    }
}