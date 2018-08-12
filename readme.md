# Submission for Shader Programming Master course

Based on [Zenseless Framework](https://github.com/danielscherzer/Zenseless) by Daniel Scherzer. 

## Controls

Current first person Controls are:

* Camera Movement:
    * W - Forward in Camera Direction
    * A - Left Strafe
    * D - Right Strafe
    * S - Backward in from Camera Direction
    * Q - Vertical Down
    * E - Vertical Up
* Camera Rotation:
    * Left/Right Arrows - Rotate Horizontal Direction
    * Up/Down Arrows - Rotate Vertical Direction

## Implementations

The following techniques are implemented:

* Phong Shading
* Textures
    * Albedo/Diffuse
    * Normal/Bump Maps
        *Normals will be replaced by normals from map
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
        * Calculates the exponential of k * lightDistance
    * Another Pass validates Distance from Light view and Camera View. And renders into shadow Pass.
        * Calculates the exponential of -k * viewDistance and multiplies with the data from Light View Pass
* Deferred Lighting
    * Point light Datatype, which stores Settings.
    * Point lights will be rendered as Spheres and shaded as Seperate pass.
    * Deferred Pass processes Directional Light and adds Point Light Data.
    * Shadow pass will be also composed.
    

### To do

Known Issues:
* Currently only One VAO can be rendered. Working on multiple VAO Rendering Support
* Shadow Maps:
    * Refine Shadowmap filtering
    * Implementation of Soft Shadows
    * Changing light Camera from perspective to Orthogonal

