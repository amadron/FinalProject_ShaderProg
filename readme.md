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
* Shadow Maps
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
* Shadow Maps:
    * Implementation of Shadow Map filtering
    * Implementation of Soft Shadows
    * Changing light Camera from perspective to Orthogonal

