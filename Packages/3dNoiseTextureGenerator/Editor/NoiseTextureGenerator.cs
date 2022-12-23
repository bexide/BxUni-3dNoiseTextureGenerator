// 2022-12-22 BeXide,Inc.
// by Y.Hayashi

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.PlayerLoop;

namespace BxUni.NoiseTextureGenerator
{
    internal class NoiseTextureGenerator : EditorWindow
    {
        private enum NoiseType
        {
            Perlin,
            Billow,
            RidgedMulti,
            CheckerBoard,
            Cylinders,
            Spheres,
            Cell,
        }

        private string    m_fileName  = "Example3DTexture";
        private NoiseType m_noiseType = NoiseType.Perlin;

        // common parameters
        private int      m_gridSize   = 64;
        private int      m_randomSeed = 1;
        private float    m_frequency  = 1f;
        private bool     m_isSeamless = false;
        private bool     m_isInverse  = false;
        private Gradient m_gradient;

        // fractal parameters
        private float m_lacunarity  = 2f;
        private int   m_octaves     = 1;
        private float m_persistence = 0.5f;

        private SharpNoise.NoiseQuality m_quality = SharpNoise.NoiseQuality.Standard;

        // cell parameters
        private SharpNoise.Modules.Cell.CellType m_cellType =
            SharpNoise.Modules.Cell.CellType.Voronoi;

        public bool IsCell    => m_noiseType == NoiseType.Cell;
        public bool IsFractal => m_noiseType != NoiseType.Cell;

        // for preview
        private float     m_previewAlpha = 0.04f;
        private Texture3D m_texture;
        private Editor    m_editor;

        // start window
        [MenuItem("BeXide/3D Noise Texture Generator")]
        private static void Start()
        {
            var window = GetWindow<NoiseTextureGenerator>();
            window.Initialize();
        }

        private void Initialize()
        {
            m_gradient ??= new Gradient
            {
                alphaKeys = new[] { new GradientAlphaKey(1, 1) },
                colorKeys = new[]
                {
                    new GradientColorKey(Color.black, 0),
                    new GradientColorKey(Color.white, 1)
                },
                mode = GradientMode.Blend,
            };
        }

        // window context
        private void OnGUI()
        {
            m_fileName = EditorGUILayout.TextField("TextureFileName", m_fileName);

            m_noiseType = (NoiseType)EditorGUILayout.EnumPopup("NoiseType", m_noiseType);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Common Parameters");
            m_gridSize   = EditorGUILayout.IntField("Grid Size", m_gridSize);
            m_randomSeed = EditorGUILayout.IntField("Random Seed", m_randomSeed);
            m_frequency  = EditorGUILayout.FloatField("Frequency", m_frequency);
            m_isSeamless = EditorGUILayout.Toggle("Is Seamless", m_isSeamless);
            m_isInverse  = EditorGUILayout.Toggle("Is Inverse", m_isInverse);
            m_gradient   = EditorGUILayout.GradientField("Gradient", m_gradient);
            EditorGUILayout.EndVertical();

            if (IsFractal)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Fractal Parameters");
                m_lacunarity  = EditorGUILayout.FloatField("Lacunarity", m_lacunarity);
                m_octaves     = EditorGUILayout.IntField("Octaves", m_octaves);
                m_persistence = EditorGUILayout.FloatField("Persistence", m_persistence);
                m_quality = (SharpNoise.NoiseQuality)EditorGUILayout.EnumPopup(
                    "Quality",
                    m_quality);
                EditorGUILayout.EndVertical();
            }

            if (IsCell)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Cell Parameters");
                m_cellType
                    = (SharpNoise.Modules.Cell.CellType)EditorGUILayout.EnumPopup(
                        "Cell Type",
                        m_cellType);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build"))
            {
                Build();
                CreatePreview();
            }
            if (GUILayout.Button("Clear"))
            {
                Clear();
            }
            EditorGUILayout.EndHorizontal();

            // preview
            if (m_editor)
            {
                EditorGUILayout.BeginHorizontal();
                m_editor.OnPreviewSettings();
                EditorGUILayout.EndHorizontal();
                var rect = GUILayoutUtility.GetAspectRect(1f);
                m_editor.OnInteractivePreviewGUI(rect, GUIStyle.none);
            }

