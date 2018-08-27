using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MafiaUnity;

public static class CameraExtension {

    public static ObjectDefinition GetFogBasedOnCameraSectorOccupancy(Vector3 camera)
    {
        var lights = ObjectDefinition.fogLights;

        ObjectDefinition keyFog = null;
        int keyLevel = 0;

        foreach (var fog in lights)
        {
            var sector = fog.transform.parent;

            if (sector != null && sector.name == fog.data.lightSectors)
            {
                var sectorData = sector.GetComponent<ObjectDefinition>();

                if (sectorData != null)
                {
                    if (sectorData.sectorBounds.Contains(camera))
                    {
                        int c = fog.transform.CalculateChildLevel();

                        // NOTE: Primary Sector is considered as top-level sector, so we need to reset child count to account for that:
                        if (sector.name == "Primary Sector")
                        {
                            c = 0;
                        }

                        if (keyLevel <= c)
                        {
                            keyFog = fog;
                            keyLevel = c;
                        }
                    }
                }
            }
            else
            {
                int c = fog.transform.CalculateChildLevel();

                if (keyLevel <= c)
                {
                    keyFog = fog;
                    keyLevel = c;
                }
            }
        }

        return keyFog;
    }

    public static ObjectDefinition GetAmbienceBasedOnCameraSectorOccupancy(Vector3 camera)
    {
        var lights = ObjectDefinition.ambientLights;

        ObjectDefinition keyAmbient = null;
        int keyLevel = 0;

        foreach (var ambient in lights)
        {
            var sector = ambient.transform.parent;

            if (sector != null && sector.name == ambient.data.lightSectors)
            {
                var sectorData = sector.GetComponent<ObjectDefinition>();

                if (sectorData != null)
                {
                    if (sectorData.sectorBounds.Contains(camera))
                    {
                        int c = ambient.transform.CalculateChildLevel();

                        // NOTE: Primary Sector is considered as top-level sector, so we need to reset child count to account for that:
                        if (sector.name == "Primary Sector")
                        {
                            c = 0;
                        }

                        if (keyLevel <= c)
                        {
                            keyAmbient = ambient;
                            keyLevel = c;
                        }
                    }
                }
            }
            else
            {
                int c = ambient.transform.CalculateChildLevel();

                if (keyLevel <= c)
                {
                    keyAmbient = ambient;
                    keyLevel = c;
                }
            }
        }

        return keyAmbient;
    }

    static void UpdateFog(Vector3 camera)
    {
        var keyFog = GetFogBasedOnCameraSectorOccupancy(camera);

        if (keyFog != null)
        {
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, new Color(keyFog.data.lightColour.x, keyFog.data.lightColour.y, keyFog.data.lightColour.z, 1f) * keyFog.data.lightPower, 0.4f * Time.deltaTime);
            RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, keyFog.data.lightNear * 1000f, 0.4f * Time.deltaTime);
            RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, keyFog.data.lightFar * 50f, 0.4f * Time.deltaTime);
            RenderSettings.fogMode = FogMode.Linear;
			RenderSettings.fog = true;
        }
    }

    static void UpdateAmbience(Vector3 camera)
    {
        var keyAmbient = GetAmbienceBasedOnCameraSectorOccupancy(camera);

        if (keyAmbient)
        {
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, new Color(keyAmbient.data.lightColour.x, keyAmbient.data.lightColour.y, keyAmbient.data.lightColour.z, 1f) * keyAmbient.data.lightPower, 0.9f * Time.deltaTime);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        }
    }

    public static void UpdateRenderSettings(this Transform mainCamera)
    {
		var pos = mainCamera.transform.position;

        UpdateFog(pos);
        UpdateAmbience(pos);
    }

}
