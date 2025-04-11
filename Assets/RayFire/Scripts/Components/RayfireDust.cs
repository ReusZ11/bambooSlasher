using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [SelectionBase]
    [AddComponentMenu ("RayFire/Rayfire Dust")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-dust-component/")]
    public class RayfireDust : MonoBehaviour
    {
        [Header("  Emit Dust")]
        [Space (3)]
                
        public bool onDemolition = false;
        [Space (1)]
        public bool onActivation = false;
        [Space (1)]
        public bool onImpact = false;

        [Header("  Main")]
        [Space (3)]

        [Range(0.01f, 1f)] public float opacity;
        [Space (2)]
        public Material dustMaterial;
        [Space (2)]
        public Material emissionMaterial;
        
        [Header("  Properties")]
        [Space (3)]

        public RFParticleEmission emission;
        [Space (2)]
        public RFParticleDynamicDust dynamic;
        [Space (2)]
        public RFParticleNoise noise;
        [Space (2)]
        public RFParticleCollisionDust collision;
        [Space (2)]
        public RFParticleLimitations limitations;
        [Space (2)]
        public RFParticleRendering rendering;
        
        // Hidden
        [HideInInspector] public RayfireRigid rigid;
        [HideInInspector] public ParticleSystem pSystem = null;
        [HideInInspector] public Transform hostTm   = null;
        [HideInInspector] public bool initialized;
        
        [HideInInspector] public List<RayfireDust> children;
        [HideInInspector] public int amountFinal;
        
        // auto alpha fade
        // few dust textures with separate alphas
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RayfireDust()
        {
            onDemolition = true;
            onActivation = false;
            onImpact = false;  
            
            dustMaterial = null;
            opacity = 0.25f;
            emissionMaterial = null;
            
            emission = new RFParticleEmission();
            dynamic = new RFParticleDynamicDust();
            noise = new RFParticleNoise();
            collision = new RFParticleCollisionDust();
            limitations = new RFParticleLimitations();
            rendering = new RFParticleRendering();
            
            // Hidden
            pSystem = null;
            hostTm = null;
            initialized = false;
            amountFinal = 5;
        }

        // Copy from
        public void CopyFrom(RayfireDust source)
        {
            onDemolition = source.onDemolition;
            onActivation = source.onActivation;
            onImpact = source.onImpact;
            
            dustMaterial = source.dustMaterial;
            opacity = source.opacity;
            emissionMaterial = source.emissionMaterial;
            
            emission.CopyFrom (source.emission);
            dynamic.CopyFrom (source.dynamic);
            noise.CopyFrom (source.noise);
            collision.CopyFrom (source.collision);
            limitations.CopyFrom (source.limitations);
            rendering.CopyFrom (source.rendering);
            
            initialized = source.initialized;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// ///////////////////////////////////////////////////////// 

        // Initialize
        public void Initialize()
        {
            // TODO AmountCheck(RayfireRigid scrSource, int pType) and xollect if ok

            // No material
            if (dustMaterial == null)
            {
                Debug.Log (gameObject.name + ": Dust material not defined.", gameObject);
                initialized = false;
                return;
            }
            
            initialized = true;
        }
        
        // Emit particles
        public void Emit()
        {
            // Initialize
            Initialize();
            
            // Emitter is not ready
            if (initialized == false)
                return;
            
            // Particle system
            ParticleSystem ps = CreateParticleSystem(this);

            // Get components
            MeshFilter emitMeshFilter = GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            // Get emit material index
            int emitMatIndex = RFParticles.GetEmissionMatIndex (meshRenderer, emissionMaterial);
            
            // TODO set amount
            this.amountFinal = 30;
            
            // Create debris
            CreateDust(transform, this, emitMeshFilter, emitMatIndex, ps);
        }
        
        // Create single dust particle system
        public void CreateDustImpact(RayfireRigid target, Vector3 impactPos, Vector3 impactNormal, short amount)
        {
            for (int i = 0; i < target.dustList.Count; i++)
            {
                // Particle system
                ParticleSystem ps = CreateParticleSystem (target.dustList[i]);

                // Align over impact
                target.dustList[i].hostTm.position = impactPos;
                target.dustList[i].hostTm.LookAt (impactPos + impactNormal);

                // Create debris
                CreateDust (target.transform, target.dustList[i], null, -1, ps);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Create common
        /// /////////////////////////////////////////////////////////
        
        // Create host and particle system
        public ParticleSystem CreateParticleSystem(RayfireDust scr)
        {
            // Create root
            GameObject host = new GameObject(scr.name + "_dust");
            host.transform.position = scr.transform.position;
            host.transform.rotation = scr.transform.rotation;
            host.transform.parent = scr.transform;
            host.transform.localScale = Vector3.one;

            // Particle system
            ParticleSystem ps = host.AddComponent<ParticleSystem>();
            scr.pSystem = ps;
            scr.hostTm = host.transform;

            // Destroy after all particles death
            Destroy(host, scr.emission.lifeMax + ps.main.duration);

            // Stop for further properties set
            ps.Stop();
            
            return ps;
        }
        
        // Create single dust particle system
        public void CreateDust(Transform host, RayfireDust scr, MeshFilter emitMeshFilter, int emitMatIndex, ParticleSystem ps)
        {
            // Set main module
            RFParticles.SetMain(ps.main, scr.emission.lifeMin, scr.emission.lifeMax, scr.emission.sizeMin, scr.emission.sizeMax, 
                scr.dynamic.gravityMin, scr.dynamic.gravityMax, scr.dynamic.speedMin, scr.dynamic.speedMax, 
                0f, scr.limitations.maxParticles, scr.emission.duration);

            // Emission over distance
            RFParticles.SetEmission(ps.emission, scr.emission.distanceRate, (short)scr.amountFinal);
            
            // Emission from mesh or from impact point
            if (emitMeshFilter != null)
                RFParticles.SetShapeMesh(ps.shape, emitMeshFilter.sharedMesh, emitMatIndex, emitMeshFilter.transform.localScale);
            else
                RFParticles.SetShapeObject(ps.shape);
            
            // Collision
            RFParticles.SetCollisionDust(ps.collision,  scr.collision);

            // Color over life time
            RFParticles.SetColorOverLife(ps.colorOverLifetime, scr.opacity);

            // Noise
            RFParticles.SetNoise(ps.noise, scr.noise);

            // Renderer
            SetParticleRendererDust(ps.GetComponent<ParticleSystemRenderer>(), scr.dustMaterial, scr.rendering.castShadows, scr.rendering.receiveShadows);
            
            // Start playing
            ps.Play();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Renderer
        /// /////////////////////////////////////////////////////////
        
        // Set renderer
        public void SetParticleRendererDust(ParticleSystemRenderer rend, Material material, bool cast, bool receive)
        {
            // Common vars
            rend.renderMode = ParticleSystemRenderMode.Billboard;
            rend.alignment = ParticleSystemRenderSpace.World;
            rend.normalDirection = 1f;

            // Set material. Original or inner
           rend.sharedMaterial = material;

            // Shadow casting
            rend.shadowCastingMode = cast == true 
                ? UnityEngine.Rendering.ShadowCastingMode.On 
                : UnityEngine.Rendering.ShadowCastingMode.Off;

            // Shadow receiving
            rend.receiveShadows = receive;

            // Dust vars
            rend.sortMode = ParticleSystemSortMode.OldestInFront;
            rend.minParticleSize = 0.0001f;
            rend.maxParticleSize = 999999f;
            rend.alignment = ParticleSystemRenderSpace.Facing;



            // Set Roll in 2018.3 and older builds TODO
            //           if (Application.unityVersion == "2018.3.0f2")
            //               renderer.shadowBias = 0.55f;
            //               renderer.allowRoll = false;
        }
        
        // Set material
        void SetMaterialDust(ParticleSystemRenderer rend, List<Material> mats)
        {
            // No material
            if (mats.Count == 0)
            {
                Debug.Log("Define dust material");
                return;
            }

            // Set material
            if (mats.Count == 1)
                rend.sharedMaterial = mats[0];
            else
                rend.sharedMaterial = mats[Random.Range(0, mats.Count - 1)];
        }
        
        public bool HasChildren { get { return children != null && children.Count > 0; } }
    }
}