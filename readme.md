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
* Deferred Lighting
    * Point light Datatype, which stores Settings.
    * Point lights will be rendered as Spheres and shaded as Seperate pass.
    * Deferred Pass processes Directional Light and adds Point Light Data.

### To do

Known Issues:

* Specular lights disappear and appear when moving the Camera.
