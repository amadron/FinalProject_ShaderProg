﻿using Example.src.model.entitys;
using Example.src.model.graphics.camera;
using Example.src.model.graphics.rendering;
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
        protected List<Renderable> geometryList;
        protected List<ParticleSystem> particleSystems;
        protected Vector4 ambientColor;
        protected Camera directionalLightCamera;

        public Scene()
        {
            pointLightList = new List<PointLight>();
            geometryList = new List<Renderable>();
            particleSystems = new List<ParticleSystem>();
        }

        public void Update(float deltatime)
        {
            for(int i = 0; i < particleSystems.Count; i++)
            {
                particleSystems[i].Update(deltatime);
            }
        }


        public List<PointLight> getPointLights()
        {
            return pointLightList;
        }

        public DirectionalLight getDirectionalLight()
        {
            return directionalLight;
        }

        public List<Renderable> getGeometry()
        {
            return geometryList;
        }

        public void AddPointLight(PointLight light)
        {
            pointLightList.Add(light);
        }

        public void AddGeometry(Renderable drawable)
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

        public void SetDirectionalLightCamera(Camera camera)
        {
            directionalLightCamera = camera;
        }

        public Camera GetDirectionalLightCamera()
        {
            return directionalLightCamera;
        }

        public void Resize(int width, int height)
        {
            if(directionalLightCamera != null)
                directionalLightCamera.Resize(width, height);
        }

        public void AddParticleSystem(ParticleSystem system)
        {
            particleSystems.Add(system);
        }

        public List<ParticleSystem> GetParticleSystems()
        {
            return particleSystems;
        }

    }
}
