using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public class LevelGenerator : MonoBehaviour
    {

        [SerializeField]
        private LevelArranger levelArranger;

        [SerializeField]
        private ColorToPrefab[] colorMappings;

        //helpers
        private Texture2D mapReference;
        private List<PanelData> panelDatas = new List<PanelData>();
        private GameObject panelPlaceHolder;


        public void GenerateLevel(Texture2D map,TileSet tileSet)
        {
            panelDatas.Clear();
            mapReference = map;
            for (int x = 0; x < mapReference.width; x++)
            {
                for (int z = 0; z < mapReference.height; z++)
                {
                    GeneratePanel(x, z);
                }
            }
            levelArranger.ArrangeLevel(panelDatas,tileSet);
        }
        public static bool ColorEquals(Color a, Color b, float tolerance = 0.04f)
        {
            if (a.r > b.r + tolerance) return false;
            if (a.g > b.g + tolerance) return false;
            if (a.b > b.b + tolerance) return false;
            if (a.r < b.r - tolerance) return false;
            if (a.g < b.g - tolerance) return false;
            if (a.b < b.b - tolerance) return false;

            return true;
        }
        private void GeneratePanel(int x, int z)
        {
            Color pixelColor = mapReference.GetPixel(x, z);

            if (pixelColor.a == 0)
            {
                return;
            }

            foreach (ColorToPrefab colorMapping in colorMappings)
            {
                if (ColorEquals(pixelColor,colorMapping.Color))
                {
                    Vector3 position = new Vector3(x, 0, z);
                    panelPlaceHolder = Instantiate(colorMapping.Prefab,position,Quaternion.identity, transform);
                    panelPlaceHolder.GetComponent<PanelData>().Panel = panelPlaceHolder;
                    panelDatas.Add(panelPlaceHolder.GetComponent<PanelData>());
                }
            }
        }
    }
}
