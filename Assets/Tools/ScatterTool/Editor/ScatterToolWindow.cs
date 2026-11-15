using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ScatterTool
{
    /// <summary>
    /// 地面の上にプレハブ(草など)を、最小〜最大間隔のランダムな間隔で敷き詰めるエディタ拡張。
    /// メニュー: Tools > Scatter Tool
    /// ゲーム固有のコードには依存しないので、このフォルダごと他のプロジェクトにコピーして使える。
    /// </summary>
    public class ScatterToolWindow : EditorWindow
    {
        [System.Serializable]
        private class PrefabEntry
        {
            public GameObject prefab;
            [Min(0f)] public float weight = 1f;
        }

        private enum AreaMode
        {
            GroundBounds, // 地面全体
            CustomBox,    // 指定したボックスの中だけ
        }

        // 地面
        [SerializeField] private List<GameObject> grounds = new List<GameObject>();
        [SerializeField] private AreaMode areaMode = AreaMode.GroundBounds;
        [SerializeField] private Vector3 boxCenter = Vector3.zero;
        [SerializeField] private Vector3 boxSize = new Vector3(10f, 10f, 10f);

        // 置くもの
        [SerializeField] private List<PrefabEntry> prefabs = new List<PrefabEntry>();

        // 間隔
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 2f;
        [SerializeField] private int maxCount = 5000;

        // 見た目のばらつき
        [SerializeField] private bool randomRotateY = true;
        [SerializeField] private bool alignToNormal = false;
        [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);
        [SerializeField] private float yOffset = 0f;
        [SerializeField] private float maxSlope = 45f;
        [SerializeField] private bool avoidOtherColliders = true;

        // 乱数
        [SerializeField] private bool useFixedSeed = false;
        [SerializeField] private int seed = 0;

        // 配置先
        [SerializeField] private Transform container;
        [SerializeField] private bool clearBeforeScatter = true;

        private SerializedObject serializedWindow;
        private Vector2 scroll;
        private string lastResult = "";
        private readonly BoxBoundsHandle boxHandle = new BoxBoundsHandle();

        [MenuItem("Tools/Scatter Tool")]
        private static void Open()
        {
            GetWindow<ScatterToolWindow>("Scatter Tool");
        }

        private void OnEnable()
        {
            serializedWindow = new SerializedObject(this);
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        // ---------------- GUI ----------------

        private void OnGUI()
        {
            serializedWindow.Update();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.LabelField("地面", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(grounds)), new GUIContent("地面オブジェクト", "ここに入れたオブジェクト(子も含む)のコライダーの上にだけ置く。コライダーが無いメッシュは一時的にMeshColliderを付けて判定する。"), true);
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(areaMode)), new GUIContent("範囲", "GroundBounds: 地面全体 / CustomBox: シーンビューのボックス内だけ"));
            if (areaMode == AreaMode.CustomBox)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(boxCenter)), new GUIContent("中心"));
                EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(boxSize)), new GUIContent("サイズ"));
                if (GUILayout.Button("ボックスを地面の大きさに合わせる"))
                {
                    if (TryGetGroundBounds(out Bounds b))
                    {
                        Undo.RecordObject(this, "Fit Scatter Box");
                        boxCenter = b.center;
                        boxSize = b.size + Vector3.up * 2f;
                        serializedWindow.Update();
                        SceneView.RepaintAll();
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("置くもの", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(prefabs)), new GUIContent("プレハブ", "weightが大きいほど選ばれやすい"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("間隔", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(minDistance)), new GUIContent("最小間隔", "どの2つもこれ以上は離れる"));
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(maxDistance)), new GUIContent("最大間隔", "隣とのすき間はだいたいこれ以下になる。最小と同じなら均一に近くなる"));
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(maxCount)), new GUIContent("最大個数", "置きすぎ防止の上限"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ばらつき", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(randomRotateY)), new GUIContent("Y軸ランダム回転"));
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(alignToNormal)), new GUIContent("地面の傾きに合わせる"));
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(scaleRange)), new GUIContent("スケール(最小,最大)", "プレハブ本来の大きさに掛ける倍率"));
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(yOffset)), new GUIContent("高さオフセット", "マイナスにすると少し地面に埋まる"));
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(maxSlope)), new GUIContent("最大傾斜(度)", "これより急な面には置かない"));
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(avoidOtherColliders)), new GUIContent("他の物の下には置かない", "地面より上に別のコライダー(ベンチ等)がある場所はスキップ"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("乱数", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(useFixedSeed)), new GUIContent("シードを固定", "オフだと毎回ちがう配置になる"));
            using (new EditorGUI.DisabledScope(!useFixedSeed))
            {
                EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(seed)), new GUIContent("シード"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("配置先", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(container)), new GUIContent("親オブジェクト", "配置物はこの子になる。空なら自動で作る。「一括削除」はこの子を全部消すので、専用の空オブジェクトにしておくこと"));
            EditorGUILayout.PropertyField(serializedWindow.FindProperty(nameof(clearBeforeScatter)), new GUIContent("配置前に前回分を消す"));

            serializedWindow.ApplyModifiedProperties();

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("配置する", GUILayout.Height(30))) Scatter();

                using (new EditorGUI.DisabledScope(container == null))
                {
                    if (GUILayout.Button("一括削除", GUILayout.Height(30)))
                    {
                        if (EditorUtility.DisplayDialog("Scatter Tool", $"「{container.name}」の子を全部削除する?(Ctrl+Zで戻せる)", "削除", "やめる"))
                        {
                            int count = ClearContainer("Clear Scatter");
                            lastResult = $"{count}個削除した";
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(lastResult)) EditorGUILayout.HelpBox(lastResult, MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (areaMode != AreaMode.CustomBox) return;

            boxHandle.center = boxCenter;
            boxHandle.size = boxSize;
            boxHandle.SetColor(new Color(0.3f, 1f, 0.4f, 1f));

            EditorGUI.BeginChangeCheck();
            boxHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Edit Scatter Box");
                boxCenter = boxHandle.center;
                boxSize = boxHandle.size;
                Repaint();
            }
        }

        // ---------------- 配置 ----------------

        private void Scatter()
        {
            if (!Validate(out string error))
            {
                EditorUtility.DisplayDialog("Scatter Tool", error, "OK");
                return;
            }

            if (!useFixedSeed)
            {
                seed = System.Environment.TickCount;
            }
            var rng = new System.Random(seed);

            Undo.SetCurrentGroupName("Scatter Prefabs");
            int undoGroup = Undo.GetCurrentGroup();

            if (container == null)
            {
                var go = new GameObject("Scatter_" + grounds[0].name);
                MoveToSceneOf(go, grounds[0]);
                Undo.RegisterCreatedObjectUndo(go, "Create Scatter Container");
                container = go.transform;
            }
            else if (clearBeforeScatter)
            {
                ClearContainer(null);
            }

            // コライダーが無い地面メッシュには一時的にMeshColliderを付ける(最後に必ず消す)
            var tempColliders = AddTemporaryColliders();
            var placements = new List<(GameObject prefab, Vector3 position, Quaternion rotation, float scale)>();
            int candidates = 0;

            try
            {
                Physics.SyncTransforms();

                var groundColliders = CollectGroundColliders();
                Bounds area = GetArea(groundColliders);
                Vector2 size = new Vector2(area.size.x, area.size.z);

                if (PoissonDiskSampler.EstimateGridCells(size, minDistance) > PoissonDiskSampler.MaxGridCells)
                {
                    EditorUtility.DisplayDialog("Scatter Tool", "範囲に対して最小間隔が小さすぎる。最小間隔を大きくするか範囲を狭くして。", "OK");
                    return;
                }

                List<Vector2> points = PoissonDiskSampler.Sample(size, minDistance, maxDistance, maxCount, rng);
                candidates = points.Count;

                float rayStart = area.max.y + 1f;
                float rayLength = area.size.y + 2f;
                float totalWeight = 0f;
                foreach (var entry in prefabs) if (entry.prefab != null) totalWeight += entry.weight;

                foreach (Vector2 p in points)
                {
                    var origin = new Vector3(area.min.x + p.x, rayStart, area.min.z + p.y);
                    if (!TryFindGround(origin, rayLength, groundColliders, out RaycastHit hit)) continue;
                    if (Vector3.Angle(hit.normal, Vector3.up) > maxSlope) continue;

                    GameObject prefab = PickPrefab(rng, totalWeight);
                    Quaternion rotation = alignToNormal ? Quaternion.FromToRotation(Vector3.up, hit.normal) : Quaternion.identity;
                    if (randomRotateY) rotation *= Quaternion.AngleAxis((float)rng.NextDouble() * 360f, Vector3.up);
                    rotation *= prefab.transform.rotation;
                    float scale = Mathf.Lerp(scaleRange.x, scaleRange.y, (float)rng.NextDouble());
                    Vector3 position = hit.point + hit.normal * yOffset;

                    placements.Add((prefab, position, rotation, scale));
                }
            }
            finally
            {
                foreach (var c in tempColliders) DestroyImmediate(c);
            }

            // レイを全部飛ばしてから生成する(置いた物のコライダーが後の判定をじゃましないように)
            foreach (var (prefab, position, rotation, scale) in placements)
            {
                GameObject instance = PrefabUtility.IsPartOfPrefabAsset(prefab)
                    ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, container.gameObject.scene)
                    : Instantiate(prefab);
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.transform.localScale = prefab.transform.localScale * scale;
                instance.transform.SetParent(container, true);
                Undo.RegisterCreatedObjectUndo(instance, "Scatter Prefab");
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(container.gameObject.scene);
            serializedWindow.Update();

            lastResult = $"{placements.Count}個配置した(候補{candidates}個 / シード{seed})";
            if (candidates >= maxCount) lastResult += "\n最大個数に達したので、範囲の一部が空いてるかも。";
        }

        private bool Validate(out string error)
        {
            error = null;
            grounds.RemoveAll(g => g == null);
            if (grounds.Count == 0) error = "地面オブジェクトを1つ以上入れて。";
            else if (!prefabs.Exists(e => e.prefab != null && e.weight > 0f)) error = "プレハブを1つ以上入れて(weightは0より大きく)。";
            else if (minDistance <= 0f) error = "最小間隔は0より大きくして。";
            else if (maxDistance < minDistance) error = "最大間隔は最小間隔以上にして。";
            else if (scaleRange.x <= 0f || scaleRange.y < scaleRange.x) error = "スケールは 0 < 最小 <= 最大 にして。";
            else if (container != null && container.gameObject.scene.IsValid() == false) error = "親オブジェクトはシーン上のオブジェクトにして(プレハブアセットは不可)。";
            return error == null;
        }

        private int ClearContainer(string undoName)
        {
            if (container == null) return 0;
            if (undoName != null) Undo.SetCurrentGroupName(undoName);
            int group = Undo.GetCurrentGroup();

            int count = container.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(container.GetChild(i).gameObject);
            }

            if (undoName != null) Undo.CollapseUndoOperations(group);
            return count;
        }

        private List<Collider> AddTemporaryColliders()
        {
            var added = new List<Collider>();
            foreach (var ground in grounds)
            {
                foreach (var filter in ground.GetComponentsInChildren<MeshFilter>())
                {
                    if (filter.sharedMesh == null || filter.GetComponent<Collider>() != null) continue;
                    var mc = filter.gameObject.AddComponent<MeshCollider>();
                    mc.hideFlags = HideFlags.HideAndDontSave;
                    mc.sharedMesh = filter.sharedMesh;
                    added.Add(mc);
                }
            }
            return added;
        }

        private HashSet<Collider> CollectGroundColliders()
        {
            var set = new HashSet<Collider>();
            foreach (var ground in grounds)
            {
                foreach (var c in ground.GetComponentsInChildren<Collider>())
                {
                    if (c.enabled && !c.isTrigger) set.Add(c);
                }
            }
            return set;
        }

        private Bounds GetArea(HashSet<Collider> groundColliders)
        {
            if (areaMode == AreaMode.CustomBox) return new Bounds(boxCenter, boxSize);

            bool first = true;
            Bounds bounds = default;
            foreach (var c in groundColliders)
            {
                if (first) { bounds = c.bounds; first = false; }
                else bounds.Encapsulate(c.bounds);
            }
            return bounds;
        }

        private bool TryGetGroundBounds(out Bounds bounds)
        {
            bounds = default;
            grounds.RemoveAll(g => g == null);
            if (grounds.Count == 0) return false;

            bool first = true;
            foreach (var ground in grounds)
            {
                foreach (var r in ground.GetComponentsInChildren<Renderer>())
                {
                    if (first) { bounds = r.bounds; first = false; }
                    else bounds.Encapsulate(r.bounds);
                }
                foreach (var c in ground.GetComponentsInChildren<Collider>())
                {
                    if (first) { bounds = c.bounds; first = false; }
                    else bounds.Encapsulate(c.bounds);
                }
            }
            return !first;
        }

        private bool TryFindGround(Vector3 origin, float length, HashSet<Collider> groundColliders, out RaycastHit result)
        {
            result = default;
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, length, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            if (hits.Length == 0) return false;
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                // 配置先の子(前回置いた草など)は無視する
                if (container != null && hit.transform.IsChildOf(container)) continue;

                if (groundColliders.Contains(hit.collider))
                {
                    result = hit;
                    return true;
                }
                if (avoidOtherColliders) return false;
            }
            return false;
        }

        private GameObject PickPrefab(System.Random rng, float totalWeight)
        {
            float r = (float)rng.NextDouble() * totalWeight;
            GameObject last = null;
            foreach (var entry in prefabs)
            {
                if (entry.prefab == null || entry.weight <= 0f) continue;
                last = entry.prefab;
                r -= entry.weight;
                if (r <= 0f) return entry.prefab;
            }
            return last;
        }

        private static void MoveToSceneOf(GameObject go, GameObject sceneOwner)
        {
            if (sceneOwner.scene.IsValid() && go.scene != sceneOwner.scene)
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, sceneOwner.scene);
            }
        }
    }
}
