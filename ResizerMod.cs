using System;
using System.Collections.Generic;
using UnityEngine;
using ICities;
using CitiesHarmony.API;

namespace Resizer
{
    public class ResizerMod : IUserMod
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

    } // class ResizerMod

    public static class Resizer
    {
        private static readonly List<BuildingInfo> _resized = new List<BuildingInfo>();

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
            Debug.Log($"Resizer: resizing prefab {prefab.name} by {scale.ToString("F2")}");

            // main mash and generated mesh only if not null
            //prefab.m_mesh.DebugDump();
            prefab.m_mesh?.Resize(scale);
            //prefab.m_mesh.DebugDump();
            prefab.m_generatedMesh?.Resize(scale);

            // shift props
            //prefab.m_props[0].m_position
            if (ResizerXml.Settings != null)
            {
                bool resizeAllProps = ResizerXml.Settings.CheckIfAllProps(prefab.name);
                foreach (BuildingInfo.Prop prop in prefab.m_props)
                    if (resizeAllProps ||
                        (prop.m_prop != null && ResizerXml.Settings.CheckPropName(prop.m_prop.name)) ||
                        (prop.m_finalProp != null && ResizerXml.Settings.CheckPropName(prop.m_finalProp.name)))
                    {
                        //Debug.Log($"Resizer: shifting prop {prop.m_prop?.name}");
                        prop.m_position = Vector3.Scale(prop.m_position, scale);
                    }
            }

            // no need to shift doors - they are calculated later
            //prefab.m_enterDoors[0].m_position
            //prefab.m_exitDoors[0].m_position

            // update lod meshes
            prefab.m_lodMesh?.Resize(scale);
            prefab.m_lodMeshCombined1?.Resize(scale);
            prefab.m_lodMeshCombined4?.Resize(scale);
            prefab.m_lodMeshCombined8?.Resize(scale);

            // 4. Shift props and other added items (?) note that some props are placed on the ground
            //prefab.m_size - calculated
            //prefab.m_renderSize - calculated
            //prefab.m_collisionHeight - calculated
            //prefab.m_generatedInfo.m_baseArea; - all calculated
            //prefab.m_generatedInfo.m_baseNormals;
            //prefab.m_generatedInfo.m_heights;

            // what about sub buildings: TODO in the next versions if needed
            //prefab.m_subBuildings[0].m_position
            //prefab.m_specialPlaces => props???
        }

    } // class Resizer

} // namespace
