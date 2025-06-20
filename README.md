# URP Head Interaction Demo (Unity 2022.3.56f1)

This repository is structured with three main branches:

- `main`: baseline branch
- `urp-unity-2022.3.56f1`: initial project setup using Unity **2022.3.56f1** with **URP**
- `head-beating`: branch where the interactive demo was developed

## Overview

The current state of the project allows building and running a demo scene featuring a stylized head model created in **Blender**.

## Interaction

- **Right Mouse Button (RMB)**:
  - Hold RMB and hover over the head to trigger a **collision**.
  - The collision originates at the impact point and displaces mesh vertices in the direction of mouse movement.
  - The **displacement amount is influenced by mouse speed**.
  - Visual and audio feedback (shaders, sounds, etc.) enhance immersion.
  - To trigger a new hit, you must **release and press RMB again**.

- **Alternative Hit**:
  - Holding RMB while stationary over the head will still cause a hit,
    but with **minimum displacement** (no mouse movement).

- **Left Mouse Button (LMB)**:
  - Rotate the camera **orbiting around the head**.
  - Vertical rotation is clamped to **±30 degrees**.

- **Mouse Scroll Wheel**:
  - Zoom in/out within predefined distance limits.

## Notes

- The project uses **custom shaders** and **vertex manipulation** for real-time mesh deformation.

## Special thanks:
To [@trashpandaboy](https://github.com/trashpandaboy), nice repos. I used the EventDispatcher and turned out to be really handy.
