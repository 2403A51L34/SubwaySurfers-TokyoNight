using System.Collections.Generic;
using UnityEngine;

/// Helper that builds simple 3D objects from Unity primitives (no external assets needed).
public static class Prim
{
    static Shader litShader;
    static Mesh coneMesh;
    static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { litShader = null; coneMesh = null; cache.Clear(); }

    // Borrow the default shader of the current render pipeline (Built-in or URP) so colours always work.
    static Shader GetLitShader()
    {
        if (litShader == null)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            litShader = temp.GetComponent<Renderer>().sharedMaterial.shader;
            Object.DestroyImmediate(temp);
        }
        return litShader;
    }

    public static Material Mat(Color c, float emission = 0f)
    {
        string key = ColorUtility.ToHtmlStringRGBA(c) + "_" + emission.ToString("F2");
        Material m;
        if (cache.TryGetValue(key, out m) && m != null) return m;
        m = new Material(GetLitShader());
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.2f);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.2f);
        if (emission > 0f)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c * emission);
        }
        cache[key] = m;
        return m;
    }

    // Semi-transparent unlit material (glow halos, shield bubble).
    public static Material TransparentMat(Color c)
    {
        string key = "T" + ColorUtility.ToHtmlStringRGBA(c);
        Material m;
        if (cache.TryGetValue(key, out m) && m != null) return m;
        Shader s = Shader.Find("Sprites/Default");
        if (s == null) return Mat(c);
        m = new Material(s);
        m.color = c;
        cache[key] = m;
        return m;
    }

    public static GameObject Make(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Color color, bool keepCollider = false, float emission = 0f)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        if (!keepCollider)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
        }
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.GetComponent<Renderer>().sharedMaterial = Mat(color, emission);
        return go;
    }

    public static GameObject Box(Transform p, Vector3 pos, Vector3 size, Color c, float em = 0f) { return Make(PrimitiveType.Cube, p, pos, size, c, false, em); }
    public static GameObject Ball(Transform p, Vector3 pos, Vector3 size, Color c, float em = 0f) { return Make(PrimitiveType.Sphere, p, pos, size, c, false, em); }
    public static GameObject Tube(Transform p, Vector3 pos, Vector3 size, Color c, float em = 0f) { return Make(PrimitiveType.Cylinder, p, pos, size, c, false, em); }

    // Cone: base centre at localPos, apex pointing up. scale = (diameter, height, diameter).
    public static GameObject MakeCone(Transform parent, Vector3 localPos, Vector3 scale, Color color, float emission = 0f)
    {
        GameObject go = new GameObject("Cone");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        go.AddComponent<MeshFilter>().sharedMesh = GetCone();
        go.AddComponent<MeshRenderer>().sharedMaterial = Mat(color, emission);
        return go;
    }

    public static BoxCollider AddBox(GameObject go, Vector3 center, Vector3 size)
    {
        BoxCollider b = go.AddComponent<BoxCollider>();
        b.isTrigger = true;
        b.center = center;
        b.size = size;
        return b;
    }

    static Mesh GetCone()
    {
        if (coneMesh != null) return coneMesh;
        const int seg = 16;
        Vector3[] verts = new Vector3[seg * 6];
        int[] tris = new int[seg * 6];
        Vector3 apex = new Vector3(0f, 1f, 0f);
        for (int i = 0; i < seg; i++)
        {
            float a0 = (float)i / seg * Mathf.PI * 2f;
            float a1 = (float)(i + 1) / seg * Mathf.PI * 2f;
            Vector3 p0 = new Vector3(Mathf.Cos(a0) * 0.5f, 0f, Mathf.Sin(a0) * 0.5f);
            Vector3 p1 = new Vector3(Mathf.Cos(a1) * 0.5f, 0f, Mathf.Sin(a1) * 0.5f);
            int s = i * 3;
            verts[s] = apex; verts[s + 1] = p1; verts[s + 2] = p0;
            int b = seg * 3 + i * 3;
            verts[b] = Vector3.zero; verts[b + 1] = p0; verts[b + 2] = p1;
        }
        for (int i = 0; i < tris.Length; i++) tris[i] = i;
        coneMesh = new Mesh();
        coneMesh.name = "WinterCone";
        coneMesh.vertices = verts;
        coneMesh.triangles = tris;
        coneMesh.RecalculateNormals();
        coneMesh.RecalculateBounds();
        return coneMesh;
    }
}