            if (m_texture != null &&
                GUILayout.Button("Save Texture"))
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "",
                    m_fileName,
                    "asset",
                    "asset");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(m_texture, path);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// ノイズソース作成
        /// </summary>
        private SharpNoise.Modules.Module CreateSource()
        {
            SharpNoise.Modules.Module source = m_noiseType switch
            {
                NoiseType.Perlin => new SharpNoise.Modules.Perlin
                {
                    Seed        = m_randomSeed,
                    Frequency   = m_frequency,
                    Lacunarity  = m_lacunarity,
                    OctaveCount = m_octaves,
                    Persistence = m_persistence,
                    Quality     = m_quality,
                },
                NoiseType.Billow => new SharpNoise.Modules.Billow
                {
                    Seed        = m_randomSeed,
                    Frequency   = m_frequency,
                    Lacunarity  = m_lacunarity,
                    OctaveCount = m_octaves,
                    Persistence = m_persistence,
                    Quality     = m_quality,
                },
                NoiseType.RidgedMulti => new SharpNoise.Modules.RidgedMulti
                {
                    Seed        = m_randomSeed,
                    Frequency   = m_frequency,
                    Lacunarity  = m_lacunarity,
                    OctaveCount = m_octaves,
                    Quality     = m_quality,
                },
                NoiseType.CheckerBoard => new SharpNoise.Modules.Checkerboard
                {
                },
                NoiseType.Cylinders => new SharpNoise.Modules.Cylinders
                {
                    Frequency = m_frequency,
                },
                NoiseType.Spheres => new SharpNoise.Modules.Spheres
                {
                    Frequency = m_frequency,
                },
                NoiseType.Cell => new SharpNoise.Modules.Cell
                {
                    Seed      = m_randomSeed,
                    Frequency = m_frequency,
                    //EnableDistance = false,
                    //Displacement = m_displacement,
                    Type = m_cellType,
                },
                _ => throw new ArgumentException("Type mismatch")
            };

            return source;
        }

        /// <summary>
        /// テクスチャ生成
        /// </summary>
        private void Build()
        {
            var noiseSource = CreateSource();

            // シームレスフィルタ
            if (m_isSeamless)
            {
                var ss = new SharpNoise.Modules.Seamless { Source0 = noiseSource };
                noiseSource = ss;
            }

            // 反転フィルタ
            if (m_isInverse)
            {
                var inv = new SharpNoise.Modules.Invert { Source0 = noiseSource };
                noiseSource = inv;
            }

            m_texture = BuildTexture(noiseSource);
        }

        private Texture3D BuildTexture(SharpNoise.Modules.Module noiseSource)
        {
            var format = TextureFormat.R8;

            // テクスチャを作成して設定を適用
            var texture = new Texture3D(m_gridSize, m_gridSize, m_gridSize, format, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode   = m_isSeamless ? TextureWrapMode.Repeat : TextureWrapMode.Clamp
            };

            // 3D配列にデータを保存
            float[] values = new float[m_gridSize * m_gridSize * m_gridSize];
            byte[]  pixels = new byte[m_gridSize * m_gridSize * m_gridSize];

            float noiseScale = 1.0f / m_gridSize;
            float min        = Single.MaxValue;
            float max        = Single.MinValue;

            // 配列に入力
            for (int z = 0; z < m_gridSize; z++)
            {
                int zOffset = z * m_gridSize * m_gridSize;
                for (int y = 0; y < m_gridSize; y++)
                {
                    int yOffset = y * m_gridSize;
                    for (int x = 0; x < m_gridSize; x++)
                    {
                        float noiseValue = (float)noiseSource.GetValue(
                            x * noiseScale - 0.5,
                            y * noiseScale - 0.5,
                            z * noiseScale - 0.5);

                        min = Mathf.Min(min, noiseValue);
                        max = Mathf.Max(max, noiseValue);

                        values[x + yOffset + zOffset] = noiseValue;
                    }
                }
            }

#if true
            // normalize
            //Debug.Log($"min={min}, max={max}");
            if (min.Equals(max))
            {
                Debug.LogError("Illegal Value Range");
                return null;
            }
            for (int i = 0; i < m_gridSize * m_gridSize * m_gridSize; i++)
            {
                float v          = values[i];
                float noiseValue = (v - min) / (max - min);
                var   color      = m_gradient.Evaluate(noiseValue);
                pixels[i] = (byte)Mathf.FloorToInt(color.r * 255.0f);
            }
#else
            for (int i = 0; i < m_gridSize * m_gridSize * m_gridSize; i++)
            {
                float v = values[i];
                pixels[i] = (byte)Mathf.FloorToInt(v * 255.0f);
            }
#endif

            // テクスチャにカラー値をコピー
            texture.SetPixelData(pixels, 0);
            texture.Apply();

            return texture;
        }

        /// <summary> Preview用Editorを生成 </summary>
        private void CreatePreview()
        {
            if (m_texture == null)
            {
                Clear();
                return;
            }
            if (m_editor == null)
            {
                m_editor = Editor.CreateEditor(m_texture);
            }
            else
            {
                Editor.CreateCachedEditor(m_texture, m_editor.GetType(), ref m_editor);
            }
        }

        private void Clear()
        {
            m_texture = null;
            m_editor  = null;
        }

    }
}
