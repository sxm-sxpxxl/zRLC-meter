using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    partial class ChartCommon
    {
        static partial void DoTextSignInternal(MonoBehaviour Text, double sign)
        {
            var mp = Text.GetComponent<TMPro.TextMeshProUGUI>();
            mp.alignment = (sign > 0f) ? TMPro.TextAlignmentOptions.MidlineLeft : TMPro.TextAlignmentOptions.MidlineRight;
        }
        static partial void SetTextParamsInternal(GameObject obj, string text, int fontSize, float sharpness, ref bool res)
        {
            var mp = obj.GetComponent<TMPro.TextMeshProUGUI>();
            if (mp != null)
            {
                mp.fontSize = (int)(fontSize * sharpness);
                mp.overflowMode = TMPro.TextOverflowModes.Overflow;
                if (text != null)
                    mp.text = text;
                res = true;
                return;
            }
            var mp3d = obj.GetComponent<TMPro.TextMeshPro>();
            if (mp3d != null)
            {
                mp3d.fontSize = (int)(fontSize * sharpness);
                mp3d.overflowMode = TMPro.TextOverflowModes.Overflow;
                if (text != null)
                    mp3d.text = text;
                res = true;
                return;
            }
            res = false;
        }
        static partial void UpdateTextParamsInternal(GameObject obj, string text)
        {
            var mp = obj.GetComponent<TMPro.TextMeshProUGUI>();
            if (mp != null)
                mp.text = text;
            var mp3d = obj.GetComponent<TMPro.TextMeshPro>();
            if (mp3d != null)
                mp3d.text = text;
        }
        static partial void GetTextInternal(GameObject obj, ref string text)
        {
            var mp = obj.GetComponent<TMPro.TextMeshProUGUI>();
            if (mp != null)
            {
                text = mp.text;
                return;
            }
            var mp3d = obj.GetComponent<TMPro.TextMeshPro>();
            if (mp3d != null)
            {
                text = mp3d.text;
                return;
            }
            text = null;
        }
    }
}
