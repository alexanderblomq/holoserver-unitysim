using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PointCloudRenderer : MonoBehaviour {
    Texture2D texColor;
    Texture2D texPosScale;
    VisualEffect vfx;

    [SerializeField]
    private uint maxParticles = 1000000;

    [SerializeField]
    private float updateFreqSeconds = 1;

    private float timer = 0;

    private uint resolution;

    private static readonly int ID_ParticleCount = Shader.PropertyToID("ParticleCount");
    private static readonly int ID_TexColor = Shader.PropertyToID("TexColor");
    private static readonly int ID_TexPosScale = Shader.PropertyToID("TexPosScale");
    private static readonly int ID_Resolution = Shader.PropertyToID("Resolution");

    public float particleSize = 0.1f;

    uint particleCount = 0;

    private Queue<Vector3> positionQueue = new();
    private Queue<Color> colorQueue = new();

    private void Start() {
        resolution = (uint)Mathf.Ceil(Mathf.Sqrt(maxParticles));

        texColor = new Texture2D((int)resolution, (int)resolution, TextureFormat.RGBAFloat, false);
        texPosScale = new Texture2D((int)resolution, (int)resolution, TextureFormat.RGBAFloat, false);

        vfx = GetComponent<VisualEffect>();
        vfx.SetUInt(ID_Resolution, resolution);
        vfx.SetTexture(ID_TexColor, texColor);
        vfx.SetTexture(ID_TexPosScale, texPosScale);

        for (int i = 0; i < 10000000; i++)
        {
            AddParticle(new Vector3(Random.value * 10, Random.value * 10, Random.value * 10),
            new Color(Random.value, Random.value, Random.value));
        }

    }

    private void LateUpdate()
    {
        timer += Time.deltaTime;
        if (timer < updateFreqSeconds) return;
        timer = 0;

        int availableSlots = (int)maxParticles - (int)particleCount;
        int countToWrite = Mathf.Min(positionQueue.Count, availableSlots);
        if (availableSlots == 0 && positionQueue.Count > 0) Debug.Log("No Writable Texture Space for Point Cloud");
        if (countToWrite == 0) return;

        for (int i = 0; i < countToWrite; i++)
        {
            Vector3 pos = positionQueue.Dequeue();
            Color col = colorQueue.Dequeue();

            int index = (int)particleCount;
            int x = index % (int)resolution;
            int y = index / (int)resolution;

            texColor.SetPixel(x, y, col);
            texPosScale.SetPixel(x, y, new Color(pos.x, pos.y, pos.z, particleSize));

            particleCount++;
        }

        texColor.Apply();
        texPosScale.Apply();
        vfx.Reinit();

        vfx.SetUInt(ID_ParticleCount, particleCount);
    }

    public void AddParticles(Vector3[] positions, Color[] colors) {
        for(int i = 0; i < Mathf.Min(positions.Length, colors.Length); i++)
        {
            positionQueue.Enqueue(positions[i]);
            colorQueue.Enqueue(colors[i]);
        }
    }

    public void AddParticle(Vector3 position, Color color)
    {
        positionQueue.Enqueue(position);
        colorQueue.Enqueue(color);
    }

    public void resetRenderer()
    {
        particleCount = 0;
        texColor = new Texture2D((int)resolution, (int)resolution, TextureFormat.RGBAFloat, false);
        texPosScale = new Texture2D((int)resolution, (int)resolution, TextureFormat.RGBAFloat, false);
        texColor.Apply();
        texPosScale.Apply();
        vfx.Reinit();

        vfx.SetUInt(ID_ParticleCount, particleCount);
    }
}
