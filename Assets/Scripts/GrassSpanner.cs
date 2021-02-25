using UnityEngine;
using System.Collections.Generic;

using Random = System.Random;

public class GrassSpanner : MonoBehaviour
{

	public Texture2D m_HeightMap;
	public float m_TerrainHeight;
	public int m_TerrainSize = 64;
	public int m_GrassPatchRowCount = 32;
	public int m_GrassCountPerPatch = 20;
	public Material m_TerrainMat;
	public Material m_GrassMat;
	private List<Vector3> m_Verts = new List<Vector3>();
	private Random m_Random;

	private List<GameObject> m_GrassLayers = new List<GameObject>();  // 点云 mesh
	GameObject m_Plane;	   // 地形mesh

	void Awake()
	{
		m_Random = new Random();
		DoGenerate();
	}

	void DoClean()
	{
		if (m_Plane)
		{
			Destroy(m_Plane);
			m_Plane = null;
		}
		foreach (GameObject grassLayer in this.m_GrassLayers)
		{
			Destroy(grassLayer);
		}
		m_GrassLayers.Clear();
	}

	void DoGenerate()
	{
		DoClean();
		GenerateTerrain();
		GenerateField(m_GrassPatchRowCount, m_GrassCountPerPatch);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			DoGenerate();
		}
	}

	/// <summary>
	/// 结合高度图，生成一个方形的地形
	/// </summary>
	private void GenerateTerrain()
	{
		List<Vector3> terrainVerts = new List<Vector3>();
		List<int> tris = new List<int>();

		// 尺寸为 m_TerrainSize x m_TerrainSize
		for (int i = 0; i < this.m_TerrainSize; i++)
		{
			for (int j = 0; j < this.m_TerrainSize; j++)
			{
				terrainVerts.Add(new Vector3(i, m_HeightMap.GetPixel(i, j).grayscale * this.m_TerrainHeight, j));
				if (i == 0 || j == 0)			   // 跳过左下边框顶点
					continue;
				tris.Add(m_TerrainSize * i + j);	  // 构建两个三角形
				tris.Add(m_TerrainSize * i + j - 1);
				tris.Add(m_TerrainSize * (i - 1) + j - 1);
				tris.Add(m_TerrainSize * (i - 1) + j - 1);
				tris.Add(m_TerrainSize * (i - 1) + j);
				tris.Add(m_TerrainSize * i + j);
			}
		}

		Vector2[] uvs = new Vector2[terrainVerts.Count];

		for (var i = 0; i < uvs.Length; i++)
		{
			uvs[i] = new Vector2(terrainVerts[i].x, terrainVerts[i].z);
		}

		m_Plane = new GameObject("groundPlane");
		m_Plane.AddComponent<MeshFilter>();
		MeshRenderer renderer = m_Plane.AddComponent<MeshRenderer>();
		renderer.sharedMaterial = m_TerrainMat;

		Mesh groundMesh = new Mesh();
		groundMesh.vertices = terrainVerts.ToArray();
		groundMesh.uv = uvs;
		groundMesh.triangles = tris.ToArray();
		groundMesh.RecalculateNormals();
		m_Plane.GetComponent<MeshFilter>().mesh = groundMesh;
	}

	/// <summary>
	/// 生成草地
	/// </summary>
	/// <param name="m_GrassPatchRowCount"></param>
	/// <param name="m_GrassCountPerPatch"></param>
	private void GenerateField(int m_GrassPatchRowCount, int m_GrassCountPerPatch)
	{
		List<int> indices = new List<int>();
		for (int i = 0; i < 65000; i++) // Unity的网格顶点上限是65000
		{
			indices.Add(i);
		}

		Vector3 startPosition = new Vector3(0, 0, 0);
		Vector3 patchSize = new Vector3(m_TerrainSize / m_GrassPatchRowCount, 0, m_TerrainSize / m_GrassPatchRowCount);

		// 所有草根顶点的位置
		for (int x = 0; x < m_GrassPatchRowCount; x++)
		{
			for (int y = 0; y < m_GrassPatchRowCount; y++)
			{
				this.GenerateGrass(startPosition, patchSize, m_GrassCountPerPatch);
				startPosition.x += patchSize.x;
			}

			startPosition.x = 0;
			startPosition.z += patchSize.z;
		}

		GameObject grassLayer;
		MeshFilter mf;
		MeshRenderer renderer;
		Mesh m;

		// 可能需要不止一个mesh来放草根mesh
		int suffix = 0;
		while (m_Verts.Count > 65000)
		{
			m = new Mesh();
			m.vertices = m_Verts.GetRange(0, 65000).ToArray();
			m.SetIndices(indices.ToArray(), MeshTopology.Points, 0);	// Points组成的mesh

			grassLayer = new GameObject("grassLayer" + suffix.ToString());
			mf = grassLayer.AddComponent<MeshFilter>();
			renderer = grassLayer.AddComponent<MeshRenderer>();
			renderer.sharedMaterial = m_GrassMat;
			mf.mesh = m;
			m_Verts.RemoveRange(0, 65000);
			suffix += 1;
			m_GrassLayers.Add(grassLayer);
		}

		m = new Mesh();
		m.vertices = m_Verts.ToArray();
		m.SetIndices(indices.GetRange(0, m_Verts.Count).ToArray(), MeshTopology.Points, 0);
		grassLayer = new GameObject("grassLayer" + suffix.ToString());
		mf = grassLayer.AddComponent<MeshFilter>();
		renderer = grassLayer.AddComponent<MeshRenderer>();
		renderer.sharedMaterial = m_GrassMat;
		mf.mesh = m;
		m_GrassLayers.Add(grassLayer);

		m_Verts.Clear();

		return;
	}

	/// <summary>
	/// 生成一个patch内的草根
	/// </summary>
	/// <param name="startPosition"></param>
	/// <param name="patchSize"></param>
	/// <param name="m_GrassCountPerPatch"></param>
	private void GenerateGrass(Vector3 startPosition, Vector3 patchSize, int m_GrassCountPerPatch)
	{
		for (var i = 0; i < m_GrassCountPerPatch; i++)
		{
			// 随机一下草的位置
			var randomizedZDistance = (float)this.m_Random.NextDouble() * patchSize.z;
			var randomizedXDistance = (float)this.m_Random.NextDouble() * patchSize.x;

			// 高度图的像素坐标
			int indexX = (int)((startPosition.x + randomizedXDistance));
			int indexZ = (int)((startPosition.z + randomizedZDistance));
			if (indexX >= m_TerrainSize)
			{
				indexX = (int)m_TerrainSize - 1;
			}
			if (indexZ >= m_TerrainSize)
			{
				indexZ = (int)m_TerrainSize - 1;
			}

			var currentPosition = new Vector3(startPosition.x + (randomizedXDistance), m_HeightMap.GetPixel(indexX, indexZ).grayscale * (m_TerrainHeight + 1), startPosition.z + randomizedZDistance);
			this.m_Verts.Add(currentPosition);
		}
	}

}
