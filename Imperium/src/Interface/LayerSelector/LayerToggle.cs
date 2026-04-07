#region

using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.LayerSelector;

internal class LayerToggle : MonoBehaviour
{
    private GameObject buttonCross;
    private GameObject buttonCheck;
    private GameObject cover;
    private TMP_Text text;

    private int currentLayer;

    private void Awake()
    {
        cover = transform.Find("Hover").gameObject;
        buttonCross = transform.Find("Selected").gameObject;
        buttonCheck = transform.Find("Cross").gameObject;
        text = transform.Find("Text").GetComponent<TMP_Text>();

        GetComponent<RectTransform>();
    }

    internal void Init(string layerName, int layer)
    {
        currentLayer = layer;
        text.text = layerName;
        cover.SetActive(false);

        // Disable if layer is not defined
        gameObject.SetActive(!string.IsNullOrEmpty(layerName));
    }

    internal void SetSelected(bool isSelected) => cover.SetActive(isSelected);

    internal void UpdateIsOn(int layerMask)
    {
        var isEnabled = (layerMask & 1 << currentLayer) != 0;
        buttonCross.gameObject.SetActive(isEnabled);
        buttonCheck.gameObject.SetActive(!isEnabled);
    }
}