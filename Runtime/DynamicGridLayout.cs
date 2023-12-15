using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryEngine
{

    public class DynamicGridLayout : GridLayoutGroup
    {
        [HideInInspector] public bool dynamic = true;

        
        public override void SetLayoutVertical()
        {
            if(dynamic)
            {
                m_StartCorner = Corner.UpperLeft;
                m_StartAxis = Axis.Vertical;
            }
            SetCellsAlongAxis(0);
        }

        public override void SetLayoutHorizontal()
        {
            if (dynamic)
            {
                m_StartCorner = Corner.UpperLeft;
                m_StartAxis = Axis.Vertical;
            }
            SetCellsAlongAxis(1);
        }

        protected void SetCellsAlongAxis(int axis)
        {
            if (axis == 0)
            {
                for (int index = 0; index < this.rectChildren.Count; ++index)
                {
                    RectTransform rectChild = this.rectChildren[index];
                    this.m_Tracker.Add((Object)this, rectChild, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
                    rectChild.anchorMin = Vector2.up;
                    rectChild.anchorMax = Vector2.up;
                    rectChild.sizeDelta = (dynamic) ? rectChildren[index].sizeDelta : cellSize;
                }
            }
            else
            {
                float x = this.rectTransform.rect.size.x;
                float y = this.rectTransform.rect.size.y;
                int num1;
                int num2;
                if (this.m_Constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                {
                    num1 = this.m_ConstraintCount;
                    num2 = Mathf.CeilToInt((float)((double)this.rectChildren.Count / (double)num1 - 1.0 / 1000.0));
                }
                else if (this.m_Constraint == GridLayoutGroup.Constraint.FixedRowCount)
                {
                    num2 = this.m_ConstraintCount;
                    num1 = Mathf.CeilToInt((float)((double)this.rectChildren.Count / (double)num2 - 1.0 / 1000.0));
                }
                else
                {
                    num1 = (double)this.cellSize.x + (double)this.spacing.x > 0.0 ? Mathf.Max(1, Mathf.FloorToInt((float)(((double)x - (double)this.padding.horizontal + (double)this.spacing.x + 1.0 / 1000.0) / ((double)this.cellSize.x + (double)this.spacing.x)))) : int.MaxValue;
                    num2 = (double)this.cellSize.y + (double)this.spacing.y > 0.0 ? Mathf.Max(1, Mathf.FloorToInt((float)(((double)y - (double)this.padding.vertical + (double)this.spacing.y + 1.0 / 1000.0) / ((double)this.cellSize.y + (double)this.spacing.y)))) : int.MaxValue;
                }
                int num3 = (int)this.startCorner % 2;
                int num4 = (int)this.startCorner / 2;
                int num5;
                int num6;
                int num7;
                if (this.startAxis == GridLayoutGroup.Axis.Horizontal)
                {
                    num5 = num1;
                    num6 = Mathf.Clamp(num1, 1, this.rectChildren.Count);
                    num7 = Mathf.Clamp(num2, 1, Mathf.CeilToInt((float)this.rectChildren.Count / (float)num5));
                }
                else
                {
                    num5 = num2;
                    num7 = Mathf.Clamp(num2, 1, this.rectChildren.Count);
                    num6 = Mathf.Clamp(num1, 1, Mathf.CeilToInt((float)this.rectChildren.Count / (float)num5));
                }
                Vector2 vector2_1 = new Vector2((float)((double)num6 * (double)this.cellSize.x + (double)(num6 - 1) * (double)this.spacing.x), (float)((double)num7 * (double)this.cellSize.y + (double)(num7 - 1) * (double)this.spacing.y));
                Vector2 vector2_2 = new Vector2(this.GetStartOffset(0, vector2_1.x), this.GetStartOffset(1, vector2_1.y));
                for (int index = 0; index < this.rectChildren.Count; ++index)
                {
                    int num8;
                    int num9;
                    if (this.startAxis == GridLayoutGroup.Axis.Horizontal)
                    {
                        num8 = index % num5;
                        num9 = index / num5;
                    }
                    else
                    {
                        num8 = index / num5;
                        num9 = index % num5;
                    }
                    if (num3 == 1)
                        num8 = num6 - 1 - num8;
                    if (num4 == 1)
                        num9 = num7 - 1 - num9;

                    float sizeX = (dynamic) ? rectChildren[index].sizeDelta[0] : cellSize[0];
                    float sizeY = (dynamic) ? rectChildren[index].sizeDelta[1] : cellSize[1];

                    this.SetChildAlongAxis(this.rectChildren[index], 0, vector2_2.x + (this.cellSize[0] + this.spacing[0]) * (float)num8, sizeX);
                    this.SetChildAlongAxis(this.rectChildren[index], 1, vector2_2.y + (this.cellSize[1] + this.spacing[1]) * (float)num9, sizeY);
                }
            }
        }
    }
}