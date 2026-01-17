using PicoGK;
using System;
using System.Numerics;

namespace PicoGKExamples
{
    public class UEARocket
    {
        public void Task()
        {
            Library.Log("Starting UEA Rocket generation...");
            
            Lattice latMetal = new Lattice();
            Lattice latVoid = new Lattice();

            // --- 20kN NOYRON GEOMETRY ---
            float fHeight = 350f;
            float fThroatZ = 180f;

            Library.Log("Building metal shell...");
            // Define the Copper Shell (Outer)
            latMetal.AddBeam(new Vector3(0,0,0), new Vector3(0,0,fThroatZ), 90, 45, true);
            latMetal.AddBeam(new Vector3(0,0,fThroatZ), new Vector3(0,0,fHeight), 45, 110, true);

            Library.Log("Building combustion core...");
            // Define the Combustion Core (Inner Void)
            // 30mm throat radius for high-pressure exhaust (Mach Diamonds)
            latVoid.AddBeam(new Vector3(0,0,-5), new Vector3(0,0,fThroatZ), 82, 30, true);
            latVoid.AddBeam(new Vector3(0,0,fThroatZ), new Vector3(0,0,fHeight+5), 30, 102, true);

            Library.Log("Adding regenerative cooling channels...");
            // --- REGEN COOLING CHANNELS ---
            // 30 channels snaking through the copper wall
            int iChannels = 30;
            for (int i = 0; i < iChannels; i++)
            {
                float fStartAngle = (float)(i * Math.PI * 2 / iChannels);
                for (float z = 10; z < fHeight - 10; z += 8)
                {
                    float fAngle = fStartAngle + (z * 0.04f);
                    // Radius interpolation to stay in the wall
                    float r = (z < fThroatZ) ? 86 - (z/fThroatZ)*48 : 38 + ((z-fThroatZ)/(fHeight-fThroatZ))*65;
                    
                    latVoid.AddSphere(new Vector3((float)Math.Cos(fAngle)*r, (float)Math.Sin(fAngle)*r, z), 2.5f);
                }
            }

            Library.Log("Converting to voxels...");
            // --- VOXEL COMPOSITION ---
            Voxels voxEngine = new Voxels(latMetal);
            voxEngine.BoolSubtract(new Voxels(latVoid));

            Library.Log("Applying slicer...");
            // --- SLICER (SEE THE COPPER INTERNALS) ---
            Lattice latSlicer = new Lattice();
            latSlicer.AddBeam(new Vector3(-500, 250, -500), new Vector3(500, 250, 500), 500, 500, true);
            voxEngine.BoolIntersect(new Voxels(latSlicer));

            Library.Log("Converting to mesh...");
            // --- RENDER ---
            // Save to file instead of viewer to avoid loading lights hang
            Mesh mshEngine = new Mesh(voxEngine);
            
            Library.Log("Saving to STL file...");
            mshEngine.SaveToStlFile("/Users/raymartin/picogk/uea_rocket_engine.stl");
            Library.Log("Successfully saved rocket engine to uea_rocket_engine.stl");
            
            // Optionally still add to viewer
            // Library.oViewer().Add(voxEngine);
        }
    }
}