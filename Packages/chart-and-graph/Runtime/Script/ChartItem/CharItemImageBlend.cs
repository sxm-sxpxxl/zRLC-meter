using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph
{
    [RequireComponent(typeof(Image))]
    public sealed class CharItemImageBlend : ChartItemLerpEffect
    {
        private Image mImage;
        private CanvasRenderer mRenderer;
        private float mInitialAlphaForImage;
        
        protected override void Start()
        {
            base.Start();
            mImage = GetComponent<Image>();
            mInitialAlphaForImage = mImage.color.a;
            ApplyLerp(0f);
        }

        protected override void ApplyLerp(float value)
        {
            Color c = mImage.color;
            c.a = Mathf.Lerp(0f, mInitialAlphaForImage, value);
            mImage.color = c;
                
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

        protected override float GetStartValue() => mImage.color.a;

        internal override Vector3 ScaleMultiplier => Vector3.one;
        
        internal override Quaternion Rotation => Quaternion.identity;
        
        internal override Vector3 Translation => Vector3.zero;
        
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
