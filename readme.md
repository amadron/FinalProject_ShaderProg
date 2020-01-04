# Submission for Shader Programming Master course

Based on [Zenseless Framework](https://github.com/danielscherzer/Zenseless) by Daniel Scherzer.

Project Contains two Scenes:
- Island Scene: Implemented Deferred Rendering and Shadow Maps
- PBR: Implementation of Physically based Rendering

# Island Scene

## Controls

Current first person Controls are:

* Camera Movement:
    * Left Shift - Faster Camera Movement Speed ("Running mode")
    * W - Forward in Camera Direction
    * A - Left Strafe
    * D - Right Strafe
    * S - Backward in from Camera Direction
    * Q - Vertical Down
    * E - Vertical Up
* Camera Rotation:
    * Mousclick and Drag to Rotate the camera
    * Left/Right Arrows - Rotate Horizontal Direction
    * Up/Down Arrows - Rotate Vertical Direction

* Draw Passes
    * F1 - Print camera Position and Rotation
    * F4 - Deferred Pass (All assembled)
    * F5 - Position Pass
    * F6 - Color/Albedo Pass
    * F7 - Normal Pass
    * F8 - Shadow Map Pass
    * F9 - Directional Pass
    * F10 - Point Light Pass


## Implementations

The following features/techniques are implemented:

* Main
    * Resizing of Window is supported
* Phong Shading
* Unlit Shading
* Textures
    * Albedo/Diffuse
    * Normal/Bump Maps
        * Normals will be replaced by normals from map
    * Height Map
        * Vertexposition will be offset by Normal direction
    * Alpha Map
        * Alpha map are involved in Shadow Map creation and Deferred lightning.
* Summed Area Table (SAT) Filter
    * Implementation by Fragment Shader
    * Each Pass is Parallelized by using Blocks
    * Currently working in 5 passes
        * First Pass Vertical: Sum vertical previous Pixel within this Block
        * Second Pass Vertical: Add the Maximum Vertical Values of the Previous Blocks
        * Third Pass Horizontal: Sum in each Block, for each Pixel the previous Pixels within this Block
        * Fourth Pass Horizontal: Add for each Block the maximum Values of the previous Blocks. 
        * Fifth/Final Pass: Get the Average value for an Range around each Pixel.
            * (LowerRightCorner - UpRightCorner - LowerLeftCorner + UpLeftCorner) / (RangeWidth * RangeHeight)
* Shadow Maps
    * SAT Filtered Shadowmap
    * Exponential Shadowmaps implemented
    * One Pass renders from Light view
        * Light view will be Rendered using orthographic camera
        * Calculates the exponential of k * lightDistance
    * Another Pass validates Distance from Light view and Camera View. And renders into shadow Pass.
        * Calculates the exponential of -k * viewDistance and multiplies with the data from Light View Pass
* Deferred Lighting
    * Point light Datatype, which stores Settings.
    * Point lights will be rendered as Spheres and shaded as Seperate pass.
    * Deferred Pass processes Directional Light and adds Point Light Data.
    * Shadow pass will be also composed.
    
* Particles
    * Particles can be configured by Range parameters
    * Particles are rendered using Instancing
    * Modules - Are Function used to Manipulating Spawned Particles. For  Example: Changing Scale over lifetime, Changin Color.
        * Two Types are Supported
            * Global Modules - One Module for all Particles
            * Per Particle Modul - Each Particle can have its own Module.

* Water Simulation
    * Creates Depthmap using Gerster Waves

* UI
    * Z Value of Element sets the order, of Rendering/ Layer
### To do

* Known Issues
    * Issues with shadow calculation of particles
    * Maybe Point Light Falloff Rework
    * Merge ShadowMap pass into Geometry Pass
    * Recalculate normals on Heightmap

# Physically Based Rendering (PBR)

Implementation of the PBR tutorial from [learnopengl.com](https://learnopengl.com/)


### Controls

Mouse Left + Mouse movement - Rotate Camera

* A - Move camera left
* D - Move camera right
* S - Move camera back
* W - Move camera forward
* Q - Move camera up
* E - Move camera down

The implementation consists of three parts.
### Lighting
This is the basic lighting, which samples the radiance of multiple lights over a hemisphere around a point on the surface.

This takes only the lighting of the lights into account. To get more satisfying results, the environment must be taken into account. This is realised by using an hdr texture for image based lighting (IBL).

### Diffuse Image based lighting
At this stage, the irradiance of the hdr texture will be sampled over a hemisphere and stored into a texture.

### Specular Image based lighting
For the specular part of the environment, the rendering equation will be split into two parts.

#### Prefiltered HDR environment map
At this part, the different roughness levels of the environment will be preprocessed.
To realize it, a cubemap with several mipmap levels will be created and each mipmap level stores a convoluted version of the environment map. 
The convolution is realized using monte carlo, sampling around a hemisphere around a point.

#### Integrated BRDF
The second part stores a Look up Texture (LUT) which stores the the results for the specular BRDF function.
The horizontal part stores the angle between normal Vector and the View direction (dot(normal, w)) from 0.0 to 1.0. 
And Vertical are the roughness inputs are 0.0 to 1.0.