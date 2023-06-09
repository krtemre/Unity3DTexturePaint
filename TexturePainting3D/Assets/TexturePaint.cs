using Unity.Collections;
using UnityEngine;

public class TexturePaint : MonoBehaviour
{
    [SerializeField] private int TextureWidth = 128;
    [SerializeField] private int TextureHeight = 128;
    private int MaxArrayLength
    {
        get
        {
            return TextureHeight * TextureWidth;
        }
    }

    [SerializeField] private int MouseInnerRadiusPerc = 10;
    [SerializeField] private int MouseOuterRadiusPerc = 10;

    [SerializeField] private Color32 colorBrush = Color.red;
    [SerializeField] private Color outerColorBrush = Color.black;

    private Renderer renderer;

    private Texture2D paintTexture;
    private NativeArray<Color32> pixelArray;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();

        paintTexture = new Texture2D(TextureWidth, TextureHeight);
        pixelArray = paintTexture.GetRawTextureData<Color32>();
        renderer.material.SetTexture("_MainTex", paintTexture);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (gameObject == GetGameObject(out RaycastHit hit))
            {
                PaintRadiusByScale(hit.point.x - transform.position.x, hit.point.y - transform.position.y);
            }
        }
    }

    private GameObject GetGameObject(out RaycastHit hit)
    {
        GameObject target = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction * 10, out hit))
        {
            target = hit.collider.gameObject;
        }

        return target;
    }

    private void Start()
    {
        ClearTexture();
    }

    public void ClearTexture()
    {
        int index = 0;

        for (int iHeight = 0; iHeight < TextureHeight; iHeight++)
        {
            for (int iWidth = 0; iWidth < TextureWidth; iWidth++)
            {
                pixelArray[index] = Color.white;
                index++;
            }
        }

        SetPaintedTexture();
    }

    private void PaintRadiusByScale(float x, float y)
    {
        int totalRadius = MouseInnerRadiusPerc + MouseOuterRadiusPerc;
        int halfRadius = totalRadius / 2;
        int halfInnerRadius = MouseInnerRadiusPerc / 2;

        float scaleX2 = transform.localScale.x;
        float scaleY2 = transform.localScale.y;

        float deterimatedPointX = x / scaleX2; // between -0.5 and 0.5
        float deterimatedPointY = y / scaleY2; // between -0.5 and 0.5 

        deterimatedPointX = (int)(deterimatedPointX * TextureWidth) + (TextureWidth / 2) - halfRadius;
        deterimatedPointY = (int)(deterimatedPointY * TextureHeight) + (TextureHeight / 2) - halfRadius;

        int approximetedIndex = (int)(deterimatedPointX + (deterimatedPointY * TextureWidth));

        int centre_X = (int)deterimatedPointX;
        int centre_Y = (int)deterimatedPointY;

        approximetedIndex = Mathf.Max(0, Mathf.Min(approximetedIndex, MaxArrayLength));

        for (int i = -halfRadius; i < halfRadius; i++)
        {
            for (int j = -halfRadius; j < halfRadius; j++)
            {
                if (IsInArrayCirle(centre_X, centre_Y, centre_X + j, centre_Y + i, halfInnerRadius))
                {
                    pixelArray[approximetedIndex] = colorBrush;
                }
                else if (IsInArrayCirle(centre_X, centre_Y, centre_X + j, centre_Y + i, halfRadius))
                {
                    if(!IsColorsSame(pixelArray[approximetedIndex], colorBrush))
                    {
                        pixelArray[approximetedIndex] = outerColorBrush;
                    }
                }
                approximetedIndex = Mathf.Min(approximetedIndex + 1, MaxArrayLength);
            }
            approximetedIndex = Mathf.Min(approximetedIndex + TextureWidth - totalRadius,
                MaxArrayLength);
        }

        SetPaintedTexture();
    }

    private bool IsColorsSame(Color32 a, Color32 b)
    {
        bool same = true;

        same = (a.r == b.r) && (a.b == b.b) && (a.g == b.g);

        return same;
    }

    private bool IsInArrayCirle(int centre_X, int centre_Y, int x, int y, int radius)
    {
        var point = Mathf.Pow((x - centre_X), 2) + Mathf.Pow((y - centre_Y), 2);

        return point < Mathf.Pow(radius, 2);
    }

    private void SetPaintedTexture()
    {
        paintTexture.LoadRawTextureData(pixelArray);
        paintTexture.Apply();
        renderer.material.SetTexture("_MainTex", paintTexture);
    }
}
