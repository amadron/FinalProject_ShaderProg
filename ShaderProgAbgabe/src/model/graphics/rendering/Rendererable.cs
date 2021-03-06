﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace Example.src.model.graphics.rendering
{
    class Renderable
    {
        public enum ProjectionMode { Planar, Spherical };
        public int hasAlbedoTexture;
        public ITexture2D albedoTexture;
        public int hasNormalMap;
        public ITexture2D normalMap;
        public int hasHeightMap;
        public ITexture2D heightMap;
        public float heightScaleFactor;
        public int hasAlphaMap;
        public ITexture2D alphaMap;
        public ITexture2D environmentMap;
        public float reflectivity;
        public int hasEnvironmentMap;
        public FaceCullingMode faceCullingMode;
        public int instances;
        public float unlit;
        public ProjectionMode projectionMode;
        private VAO deferredMesh;
        private VAO lightViewMesh;
        private VAO shadowMapMesh;
        private IShaderProgram deferredShader;
        private IShaderProgram shadowMapShader;
        private IShaderProgram lightViewShader;

        public Renderable()
        {
            hasAlbedoTexture = 0;
            albedoTexture = null;
            hasNormalMap = 0;
            normalMap = null;
            hasHeightMap = 0;
            heightMap = null;
            heightScaleFactor = 1;
            hasAlphaMap = 0;
            alphaMap = null;
            hasEnvironmentMap = 0;
            environmentMap = null;
            reflectivity = 0;
            faceCullingMode = FaceCullingMode.BACK_SIDE;
            instances = 0;
            unlit = 0 ;
            projectionMode = ProjectionMode.Planar;
        }

        public void SetAlbedoTexture(ITexture2D texture)
        {
            albedoTexture = texture;
            hasAlbedoTexture = 1;
        }

        public void SetNormalMap(ITexture2D texture)
        {
            normalMap = texture;
            hasNormalMap = 1;
        }

        public void SetHeightMap(ITexture2D texture)
        {
            heightMap = texture;
            hasHeightMap = 1;
        }

        public void SetAlphaMap(ITexture2D texture)
        {
            alphaMap = texture;
            hasAlphaMap = 1;
        }

        public void SetEnvironmentMap(ITexture2D texture)
        {
            environmentMap = texture;
            hasEnvironmentMap = 1;
            reflectivity = 1;
        }

        public void SetDeferredGeometryMesh(VAO mesh, IShaderProgram shader)
        {
            deferredMesh = mesh;
            deferredShader = shader;
        }

        public VAO GetDeferredMesh()
        {
            return deferredMesh;
        }

        public void SetLightViewMesh(VAO mesh, IShaderProgram shader)
        {
            lightViewMesh = mesh;
            lightViewShader = shader;
        }

        public VAO GetLightViewMesh()
        {
            return lightViewMesh;
        }

        public void SetShadowMapMesh(VAO mesh, IShaderProgram shader)
        {
            shadowMapMesh = mesh;
            shadowMapShader = shader;
        }

        public VAO GetShadowMapMesh()
        {
            return shadowMapMesh;
        }

        public ref VAO GetMesh()
        {
            return ref deferredMesh;
        }

        public IShaderProgram GetShader()
        {
            return deferredShader;
        }

        public void SetInstancePositions(Vector3[] positions)
        {
            deferredMesh.SetAttribute(deferredShader.GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instancePosition"), positions, true);
            lightViewMesh.SetAttribute(lightViewShader.GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instancePosition"), positions, true);
            shadowMapMesh.SetAttribute(shadowMapShader.GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instancePosition"), positions, true);
        }
        
        public void SetInstanceRotations(Vector4[] rotations)
        {
            deferredMesh.SetAttribute(deferredShader.GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instanceRotation"), rotations, true);
            lightViewMesh.SetAttribute(lightViewShader.GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instanceRotation"), rotations, true);
            shadowMapMesh.SetAttribute(shadowMapShader.GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instanceRotation "), rotations, true);
        }

        public void SetInstanceScales(Vector3[] scales)
        {
            deferredMesh.SetAttribute(deferredShader.GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instanceScale"), scales, true);
            lightViewMesh.SetAttribute(lightViewShader.GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instanceScale"), scales, true);
            shadowMapMesh.SetAttribute(shadowMapShader.GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instanceScale"), scales, true);
        }

    }
}
