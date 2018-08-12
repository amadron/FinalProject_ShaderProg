﻿using Example.src.model.entitys;
using Example.src.model.lightning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zenseless.Geometry;
using Zenseless.HLGL;

namespace Example.src.model
{
    class Scene
    {
        protected List<PointLight> pointLightList;
        protected DirectionalLight directionalLight;
        protected List<IDrawable> geometryList;
        protected Terrain terrain;
        protected Vector4 ambientColor;
        protected Camera<Orbit, Perspective> directionalLightCamera;

        public Scene()
        {
            pointLightList = new List<PointLight>();
            geometryList = new List<IDrawable>();
        }

        public PointLight[] getPointLights()
        {
            return pointLightList.ToArray();
        }

        public Terrain getTerrain()
        {
            return terrain;
        }

        public DirectionalLight getDirectionalLight()
        {
            return directionalLight;
        }

        public IDrawable[] getGeometry()
        {
            return geometryList.ToArray();
        }

        public void AddPointLight(PointLight light)
        {
            pointLightList.Add(light);
        }

        public void AddGeometry(IDrawable drawable)
        {
            geometryList.Add(drawable);
        }

        public void SetAmbientColor(Vector4 color)
        {
            ambientColor = color;
        }

        public Vector4 GetAmbientColor()
        {
            return ambientColor;
        }

        public void SetDirectionalLightCamera(Camera<Orbit, Perspective> camera)
        {
            directionalLightCamera = camera;
        }

        public Camera<Orbit, Perspective> GetDirectionalLightCamera()
        {
            return directionalLightCamera;
        }

    }
}