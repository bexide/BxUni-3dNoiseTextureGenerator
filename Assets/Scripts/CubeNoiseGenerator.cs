﻿// 2022-05-24 BeXide
// by Y.Hayashi
// using https://github.com/rthome/SharpNoise

using System;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;

/// <summary>
/// エディタ上で3Dノイズテクスチャを生成する
/// 使い方：インスペクタ上で Build ボタンを押す→よければ Save ボタンを押す
/// </summary>
[ExecuteAlways]
public class CubeNoiseGenerator : MonoBehaviour
{
    public enum NoiseType
    {
        Perlin,
        Simplex,
        RidgedMulti,
        Cell,
    }

    [SerializeField]
    private NoiseType m_noiseType = NoiseType.Perlin;

    //[SerializeField]
    //private TextureFormat m_textureFormat = TextureFormat.R8;

    [BoxGroup("Common Parameters"), SerializeField]
    private int m_gridSize = 32;

    [BoxGroup("Common Parameters"), SerializeField]
    private float m_frequency = 4.0f;

    [BoxGroup("Common Parameters"), SerializeField]
    private SharpNoise.NoiseQuality m_quality = SharpNoise.NoiseQuality.Standard;

    [BoxGroup("Common Parameters"), SerializeField]
    private bool m_isSeamless = true;

    [BoxGroup("Common Parameters"), SerializeField]
    private bool m_isInverse = false;
    
    [BoxGroup("Common Parameters"), SerializeField]
    private Gradient m_gradient;

    [BoxGroup("Perlin/Simplex/RidgedMulti Parameters"), EnableIf("IsPSR"), SerializeField]
    private float m_lacunarity = 2.0f;

    [BoxGroup("Perlin/Simplex/RidgedMulti Parameters"), EnableIf("IsPSR"), SerializeField]
    private int m_octaves = 4;

    [BoxGroup("Perlin/Simplex Parameters"), EnableIf("IsPS"), SerializeField]
    private float m_persistence = 0.5f;

    [BoxGroup("Cell Parameters"), EnableIf("IsCell"), SerializeField]
    private SharpNoise.Modules.Cell.CellType m_cellType =
        SharpNoise.Modules.Cell.CellType.Voronoi;

    [BoxGroup("Cell Parameters"), EnableIf("IsCell"), SerializeField]
    private float m_displacement = (float)SharpNoise.Modules.Cell.DefaultDisplacement;

    [BoxGroup("Preview"), SerializeField]
    private Renderer m_cubeObject;

    [BoxGroup("Preview"), Range(0,1), SerializeField]
    private float m_previewAlpha = 0.04f;
    
    private Texture3D m_texture;

    public bool IsPerlin => m_noiseType == NoiseType.Perlin;
    public bool IsSimplex => m_noiseType == NoiseType.Simplex;
    public bool IsRidgedMulti => m_noiseType == NoiseType.RidgedMulti;
    public bool IsCell => m_noiseType == NoiseType.Cell;
    public bool IsPS => IsPerlin || IsSimplex;
    public bool IsPSR => IsPerlin || IsSimplex || IsRidgedMulti;

    private void OnValidate()
    {
        if (m_gradient == null)
        {
            m_gradient = new Gradient
            {
                alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1, 1)
                },
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(Color.black, 0),
                    new GradientColorKey(Color.white, 1)
                },
                mode = GradientMode.Blend,
            };
        }

        if (m_cubeObject != null &&
            m_cubeObject.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial.SetFloat("_Alpha", m_previewAlpha);
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
                Seed = new System.Random().Next(),
                Frequency = m_frequency,
                Lacunarity = m_lacunarity,
                OctaveCount = m_octaves,
                Persistence = m_persistence,
                Quality = m_quality,
            },
            NoiseType.Simplex => new SharpNoise.Modules.Simplex
            {
                Frequency = m_frequency,
                Lacunarity = m_lacunarity,
                OctaveCount = m_octaves,
                Persistence = m_persistence,
            },
            NoiseType.RidgedMulti => new SharpNoise.Modules.RidgedMulti
            {
                Seed = new System.Random().Next(),
                Frequency = m_frequency,
                Lacunarity = m_lacunarity,
                OctaveCount = m_octaves,
                Quality = m_quality,
            },
            NoiseType.Cell => new SharpNoise.Modules.Cell
            {
                Seed = new System.Random().Next(),
                Frequency = m_frequency,
                Displacement = m_displacement,
                Type = m_cellType,
            },
            _ => throw new ArgumentException("Type mismatch")
        };

        return source;
    }

    /// <summary>
    /// テクスチャ生成
    /// </summary>
    [Button("Build")]
    private void BuildPerlin()
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

        BuildTexture(noiseSource);
    }

    private void BuildTexture(SharpNoise.Modules.Module noiseSource)
    {
        TextureFormat format = TextureFormat.R8;
        //TextureFormat format = TextureFormat.Alpha8;

        // テクスチャを作成して設定を適用 
        m_texture = new Texture3D(m_gridSize, m_gridSize, m_gridSize, format, false);
        m_texture.wrapMode = TextureWrapMode.Repeat;

        // 3D配列にデータを保存
        float[] values = new float[m_gridSize * m_gridSize * m_gridSize];
        byte[] pixels = new byte[m_gridSize * m_gridSize * m_gridSize];

        float noiseScale = 1.0f / m_gridSize;
        float min = Single.MaxValue;
        float max = Single.MinValue;

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
                        x * noiseScale, y * noiseScale, z * noiseScale);

                    min = Mathf.Min(min, noiseValue);
                    max = Mathf.Max(max, noiseValue);

                    values[x + yOffset + zOffset] = noiseValue;
                }
            }
        }

#if true
        // normalize
        Debug.Log($"min={min}, max={max}");
        for (int i = 0; i < m_gridSize * m_gridSize * m_gridSize; i++)
        {
            float v = values[i];
            float noiseValue = (v - min) / (max - min);
            var color = m_gradient.Evaluate(noiseValue);
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
        m_texture.SetPixelData(pixels, 0);

        // Apply the changes to the texture and upload the updated texture to the GPU
        m_texture.Apply();

        m_cubeObject.sharedMaterial.mainTexture = m_texture;
    }

    [SerializeField]
    private string m_fileName = "Example3DTexture";
    
    [Button("Save")]
    private void SaveTexture()
    {
        // テクスチャを Unity プロジェクトに保存
        AssetDatabase.CreateAsset(m_texture, $"Assets/{m_fileName}.asset");
        AssetDatabase.SaveAssets();
    }

}