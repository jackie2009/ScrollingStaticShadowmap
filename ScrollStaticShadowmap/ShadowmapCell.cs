using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.jackie2009.scrollStaticShadowmap
{
    public class ShadowmapCell
    {
        private int offsetX;
        private int offsetZ;

        private int currentX ;
        private int currentZ ;
        private int lod;
        private int index;
        private Transform testItem;
        private float cellSize=10;
        static private int[] cellOffsetList3_3 = {0, -1, 1};

        public ShadowmapCell(int offsetX, int offsetZ, Transform testItem,float cellSize)
        {
            resetCurrentPos();
            this.cellSize = cellSize;
            this.offsetX = offsetX;
            this.offsetZ = offsetZ;
            index = (offsetZ + 1) * 3 + (offsetX + 1);
            this.testItem = testItem;
        }

        int getShouldPos(int center, int offset)
        {
            return center + cellOffsetList3_3[(center - offset + 3 * 10000) % 3];
            //int  value= (center+offset)%3;
            // return center-  (value < 1 ? -value : (value < 2 ? 2 * value - 3 : -value + 3))%2;
        }

        public void resetCurrentPos()
        {
          currentX = -1000;
          currentZ = -1000;
        }

        public void updateDraw(StaticShadowCaster shadowCaster)
        {
            var centerPos = Camera.main.transform.position / cellSize;
            int centerX = (int) Mathf.Floor(centerPos.x + 0.5f);
            int centerZ = (int) Mathf.Floor(centerPos.z + 0.5f);
            int shouldX = getShouldPos(centerX, offsetX);
            int shouldZ = getShouldPos(centerZ, offsetZ);
            if (shouldX != currentX || shouldZ != currentZ)
            {

                currentX = shouldX;
                currentZ = shouldZ;

                var pos = testItem.position;
                pos.x = currentX * cellSize;
                pos.z = currentZ * cellSize;
                testItem.position = pos;
                shadowCaster.renderWithIndex(index, pos);
               // Debug.Log("updateDraw");
            }
        }

    }
}