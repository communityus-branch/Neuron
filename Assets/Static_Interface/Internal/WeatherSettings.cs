using Artngame.GIPROXY;
using Artngame.SKYMASTER;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.Internal
{
    public class WeatherSettings : MonoBehaviour
    {
        public float GlobalScale = 10000;//scale of globe, used for the dome system
        public Object SunSystemPREFAB;
        public float SunSystemScale = 11f;
        public Material MoonPhasesMat;
        public GameObject Upper_Dynamic_CloudPREFAB;
        public GameObject Lower_Dynamic_CloudPREFAB;
        public GameObject Upper_Cloud_BedPREFAB;
        public GameObject Lower_Cloud_BedPREFAB;
        public GameObject Upper_Cloud_RealPREFAB;
        public GameObject Lower_Cloud_RealPREFAB;
        public GameObject Upper_Static_CloudPREFAB;
        public GameObject Lower_Static_CloudPREFAB;
        public GameObject Surround_CloudsPREFAB;
        public GameObject Surround_Clouds_HeavyPREFAB;

        public GameObject Sun_Ray_CloudPREFAB;
        public GameObject Cloud_DomePREFAB;
        public GameObject Star_particlesPREFAB;
        public GameObject Star_domePREFAB;
        public GameObject Cloud_Domev22PREFAB1;
        public GameObject Cloud_Domev22PREFAB2;
        public GameObject VolumeRain_Heavy;
        public GameObject VolumeRain_Mild;
        public GameObject RefractRain_Heavy;
        public GameObject RefractRain_Mild;
        public GameObject Rain_Heavy;
        public GameObject Rain_Mild;

        public GameObject SnowStormPREFAB;
        public GameObject FallingLeavesPREFAB;
        public GameObject ButterflyPREFAB;
        public GameObject TornadoPREFAB;
        public GameObject Butterfly3DPREFAB;
        public GameObject Ice_SpreadPREFAB;
        public GameObject Ice_SystemPREFAB;
        public GameObject Lightning_SystemPREFAB;
        public GameObject LightningPREFAB;//single lightning to instantiate 
        public GameObject VolcanoPREFAB;
        public GameObject VolumeFogPREFAB;
        public Material StarsMat;

        public Material skyboxMat;
        public Material skyMatDUAL_SUN;

        public Object HeavyStormVOLUME_CLOUD2;
        public Object DustyStormVOLUME_CLOUD2;
        public Object DayClearVOLUME_CLOUD2;
        public Object SnowStormVOLUME_CLOUD2;
        public Object SnowVOLUME_CLOUD2;
        public Object RainStormVOLUME_CLOUD2;
        public Object RainVOLUME_CLOUD2;
        public Object PinkVOLUME_CLOUD2;
        public Object LightningVOLUME_CLOUD2;
        public Material UnityTerrainSnowMat;
        private SkyMaster skyMaster;

        public GameObject FogPresetGradient1_5;
        public GameObject FogPresetGradient6;
        public GameObject FogPresetGradient7;
        public GameObject FogPresetGradient8;
        public GameObject FogPresetGradient9;
        public GameObject FogPresetGradient10;
        public GameObject FogPresetGradient11;
        public GameObject FogPresetGradient12;
        public GameObject FogPresetGradient13;
        public GameObject FogPresetGradient14;
        public GameObject FogPresetGradient15;//v3.3
        public Object UnityTreePrefab;
        public Material MeshTerrainSnowMat;

        public void SetupSky(SkyMaster script)
        {
            var mapCenter = GameObject.Find("Map Center");
            if(mapCenter == null)
                mapCenter = new GameObject("Map Center");
            var mgr = script.gameObject.AddComponent<SkyMasterManager>();
            script.SkyManager = mgr;
            script.SkyManager.MapCenter = mapCenter.transform;
            script.SkyManager = script.gameObject.GetComponent<SkyMasterManager>();
            script.SkyManager.PlanetScale = GlobalScale; 
            script.SkyManager.DefinePlanetScale = true;
            script.SkyManager.Unity5 = true;

            script.transform.position = script.SkyManager.MapCenter.position;

            script.SkyManager.SunSystem = (GameObject) Instantiate(SunSystemPREFAB);
            script.SkyManager.SunSystem.transform.position = script.SkyManager.MapCenter.transform.position;

            script.SkyManager.Current_Time = 20.5f;

            script.SkyManager.SunSystem.transform.eulerAngles = new Vector3(28.14116f, 170, 180);
            script.SkyManager.SunSystem.name = "Sun System";
            script.SkyManager.SunSystem.transform.localScale = SunSystemScale*Vector3.one;
            script.SkyManager.SunSystem.transform.parent = script.transform;

            script.SkyManager.MoonPhasesMat = MoonPhasesMat;
            script.SkyManager.MoonPhases = true;

            // --------------- FIND SUN SYSTEM CENTER AND ALIGN TO MAP CENTER --------------- 
            GameObject SunSystemCenter = FindInChildren(script.SkyManager.SunSystem, "Sun Target");
            Vector3 Distance = SunSystemCenter.transform.position - script.SkyManager.MapCenter.transform.position;
            script.SkyManager.SunSystem.transform.position -= Distance;
            script.SkyManager.SunTarget = SunSystemCenter;

            // --------------- ASSIGN SUN - MOON LIGHTS TO SKY MANAGER --------------- 
            script.SkyManager.SunObj = FindInChildren(script.SkyManager.SunSystem, "SunBody");
            script.SkyManager.MoonObj = FindInChildren(script.SkyManager.SunSystem, "MoonBody");
            script.SkyManager.SUN_LIGHT = script.SkyManager.SunObj.transform.parent.gameObject;
            script.SkyManager.SUPPORT_LIGHT = FindInChildren(script.SkyManager.SUN_LIGHT, "MoonLight");
            script.SkyManager.MOON_LIGHT = FindInChildren(FindInChildren(script.SkyManager.SunSystem, "MoonBody"),
                "MoonLight");

            //ADD CLOUDS
            script.SkyManager.Upper_Dynamic_Cloud =
                (GameObject)
                    Instantiate(Upper_Dynamic_CloudPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Upper_Dynamic_Cloud.transform.parent = script.transform;
            script.SkyManager.Lower_Dynamic_Cloud =
                (GameObject)
                    Instantiate(Lower_Dynamic_CloudPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Lower_Dynamic_Cloud.transform.parent = script.transform;
            script.SkyManager.Upper_Cloud_Bed =
                (GameObject)
                    Instantiate(Upper_Cloud_BedPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Upper_Cloud_Bed.transform.parent = script.transform;
            script.SkyManager.Lower_Cloud_Bed =
                (GameObject)
                    Instantiate(Lower_Cloud_BedPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Lower_Cloud_Bed.transform.parent = script.transform;
            script.SkyManager.Upper_Cloud_Real =
                (GameObject)
                    Instantiate(Upper_Cloud_RealPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Upper_Cloud_Real.transform.parent = script.transform;
            script.SkyManager.Lower_Cloud_Real =
                (GameObject)
                    Instantiate(Lower_Cloud_RealPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Lower_Cloud_Real.transform.parent = script.transform;
            script.SkyManager.Upper_Static_Cloud =
                (GameObject)
                    Instantiate(Upper_Static_CloudPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Upper_Static_Cloud.transform.parent = script.transform;
            script.SkyManager.Lower_Static_Cloud =
                (GameObject)
                    Instantiate(Lower_Static_CloudPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Lower_Static_Cloud.transform.parent = script.transform;
            script.SkyManager.Surround_Clouds =
                (GameObject)
                    Instantiate(Surround_CloudsPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Surround_Clouds.transform.parent = script.transform;
            script.SkyManager.Surround_Clouds_Heavy =
                (GameObject)
                    Instantiate(Surround_Clouds_HeavyPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Surround_Clouds_Heavy.transform.parent = script.transform;

            script.SkyManager.cloud_upMaterial =
                script.SkyManager.Upper_Dynamic_Cloud.GetComponentsInChildren<ParticleSystemRenderer>(true)[0]
                    .sharedMaterial;
            script.SkyManager.cloud_downMaterial =
                script.SkyManager.Upper_Dynamic_Cloud.GetComponentsInChildren<ParticleSystemRenderer>(true)[0]
                    .sharedMaterial;
            script.SkyManager.flat_cloud_upMaterial =
                script.SkyManager.Upper_Cloud_Bed.GetComponentsInChildren<ParticleSystemRenderer>(true)[0]
                    .sharedMaterial;
            script.SkyManager.flat_cloud_downMaterial =
                script.SkyManager.Lower_Cloud_Bed.GetComponentsInChildren<ParticleSystemRenderer>(true)[0]
                    .sharedMaterial;
            script.SkyManager.Surround_Clouds_Mat =
                script.SkyManager.Surround_Clouds.GetComponentsInChildren<ParticleSystemRenderer>(true)[0]
                    .sharedMaterial;
            script.SkyManager.real_cloud_upMaterial =
                script.SkyManager.Upper_Cloud_Real.GetComponentsInChildren<ParticleSystemRenderer>(true)[0]
                    .sharedMaterial;
            script.SkyManager.real_cloud_downMaterial =
                script.SkyManager.Lower_Cloud_Real.GetComponentsInChildren<ParticleSystemRenderer>(true)[0]
                    .sharedMaterial;

            script.SkyManager.Sun_Ray_Cloud =
                (GameObject) Instantiate(Sun_Ray_CloudPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Sun_Ray_Cloud.transform.parent = script.transform;
            script.SkyManager.Cloud_Dome =
                (GameObject) Instantiate(Cloud_DomePREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Cloud_Dome.transform.parent = script.transform;
            script.SkyManager.Star_particles_OBJ =
                (GameObject)
                    Instantiate(Star_particlesPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Star_particles_OBJ.transform.parent = script.transform;
            script.SkyManager.Star_particles_OBJ.transform.position = script.SkyManager.MapCenter.position;

            script.SkyManager.StarDome =
                (GameObject) Instantiate(Star_domePREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.StarDome.transform.parent = script.transform;
            script.SkyManager.CloudDomeL1 =
                (GameObject)
                    Instantiate(Cloud_Domev22PREFAB1, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.CloudDomeL1.transform.parent = script.transform;
            script.SkyManager.CloudDomeL2 =
                (GameObject)
                    Instantiate(Cloud_Domev22PREFAB2, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.CloudDomeL2.transform.parent = script.transform;

            script.SkyManager.cloud_dome_downMaterial =
                script.SkyManager.Cloud_Dome.GetComponentsInChildren<Renderer>(true)[0].sharedMaterial;
            script.SkyManager.star_dome_Material =
                script.SkyManager.StarDome.GetComponentsInChildren<Renderer>(true)[0].sharedMaterial;
            //script.SkyManager.star = script.SkyManager.Lower_Cloud_Real.GetComponentsInChildren<Renderer>(true)[0].sharedMaterial;

            script.SkyManager.CloudDomeL1Mat =
                script.SkyManager.CloudDomeL1.GetComponentsInChildren<Renderer>(true)[0].sharedMaterial;
            script.SkyManager.CloudDomeL2Mat =
                script.SkyManager.CloudDomeL2.GetComponentsInChildren<Renderer>(true)[0].sharedMaterial;

            //DUAL SUN HANDLE

            //STARS SHADER
            script.SkyManager.StarsMaterial = StarsMat;

            //FOGS
            script.SkyManager.VFogsPerVWeather.Add(0); //sunny--
            script.SkyManager.VFogsPerVWeather.Add(13); //foggy
            script.SkyManager.VFogsPerVWeather.Add(14); //heavy fog
            script.SkyManager.VFogsPerVWeather.Add(0); //tornado
            script.SkyManager.VFogsPerVWeather.Add(7); //snow storm--

            script.SkyManager.VFogsPerVWeather.Add(7); //freeze storm
            script.SkyManager.VFogsPerVWeather.Add(0); //flat clouds
            script.SkyManager.VFogsPerVWeather.Add(0); //lightning storm
            script.SkyManager.VFogsPerVWeather.Add(7); //heavy storm--
            script.SkyManager.VFogsPerVWeather.Add(7); //heavy storm dark--

            script.SkyManager.VFogsPerVWeather.Add(12); //cloudy--
            script.SkyManager.VFogsPerVWeather.Add(0); //rolling fog
            script.SkyManager.VFogsPerVWeather.Add(0); //volcano erupt
            script.SkyManager.VFogsPerVWeather.Add(14); //rain

            //LOCALIZE EFFECTS
            script.SkyManager.Snow_local = true;
            script.SkyManager.Mild_rain_local = true;
            script.SkyManager.Heavy_rain_local = true;
            script.SkyManager.Fog_local = true;
            script.SkyManager.Butterflies_local = true;

            //ADD RAIN
            script.SkyManager.SnowStorm_OBJ =
                (GameObject) Instantiate(SnowStormPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.SnowStorm_OBJ.transform.parent = script.transform;
            script.SkyManager.Butterfly_OBJ =
                (GameObject) Instantiate(ButterflyPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Butterfly_OBJ.transform.parent = script.transform;

            //Parent ice spread to snow storm system and assign pool particle to collision manager
            //v.3.0.3	script.SkyManager.Ice_System_OBJ=(GameObject)Instantiate(Ice_SystemPREFAB); script.SkyManager.Ice_System_OBJ.transform.parent = script.transform;
            //v.3.0.3	script.SkyManager.Ice_Spread_OBJ=(GameObject)Instantiate(Ice_SpreadPREFAB); script.SkyManager.Ice_Spread_OBJ.transform.parent = script.SkyManager.SnowStorm_OBJ.transform;
            //v.3.0.3	script.SkyManager.Ice_Spread_OBJ.GetComponentsInChildren<ParticleCollisionsSKYMASTER>(true)[0].ParticlePOOL = script.SkyManager.Ice_System_OBJ.GetComponentsInChildren<ParticlePropagationSKYMASTER>(true)[0].gameObject;


            script.SkyManager.Lightning_OBJ =
                (GameObject) Instantiate(LightningPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Lightning_OBJ.transform.parent = script.transform;

            script.SkyManager.Lightning_System_OBJ =
                (GameObject)
                    Instantiate(Lightning_SystemPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Lightning_System_OBJ.transform.parent = script.transform;
            script.SkyManager.VolumeFog_OBJ =
                (GameObject) Instantiate(VolumeFogPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.VolumeFog_OBJ.transform.parent = script.transform;
            script.SkyManager.Rain_Heavy =
                (GameObject) Instantiate(Rain_Heavy, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Rain_Heavy.transform.parent = script.transform;
            script.SkyManager.Rain_Mild =
                (GameObject) Instantiate(Rain_Mild, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Rain_Mild.transform.parent = script.transform;
            script.SkyManager.VolumeRain_Heavy =
                (GameObject) Instantiate(VolumeRain_Heavy, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.VolumeRain_Heavy.transform.parent = script.transform;
            script.SkyManager.VolumeRain_Mild =
                (GameObject) Instantiate(VolumeRain_Mild, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.VolumeRain_Mild.transform.parent = script.transform;

            //init arrays
            script.SkyManager.FallingLeaves_OBJ = new GameObject[1];
            script.SkyManager.Tornado_OBJs = new GameObject[1];
            //v.3.0.3 script.SkyManager.Butterfly3D_OBJ = new GameObject[1];
            script.SkyManager.Volcano_OBJ = new GameObject[1];

            script.SkyManager.FallingLeaves_OBJ[0] =
                (GameObject) Instantiate(FallingLeavesPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.FallingLeaves_OBJ[0].transform.parent = script.transform;
            script.SkyManager.Tornado_OBJs[0] =
                (GameObject) Instantiate(TornadoPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Tornado_OBJs[0].transform.parent = script.transform;
            //v.3.0.3			script.SkyManager.Butterfly3D_OBJ[0]	=(GameObject)Instantiate(Butterfly3DPREFAB); script.SkyManager.Butterfly3D_OBJ[0].transform.parent = script.transform;						
            script.SkyManager.Volcano_OBJ[0] =
                (GameObject) Instantiate(VolcanoPREFAB, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.Volcano_OBJ[0].transform.parent = script.transform;

            script.SkyManager.RefractRain_Mild =
                (GameObject) Instantiate(RefractRain_Mild, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.RefractRain_Mild.transform.parent = script.transform;
            script.SkyManager.RefractRain_Heavy =
                (GameObject) Instantiate(RefractRain_Heavy, script.SkyManager.MapCenter.position, Quaternion.identity);
            script.SkyManager.RefractRain_Heavy.transform.parent = script.transform;

            //SETUP GI PROXY for sun
            //SET PLAYER
            if (!script.SkyManager.Tag_based_player)
            {
                script.SkyManager.SUN_LIGHT.GetComponent<LightCollisionsPDM>().Tag_based_player = false;
                if (script.SkyManager.Hero != null)
                {
                    script.SkyManager.SUN_LIGHT.GetComponent<LightCollisionsPDM>().HERO = script.SkyManager.Hero;
                }
                else
                {
                    Debug.Log(
                        "Note: Hero has not been defined. The Main Camera will be used as the player. For a specific player usage, define a hero in 'hero' parameters in Sky Master Manager and LightColliionsPDM scripts.");
                    Debug.Log(
                        "The 'tag based player' option may also be used to define player by tag (default tag is 'Player')");
                }
            }
            else
            {
                script.SkyManager.SUN_LIGHT.GetComponent<LightCollisionsPDM>().Tag_based_player = true;
            }

            //DEFINE START PRESET
            script.SkyManager.Preset = 9; //v3.0 day time - no red sun
            script.SkyManager.Auto_Cycle_Sky = true;
            script.SkyManager.SPEED = 1;
            script.SkyManager.currentWeatherName = SkyMasterManager.Volume_Weather_types.Cloudy;

            //ASSIGN SKY MATERIALS
            if (script.SkyManager.SunObj2 != null)
            {
                script.SkyManager.skyboxMat = skyMatDUAL_SUN;
            }
            else
            {
                script.SkyManager.skyboxMat = skyboxMat;
            }
            RenderSettings.skybox = script.SkyManager.skyboxMat;
            //Debug.Log(script.SkyManager.skyMat.name);
            //Debug.Log(RenderSettings.skybox.name);
            //script.SkyManager.skyMat = skyMat;		//v3.3c

            //Set fog present to realitistic
            script.SkyManager.Preset = 11;
            if (script.TerrainManager != null)
            {
                script.TerrainManager.FogPreset = 11;
            }
            skyMaster = script;
            SetupCloudsRealistic();
        }

        public void SetupTerrain(Terrain terrain)
        {
            skyMaster.UnityTerrains.Clear();
            skyMaster.UnityTerrains.Add(terrain);
            foreach (Terrain t in skyMaster.UnityTerrains)
            {
                t.materialType = Terrain.MaterialType.Custom;
                t.materialTemplate = UnityTerrainSnowMat;
            }
            skyMaster.SkyManager.Unity_terrain = skyMaster.UnityTerrains[0].transform;

            skyMaster.SkyManager.Unity_terrain.gameObject.AddComponent<SeasonalTerrainSKYMASTER>();
            skyMaster.TerrainManager = skyMaster.SkyManager.Unity_terrain.gameObject.GetComponent<SeasonalTerrainSKYMASTER>();
            skyMaster.TerrainManager.TerrainMat = UnityTerrainSnowMat;

            skyMaster.SkyManager.SnowMat = MeshTerrainSnowMat;
            skyMaster.SkyManager.SnowMatTerrain = UnityTerrainSnowMat;

            skyMaster.TerrainManager.TreePefabs.Add(UnityTreePrefab as GameObject);
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient1_5.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient1_5.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient1_5.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient1_5.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient1_5.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient1_5.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient6.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient7.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient8.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient9.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient10.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient11.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient12.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient13.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient14.GetComponent<GlobalFogSkyMaster>());
            skyMaster.TerrainManager.GradientHolders.Add(FogPresetGradient15.GetComponent<GlobalFogSkyMaster>());

            skyMaster.TerrainManager.Mesh_moon = true;
            skyMaster.TerrainManager.Glow_moon = true;
            skyMaster.TerrainManager.Enable_trasition = true;
            skyMaster.TerrainManager.Fog_Sky_Update = true;
            skyMaster.TerrainManager.Foggy_Terrain = true;
            skyMaster.TerrainManager.Use_both_fogs = true;
            skyMaster.TerrainManager.SkyManager = skyMaster.SkyManager;

            if (skyMaster.TerrainManager != null && skyMaster.WaterManager != null)
            {
                skyMaster.WaterManager.TerrainManager = skyMaster.TerrainManager;
            }
        }

        private void SetupCloudsRealistic()
        {
            skyMaster.SkyManager.HeavyStormVolumeClouds = HeavyStormVOLUME_CLOUD2 as GameObject;
            skyMaster.SkyManager.DayClearVolumeClouds = DayClearVOLUME_CLOUD2 as GameObject;
            skyMaster.SkyManager.SnowVolumeClouds = SnowVOLUME_CLOUD2 as GameObject;
            skyMaster.SkyManager.RainStormVolumeClouds = RainStormVOLUME_CLOUD2 as GameObject;
            skyMaster.SkyManager.SnowStormVolumeClouds = SnowStormVOLUME_CLOUD2 as GameObject;
            skyMaster.SkyManager.RainVolumeClouds = RainVOLUME_CLOUD2 as GameObject;
            skyMaster.SkyManager.PinkVolumeClouds = PinkVOLUME_CLOUD2 as GameObject;
            skyMaster.SkyManager.DustyStormVolumeClouds = DustyStormVOLUME_CLOUD2 as GameObject;
            skyMaster.SkyManager.LightningVolumeClouds = LightningVOLUME_CLOUD2 as GameObject;
        }

        public GameObject FindInChildren(GameObject gameObject, string name)
        {
            foreach (Transform transf in gameObject.GetComponentsInChildren<Transform>())
            {
                if (transf.name == name)
                {
                    Debug.Log(transf.name);
                    return transf.gameObject;
                }
            }
            return null;
        }

        public void SetupPlayer(Transform playerTransform)
        {
            skyMaster.SkyManager.Hero = playerTransform;
        }

        public void SetupCamera(Camera targetCamera)
        {
            if (true) return; //Todo: missing shader errors
            //fog
            targetCamera.gameObject.AddComponent<GlobalFogSkyMaster>();
            targetCamera.gameObject.GetComponent<GlobalFogSkyMaster>().SkyManager = skyMaster.SkyManager;
            targetCamera.gameObject.GetComponent<GlobalFogSkyMaster>().Sun = skyMaster.SkyManager.SUN_LIGHT.transform;
            skyMaster.TerrainManager.Lerp_gradient = true;
            skyMaster.TerrainManager.ImageEffectFog = true;
            skyMaster.TerrainManager.FogHeightByTerrain = true;

            //sun shafts
            targetCamera.gameObject.AddComponent<SunShaftsSkyMaster>();
            targetCamera.gameObject.GetComponent<SunShaftsSkyMaster>().sunTransform = skyMaster.SkyManager.SunObj.transform;
            skyMaster.TerrainManager.ImageEffectShafts = true;

            //bloom
            targetCamera.gameObject.AddComponent<BloomSkyMaster>();

            //aberrarion
            targetCamera.gameObject.AddComponent<VignetteAndChromaticAberrationSM>();

            //tone mapping
            targetCamera.gameObject.AddComponent<TonemappingSM>();
            targetCamera.gameObject.GetComponent<TonemappingSM>().exposureAdjustment = 2.2f;

            //underwater effect
            targetCamera.gameObject.AddComponent<UnderWaterImageEffect>();
            targetCamera.gameObject.GetComponent<UnderWaterImageEffect>().enabled = false;
        }
    }
}