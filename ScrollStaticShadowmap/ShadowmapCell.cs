using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShadowmapCell
{
    private int offsetX;
    private int offsetZ;
    
    private int currentX;
    private int currentZ;
    private int lod;

    private Transform testItem;
    static private int[] cellOffsetList3_3 = {0, -1, 1}; 
   public ShadowmapCell(int offsetX,int offsetZ, Transform testItem)
   {
       this.offsetX=offsetX;
       this.offsetZ=offsetZ;
       this.testItem = testItem;
   }
   int getShouldPos(int center,int offset)
   {
       return center+cellOffsetList3_3[(center-offset+3*10000)%3];
       //int  value= (center+offset)%3;
       // return center-  (value < 1 ? -value : (value < 2 ? 2 * value - 3 : -value + 3))%2;
   }
    public void updateDraw( )
    {
     var centerPos=   Camera.main.transform.position / 100;
     int centerX = Mathf.RoundToInt(centerPos.x);
     int centerZ = Mathf.RoundToInt(centerPos.z);
     int shouldX = getShouldPos(centerX ,offsetX);
     int shouldZ = getShouldPos(centerZ , offsetZ);
     if (shouldX != currentX || shouldZ != currentZ)
     {
       
         currentX=shouldX;   
         currentZ=shouldZ;  
         
         var pos= testItem.position;
         pos.x = currentX * 100;
         pos.z = currentZ * 100;
         testItem.position = pos;
         Debug.Log("updateDraw");
     }
    }

}
