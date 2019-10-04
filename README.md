# HDRP Graphics Stress Test
![HDRP Stress test](https://i.imgur.com/JLpMwqL.png)  
## How to use  
- Open Base scene and click play  
- Before building the player: execute **Edit->Visual Effects->Rebuild All Visual Effect Graphs**  
- Scene switching - **Space Bar** or click on **Prev/Next**  
- It starts sampling frames after frame **1000**, wait till frame **2000** to get the average FPS  
## Legend
- AllocatedMemoryForGraphicsDriver: The amount of allocated memory for the graphics driver, in bytes  
- TotalAllocatedMemory: The total memory allocated by the internal allocators in Unity. Unity reserves large pools of memory from the system  
- TempAllocatorSize: The size of the temp allocator  
- TotalReservedMemoryg: The total memory Unity has reserved  
- TotalUnusedReservedMemory: The amount of unused memory in pools where Unity allocates memory for usage when unity needs to allocate memory  
## Scenes
- CPUParticles
- CPUPhysics
- GPUParticles
- Lights
- Particles_Collision
- Rendering_OpaqueLit
- Rendering_TransparentLit


