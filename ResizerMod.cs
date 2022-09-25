using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ICities;
using CitiesHarmony.API;

namespace Resizer
{
    public class ResizerMod : IUserMod, ILoadingExtension
    {
        public string Name => "Resizer";
        public string Description => "Changes the size of building prefabs";

        //private bool _resized = false;

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => ResizerPatcher.PatchAll());
            //Debug.Log("Infixo is asking where this message appears");
            ResizerXml.LoadSettings();
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) ResizerPatcher.UnpatchAll();
        }

        // called when level loading begins
        public void OnCreated(ILoading loading)
        {
            //throw new System.NotImplementedException();
        }

        // called when level is loaded
        public void OnLevelLoaded(LoadMode mode)
        {
            /*
            //Debug.Log("Resizer: OnLevelLoaded - called when level is loaded");
            if (_resized)
                Debug.Log("Resizer: building prefabs already resized");
            else
            {
                Debug.Log("Resizer: resizing prefabs...");
                //Resizer.ResizeBuildingPrefabs();
                _resized = true;
                Debug.Log("Resizer: resizing prefabs OK");
            }
            */
       }
        
        // called when unloading begins
        public void OnLevelUnloading()
        {
            //throw new System.NotImplementedException();
        }

        // called when unloading finished
        public void OnReleased()
        {
            //throw new System.NotImplementedException();
        }

    } // class ResizerMod

    public static class Resizer
    {
        private static readonly List<BuildingInfo> _resized = new List<BuildingInfo>();
        /*
        public static string[] prefabsToResize = new string[] { "University", "Police Headquarters", "Fire Station" };
        public static Vector3 scaleValue = new Vector3(0.5f, 0.5f, 0.5f);
        public static string[] propsToShift = new string[] {
            "door marker", // 3 props
            "billboard", // 140+ props => there are some that are standalone!
            "wall", // 30+ props
            "logo", // 60+ props
            "roof", // roofad, Rooftop access, Rooftop window, Slanted rooftop window, Roof Vegetation, Roof Walkway
            "flood light", // 25+ props
            "ac box", "rotating ac",
            "light pole", // 2 props
            "solar panel", // 5 props
            "antenna", // Microwave antenna, Wifi antenna
            "neon", // 8 props
            "organic shop 3d sign", // 4 props
            "air source heat pump", // 2 props
            "ventilation pipe", // 2 props
            "invisible helipad marker", // not sure?
            "radio mast"
        };
        */
        /*
        public static void ResizeBuildingPrefabs()
        {
            // total number of loaded building assets
            int buildingPrefabCount = PrefabCollection<BuildingInfo>.LoadedCount();
            Debug.Log($"Resizer: number of loaded building prefabs is {buildingPrefabCount}");
            // iterate through the prefabs
            for (uint i = 0; i < buildingPrefabCount; i++)
            {
                // get the building asset with the given index from the collection
                BuildingInfo prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);
                //Debug.Log($"Resizer: prefab no {i} {prefab.name}");
                if (prefabsToResize.Contains(prefab.name))
                    prefab.ProcessBuildingPrefab(scaleValue);
            }
        }
        */
        public static void DebugDump(this Mesh mesh)
        {
            if (mesh == null)
            {
                Debug.Log("Resizer: mesh is null");
                return;
            }
            Debug.Log($"Resizer: **** mesh info for [{mesh.name}] ****");
            Debug.Log($"isReadable: {mesh.isReadable}");
            Debug.Log($"bounds:     {mesh.bounds}");
            Debug.Log($"bounds min: {mesh.bounds.min}");
            Debug.Log($"bounds max: {mesh.bounds.max}");
            Debug.Log($"bounds size:{mesh.bounds.size}");
            Debug.Log($"#vertices:  {mesh.vertexCount}");
            Debug.Log($"#subMeshes: {mesh.subMeshCount}");
            for (int i = 0; i < mesh.subMeshCount; i++)
                Debug.Log($"topology {i}: {mesh.GetTopology(i)}");
            Debug.Log($"vertices:   {mesh.vertices.Length}");
            Debug.Log($"normals:    {mesh.normals.Length}");
            Debug.Log($"tangents:   {mesh.tangents.Length}");
            Debug.Log($"triangles:  {mesh.triangles.Length}");
            Debug.Log($"uv0:        {mesh.uv.Length}");
            Debug.Log($"uv1:        {mesh.uv2.Length}");
            Debug.Log($"uv2:        {mesh.uv3.Length}");
            Debug.Log($"uv3:        {mesh.uv4.Length}");
            Debug.Log($"colors:     {mesh.colors.Length}");
            // find min and max coordinates from vertices
            Vector3 minMesh = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxMesh = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            List<Vector3> vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            foreach (Vector3 item in vertices)
            {
                minMesh.x = Math.Min(minMesh.x, item.x);
                maxMesh.x = Math.Max(maxMesh.x, item.x);
                minMesh.y = Math.Min(minMesh.y, item.y);
                maxMesh.y = Math.Max(maxMesh.y, item.y);
                minMesh.z = Math.Min(minMesh.z, item.z);
                maxMesh.z = Math.Max(maxMesh.z, item.z);
            }
            Debug.Log($"mesh min:  {minMesh} ");
            Debug.Log($"mesh max:  {maxMesh} ");
            Debug.Log($"mesh size: {maxMesh-minMesh} ");
        }

        public static void Resize(this Mesh mesh, Vector3 scale)
        {
            //Debug.Log($"Resizer: resizing mesh {mesh.name} by {scale}");
            //1.Calculate new positions or vertices.
            List<Vector3> oldVertices = new List<Vector3>();
            List<Vector3> newVertices = new List<Vector3>();
            mesh.GetVertices(oldVertices);
            foreach (Vector3 vertex in oldVertices)
            {
                vertex.Scale(scale);
                newVertices.Add(vertex);
            }
            mesh.SetVertices(newVertices);
            //2.Recalculate bounds, normals and tangents
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        public static void ProcessBuildingPrefab(this BuildingInfo prefab)
        {
            //Debug.Log($"Resizer: prefab {prefab.name}");
            if (ResizerXml.Settings != null && ResizerXml.Settings.CheckPrefabName(prefab.name) && !_resized.Contains(prefab))
            {
                prefab.ProcessBuildingPrefab(ResizerXml.Settings.GetScale(prefab.name));
                _resized.Add(prefab);
            }
        }

        public static void ProcessBuildingPrefab(this BuildingInfo prefab, Vector3 scale)
        {
            Debug.Log($"Resizer: resizing prefab {prefab.name} by {scale}");

            // main mash and generated mesh only if not null
            //prefab.m_mesh.DebugDump();
            prefab.m_mesh?.Resize(scale);
            //prefab.m_mesh.DebugDump();
            prefab.m_generatedMesh?.Resize(scale);

            // shift props
            //prefab.m_props[0].m_position

            bool CheckPropName(string propName)
            {
                //foreach (string propPart in propsToShift)
                //if (propName.Contains(propPart))
                //return true;
                //return false;
                return ResizerXml.Settings != null && ResizerXml.Settings.CheckPropName(propName);
            }

            foreach (BuildingInfo.Prop prop in prefab.m_props)
                if ( (prop.m_prop != null && CheckPropName(prop.m_prop.name)) || (prop.m_finalProp != null && CheckPropName(prop.m_finalProp.name)) )
                {
                    //Debug.Log($"Resizer: shifting prop {prop.m_prop?.name}");
                    prop.m_position = Vector3.Scale(prop.m_position, scale);
                }

            // shift doors
            //prefab.m_enterDoors[0].m_position
            //prefab.m_exitDoors[0].m_position
            /*
            if (prefab.m_enterDoors != null)
                foreach (BuildingInfo.Prop prop in prefab.m_enterDoors)
                {
                    //Debug.Log($"Resizer: shifting entry door {prop.m_prop?.name}");
                    prop.m_position = Vector3.Scale(prop.m_position, scale);
                }
            if (prefab.m_exitDoors != null)
                foreach (BuildingInfo.Prop prop in prefab.m_exitDoors)
                {
                    //Debug.Log($"Resizer: shifting exit doors {prop.m_prop?.name}");
                    prop.m_position = Vector3.Scale(prop.m_position, scale);
                }
            */

            // update lod meshes
            // LOD mesh
            //Debug.Log($"Resizer: resizing lod meshes");
            //prefab.m_lodMesh.DebugDump();
            prefab.m_lodMesh?.Resize(scale);
            //prefab.m_lodMesh.DebugDump();
            // LOD mesh 1
            //prefab.m_lodMeshCombined1.DebugDump();
            prefab.m_lodMeshCombined1?.Resize(scale);
            //prefab.m_lodMeshCombined1.DebugDump();
            // LOD mesh 4
            //prefab.m_lodMeshCombined4.DebugDump();
            prefab.m_lodMeshCombined4?.Resize(scale);
            //prefab.m_lodMeshCombined4.DebugDump();
            // LOD mesh 8
            //prefab.m_lodMeshCombined8.DebugDump();
            prefab.m_lodMeshCombined8?.Resize(scale);
            //prefab.m_lodMeshCombined8.DebugDump();


            // 4. Shift props and other added items (?) note that some props are placed on the ground
            //prefab.m_size
            //prefab.m_renderSize;
            //prefab.m_collisionHeight
            //prefab.m_generatedInfo.m_baseArea;
            //prefab.m_generatedInfo.m_baseNormals;
            //prefab.m_generatedInfo.m_heights;

            // what about sub buildings: TODO in the next versions if needed
            //prefab.m_subBuildings[0].m_position
            //prefab.m_specialPlaces => props???
        }

    } // class Resizer

} // namespace
