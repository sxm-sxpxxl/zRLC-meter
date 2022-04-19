using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph
{
    public class ChartItemTextBlend : ChartItemLerpEffect
    {
        [SerializeField] private Image image;
        
        private Text mText;
        private Shadow[] mShadows;
        
        private Dictionary<UnityEngine.Object, float> mInitialValues = new Dictionary<UnityEngine.Object, float>();
        private CanvasRenderer mRenderer = null;
        private float mInitialAlphaForImage = 0f;

        protected override void Start()
        {
            base.Start();
            
            mText = GetComponent<Text>();
            mShadows = GetComponents<Shadow>();
            mInitialAlphaForImage = image != null ? image.color.a : 0f;

            foreach (Shadow s in mShadows)
            {
                mInitialValues.Add(s, s.effectColor.a);
            }

            ApplyLerp(0f);
        }
        
        internal override Quaternion Rotation => Quaternion.identity;

        internal override Vector3 ScaleMultiplier => Vector3.one;

        internal override Vector3 Translation => Vector3.zero;

        protected override float GetStartValue() => mText != null ? mText.color.a : 0f;

        protected override void ApplyLerp(float value)
        {
            for (int i = 0; i < mShadows.Length; i++)
            {
                Shadow s = mShadows[i];
                float inital;
                if (mInitialValues.TryGetValue(s, out inital) == false)
                    continue;
                Color c = s.effectColor;
                c.a = Mathf.Lerp(0f, inital, value);
                s.effectColor = c;
            }

            if (image != null)
            {
                Color c = image.color;
                c.a = Mathf.Lerp(0f, mInitialAlphaForImage, value);
                image.color = c;
                
                CanvasRenderer rend = EnsureRenderer();
                if (rend != null)
                {
                    if (value <= 0f)
                    {
                        if (rend.cull == false)
                            rend.cull = true;
                    }
                    else
                    {
                        if (rend.cull == true)
                            rend.cull = false;
                    }
                }
            }
            
            if (mText != null)
            {
                Color c = mText.color;
                c.a = Mathf.Clamp(value,0f,1f);
                mText.color = c;
                
                CanvasRenderer rend = EnsureRenderer();
                if (rend != null)
                {
                    if (value <= 0f)
                    {
                        if (rend.cull == false)
                            rend.cull = true;
                    }
                    else
                    {
                        if (rend.cull == true)
                            rend.cull = false;
                    }
                }
            }
        }
        
        private CanvasRenderer EnsureRenderer()
        {
            if (mRenderer == null)
            {
                mRenderer = GetComponent<CanvasRenderer>();   
            }
            
            return mRenderer;
        }
    }
}
