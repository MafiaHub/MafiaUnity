using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace MafiaUnity
{
	public class CityDefinition : MonoBehaviour
	{
		public MafiaUnity.MafiaFormats.CacheBINLoader data;
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(CityDefinition))]
	public class CityEditor : Editor
	{
		string exportPath = "missions/freeride/cache.bin";

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			var def = target as CityDefinition;
			
			exportPath = def.transform.name;

			if (def == null)
				return;

			var data = def.data;

			GUILayout.Label("Number of objects: " + def.transform.childCount);
            GUILayout.Label("Number of cached objects: " + data.objects.Count);

			if (def.transform.childCount != data.objects.Count)
			{
                GUILayout.Label("Objects are not in a sync, perform update!");
			}

			if (GUILayout.Button("Update object list"))
			{
				UpdateObjectList(def, data);
			}

			GUILayout.BeginHorizontal();


			if (GUILayout.Button("Save cache.bin"))
			{
                UpdateObjectList(def, data);
				data.WriteCache(exportPath);
			}

            GUILayout.EndHorizontal();
		}

		void UpdateObjectList(CityDefinition def, MafiaFormats.CacheBINLoader data)
		{
			def.data.objects.Clear();

            foreach (Transform child in def.transform)
            {
                var newObject = new MafiaFormats.CacheBINLoader.Object();
                newObject.objectName = child.name;
                newObject.bounds = new byte[0x4C];
                newObject.header = new MafiaFormats.CacheBINLoader.Header();

                var instances = new List<MafiaFormats.CacheBINLoader.Instance>();

                foreach (Transform inst in child)
                {
					
                    var newInstance = new MafiaFormats.CacheBINLoader.Instance();
                    newInstance.header = new MafiaFormats.CacheBINLoader.Header();
                    
					var modelDef = inst.GetComponentInChildren<ModelDefinition>();

					if (modelDef != null)
						newInstance.modelName = modelDef.modelName;

                    newInstance.pos = inst.position;
                    newInstance.rot = inst.rotation;

                    newInstance.scale = inst.localScale;
                    newInstance.scale2 = inst.localScale;
                    newInstance.unk0 = 0;

					newInstance.header.size = (uint)(6 /* header */ 
													+ 24 /* scale+scale2 */ 
													+ 16 /* rot */
													+ 12 /* pos */
													+ 4 /* unk0 */
													+ 4 /* modelName.Length */
													+ newInstance.modelName.Length);

                    instances.Add(newInstance);
                }

				newObject.header.size = (uint)(sizeof(ushort) + sizeof(uint)*2 + newObject.objectName.Length + 0x4C 
					+ instances.Sum(x => x.header.size));

                newObject.instances = instances;

				data.objects.Add(newObject);
            }
		}
	}
#endif
}