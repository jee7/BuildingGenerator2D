﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ST;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BuildingGen2D
{
    public class BuildingGen2D : ScriptableObject
    {
        [SerializeField]
        private List<STSpriteInfo> m_GroundSprites = new List<STSpriteInfo>();
		private List<STSpriteInfo> m_RoofSprites = new List<STSpriteInfo>();
		private GameObject m_building;

		public int MinLength { get; set; }
		public int MaxLength { get; set; }

		public int MinHeight { get; set; }
		public int MaxHeight { get; set; }
		public static float pixelsPerUnit = 100f;
		public float onePixelUnit = 1f / pixelsPerUnit;


        public List<STSpriteInfo> GroundSprites
        {
            get
            {
                return this.m_GroundSprites;
            }
        }

		public List<STSpriteInfo> RoofSprites
		{
			get
			{
				return this.m_RoofSprites;
			}
		}
		
		public List<STSpriteInfo> copyOfGroundSprites
		{
			get
            {
                List<STSpriteInfo> list = new List<STSpriteInfo>();
                foreach (STSpriteInfo i in this.m_GroundSprites)
                    list.Add(i);
                return list;
            }
        }

		public BuildingGen2D() {
			MinLength = 1;
			MaxLength = 1;
			MinHeight = 1;
			MaxHeight = 1;
		}

        public void RemoveGroundSprite(STSpriteInfo info)
        {
            m_GroundSprites.Remove(info);
        }

        public void AddGroundSprite(Object resource)
        {
            if (resource is Texture2D || resource is Sprite)
            {

                STSpriteInfo info = new STSpriteInfo();
                info.source = resource;
                if (resource is Sprite)
                {
                    info.target = resource as Sprite;
                }

                this.GroundSprites.Add(info);
            }
        }

        public void AddGroundSprites(Object[] resources)
        {
            foreach (Object resource in resources)
            {
                this.AddGroundSprite(resource);
            }
        }

        public void AddRectangleSprite(GameObject gameObject, Color32 color, Color32 borderColor, int borderWidth, int width, int height)
        {
			
			Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false, true);
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
			Color32[] pixels = texture.GetPixels32();
			for (int i = 0; i < pixels.Length; i++) {
                if (i <= width * borderWidth || i >= width * height - width * borderWidth
                    || (i % width) < borderWidth || (i % width) >= width - borderWidth)
                {
                    pixels[i] = borderColor;
                }
                else
                {
                    pixels[i] = color;
                }
			}
			texture.SetPixels32(pixels);
			texture.Apply();

			Sprite newSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f),128f);

			gameObject.AddComponent<SpriteRenderer>();
			SpriteRenderer sprRenderer = gameObject.GetComponent<SpriteRenderer>();
			sprRenderer.sprite = newSprite;
		}

		public void GenerateBuilding() {

			if (m_GroundSprites.Count != 0 && m_RoofSprites.Count != 0) {
				if (m_building == null || GameObject.Find (m_building.name) == null) {
					m_building = new GameObject ("Building");
				} else {
					GameObject.DestroyImmediate (GameObject.Find (m_building.name));
					Debug.Log ("deleted gameobject ");
					m_building = new GameObject ("Building");
				}
				int random_length = Random.Range (MinLength, MaxLength + 1);
				int random_height = Random.Range (MinHeight, MaxHeight + 1);
				//Ground generation
				GenerateGround(random_length);

				//Roof generation
				GenerateRoof(random_length, random_height);

				//Building wall generation
				GameObject go2 = new GameObject("Wall");
				go2.transform.position = new Vector3 (0, random_height/2.0f, 0);
				go2.transform.parent = m_building.transform;
                AddRectangleSprite(go2, new Color32(90, 84, 76, 255), new Color32(53, 47, 45, 255), 1, random_length * 128, random_height * 128);
				
			} else {
				Debug.Log ("No Sprites Added!! Try Again!");
			}
			Debug.Log ("Building thingy!");



		}

		public void GenerateGround(int random_length) {

			for (int i = 0; i < random_length; i++) {
				
				GameObject go = new GameObject ("Ground_" + i);
				float x_transformed_beginning = random_length /  -2.0f + 0.5f;
				go.transform.position = new Vector3 (x_transformed_beginning + i, 0, 0);
				//to flip it or not decision
				int posNeg = Random.Range (0, 2);
				if (posNeg == 0) {
					go.transform.localScale = new Vector3 (-1, 1, 1);
				} 
				
				go.transform.parent = m_building.transform;
				SpriteRenderer renderer = go.AddComponent<SpriteRenderer> ();
				int random = Random.Range (0, m_GroundSprites.Count);
				renderer.sprite = m_GroundSprites [random].target;
			}
		
		}

		public void GenerateRoof(int building_width, int building_height) {


			//proovin pilti loigata
			int random = Random.Range (0, m_RoofSprites.Count);
			Sprite roofSprite = m_RoofSprites [random].target;
			//creating new texture from the sprite(this texture is exactly the size of the image, not 128x128)
			Texture2D source = new Texture2D((int)roofSprite.rect.width,(int)roofSprite.rect.height); 
			source.filterMode = FilterMode.Point; //makes it into pixels. Basically same as TextureFormat truecolor

			Color[] newColors = roofSprite.texture.GetPixels((int)roofSprite.rect.x, 
			                                                                    (int)roofSprite.rect.y, 
			                                                                    (int)roofSprite.rect.width, 
			                                                                    (int)roofSprite.rect.height);

			source.SetPixels(newColors);
			source.Apply();

			int sourceHeight = source.height;
			int sourceWidth = source.width;
			//Debug.Log ("Roofe " + newText.height);
			//Important!! in Rect the values start form the bottom left corner
			Sprite leftRoofTip = Sprite.Create(source, new Rect(0f, 0f, 2f, sourceHeight), new Vector2(0f, 1f));//moving point is in the upper left corner (0,1)
			GameObject roof_left = new GameObject ("Roof_left_0");
			SpriteRenderer renderer_l = roof_left.AddComponent<SpriteRenderer> ();
			renderer_l.sprite = leftRoofTip;
			roof_left.transform.position = new Vector3 (-building_width / 2f - 2 * onePixelUnit, building_height + onePixelUnit, 0);
			//roof_left.transform.localScale = new Vector3 (2, 2, 1);
			roof_left.transform.parent = m_building.transform;

			//greate texture for middlepart
			Sprite middlePart = Sprite.Create(source, new Rect(2f, 0f, sourceWidth - 4f, sourceHeight), new Vector2(0f, 1f));//moving point is in the upper left corner
			//Debug.Log ("-- testing   " + middlePart.rect.width);
			//creating new texture from the sprite
			Texture2D middleRoof_texture = new Texture2D((int)middlePart.rect.width,(int)middlePart.rect.height); 
			middleRoof_texture.filterMode = FilterMode.Point; //makes it into pixels. Basically same as TextureFormat truecolor
			
			Color[] middleColors = middlePart.texture.GetPixels((int)middlePart.rect.x, 
			                                                    (int)middlePart.rect.y, 
                                                               (int)middlePart.rect.width, 
                                                               (int)middlePart.rect.height);
			
			middleRoof_texture.SetPixels(middleColors);
			middleRoof_texture.Apply();

			//roof middle part generation

			Sprite correctLength_middlePart = Sprite.Create(middleRoof_texture, new Rect(0f, 0f, building_width * pixelsPerUnit, sourceHeight), new Vector2(0f, 1f));
			GameObject roof_mid = new GameObject ("Roof_mid_0");
			SpriteRenderer sr = roof_mid.AddComponent<SpriteRenderer> ();
			sr.sprite = correctLength_middlePart;
			roof_mid.transform.position = new Vector3 (-building_width / 2f, building_height + onePixelUnit, 0);

			roof_mid.transform.parent = m_building.transform;

			Sprite rightRoofTip = Sprite.Create(source, new Rect(sourceWidth - 2f, 0f, 2, sourceHeight), new Vector2(0f, 1f));//moving point is in the upper left corner (0,1)
			GameObject roof_right = new GameObject ("Roof_right_0");
			SpriteRenderer renderer_r = roof_right.AddComponent<SpriteRenderer> ();
			renderer_r.sprite = rightRoofTip;
			roof_right.transform.position = new Vector3 (building_width / 2f, building_height + onePixelUnit, 0);
			roof_right.transform.parent = m_building.transform;
			
		}
    }
}
