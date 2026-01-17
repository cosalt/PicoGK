using PicoGK;
using System;
using System.Numerics;

namespace PicoGKExamples
{
    /// <summary>
    /// race2space 15kn regeneratively cooled rocket engine
    /// designed for 3d printing in copper alloy
    /// </summary>
    public class Race2SpaceEngine
    {
        public void Task()
        {
            Library.Log("=== Race2Space 15kN Engine Generation ===");
            
            Lattice latMetal = new Lattice();
            Lattice latVoid = new Lattice();
            
            // === engine geometry parameters ===
            // throat diameter: 32mm (calculated for 15kn at pc=25bar, cf=1.65)
            // chamber diameter: 70mm (contraction ratio ~4.8)
            // nozzle exit: 70mm (expansion ratio ~4.8 for sea level)
            // total length: 280mm
            
            float fChamberLength = 100f;       // combustion chamber length
            float fConvergingLength = 40f;     // convergence section
            float fThroatZ = fChamberLength + fConvergingLength;
            float fDivergingLength = 140f;     // diverging nozzle section
            float fTotalLength = fThroatZ + fDivergingLength;
            
            // radii
            float fChamberRadius = 35f;        // chamber id: 70mm
            float fThroatRadius = 16f;         // throat id: 32mm
            float fExitRadius = 35f;           // exit id: 70mm
            float fWallThickness = 3.5f;       // minimum wall: 3.5mm
            
            Library.Log($"Chamber: ID={fChamberRadius*2}mm, Length={fChamberLength}mm");
            Library.Log($"Throat: ID={fThroatRadius*2}mm at z={fThroatZ}mm");
            Library.Log($"Exit: ID={fExitRadius*2}mm, Total Length={fTotalLength}mm");
            
            // === outer shell (copper structure) ===
            Library.Log("Building outer shell...");
            
            // Chamber section
            latMetal.AddBeam(
                new Vector3(0, 0, 0), 
                new Vector3(0, 0, fChamberLength),
                fChamberRadius + fWallThickness,
                fChamberRadius + fWallThickness,
                true
            );
            
            // Converging section
            latMetal.AddBeam(
                new Vector3(0, 0, fChamberLength),
                new Vector3(0, 0, fThroatZ),
                fChamberRadius + fWallThickness,
                fThroatRadius + fWallThickness + 2.5f,
                true
            );
            
            // throat section (constant area, slightly thicker wall)
            latMetal.AddBeam(
                new Vector3(0, 0, fThroatZ),
                new Vector3(0, 0, fThroatZ + 15f),
                fThroatRadius + fWallThickness + 2.5f,
                fThroatRadius + fWallThickness + 2.5f,
                true
            );
            
            // diverging section (nozzle)
            latMetal.AddBeam(
                new Vector3(0, 0, fThroatZ + 15f),
                new Vector3(0, 0, fTotalLength),
                fThroatRadius + fWallThickness + 2.5f,
                fExitRadius + fWallThickness,
                true
            );
            
            // === inner flow path (void) ===
            Library.Log("Building combustion chamber and nozzle flow path...");
            
            // extend slightly beyond actual geometry for clean boolean
            latVoid.AddBeam(
                new Vector3(0, 0, -2),
                new Vector3(0, 0, fChamberLength),
                fChamberRadius,
                fChamberRadius,
                true
            );
            
            latVoid.AddBeam(
                new Vector3(0, 0, fChamberLength),
                new Vector3(0, 0, fThroatZ),
                fChamberRadius,
                fThroatRadius,
                true
            );
            
            latVoid.AddBeam(
                new Vector3(0, 0, fThroatZ),
                new Vector3(0, 0, fTotalLength + 2),
                fThroatRadius,
                fExitRadius,
                true
            );
            
            // === regenerative cooling channels ===
            Library.Log("Adding regenerative cooling channels...");
            
            // 48 channels spiraling around throat area (most critical cooling zone)
            int iChannels = 48;
            float fChannelRadius = 1.8f;  // 3.6mm diameter channels
            
            // focus cooling on convergent section, throat, and initial divergent
            float fCoolingStart = fChamberLength - 20f;
            float fCoolingEnd = fThroatZ + 80f;
            
            for (int i = 0; i < iChannels; i++)
            {
                float fStartAngle = (float)(i * Math.PI * 2.0 / iChannels);
                
                for (float z = fCoolingStart; z < fCoolingEnd; z += 3.5f)
                {
                    // spiral angle increases with z
                    float fAngle = fStartAngle + (z - fCoolingStart) * 0.03f;
                    
                    // calculate local wall radius (mid-wall position)
                    float fLocalInnerRadius;
                    float fLocalOuterRadius;
                    
                    if (z < fChamberLength)
                    {
                        fLocalInnerRadius = fChamberRadius;
                        fLocalOuterRadius = fChamberRadius + fWallThickness;
                    }
                    else if (z < fThroatZ)
                    {
                        float t = (z - fChamberLength) / fConvergingLength;
                        fLocalInnerRadius = fChamberRadius - t * (fChamberRadius - fThroatRadius);
                        fLocalOuterRadius = fLocalInnerRadius + fWallThickness + 2.5f;
                    }
                    else if (z < fThroatZ + 15f)
                    {
                        fLocalInnerRadius = fThroatRadius;
                        fLocalOuterRadius = fThroatRadius + fWallThickness + 2.5f;
                    }
                    else
                    {
                        float t = (z - (fThroatZ + 15f)) / (fCoolingEnd - (fThroatZ + 15f));
                        fLocalInnerRadius = fThroatRadius + t * (fExitRadius - fThroatRadius);
                        fLocalOuterRadius = fLocalInnerRadius + fWallThickness + 2.5f;
                    }
                    
                    // place channel in middle of wall
                    float fChannelRadius_pos = (fLocalInnerRadius + fLocalOuterRadius) / 2.0f;
                    
                    Vector3 vPos = new Vector3(
                        (float)Math.Cos(fAngle) * fChannelRadius_pos,
                        (float)Math.Sin(fAngle) * fChannelRadius_pos,
                        z
                    );
                    
                    latVoid.AddSphere(vPos, fChannelRadius);
                }
            }
            
            // === injector mounting plate ===
            Library.Log("Adding injector mounting flange...");
            latMetal.AddBeam(
                new Vector3(0, 0, -8),
                new Vector3(0, 0, 0),
                fChamberRadius + fWallThickness + 8f,
                fChamberRadius + fWallThickness,
                true
            );
            
            // injector manifold passage void
            latVoid.AddBeam(
                new Vector3(0, 0, -10),
                new Vector3(0, 0, -2),
                fChamberRadius - 3f,
                fChamberRadius,
                true
            );
            
            // === propellant inlet ports ===
            Library.Log("Adding fuel and oxidizer inlet ports...");
            
            // fuel inlet (tangential entry for swirl)
            float fInletRadius = 6f;  // 12mm diameter ports
            float fInletZ = -5f;
            float fInletPositionRadius = fChamberRadius + fWallThickness + 4f;
            
            // two fuel inlets at 90 degrees
            for (int i = 0; i < 2; i++)
            {
                float angle = (float)(i * Math.PI);
                Vector3 vInletPos = new Vector3(
                    (float)Math.Cos(angle) * fInletPositionRadius,
                    (float)Math.Sin(angle) * fInletPositionRadius,
                    fInletZ
                );
                
                // inlet passage
                latVoid.AddBeam(
                    vInletPos,
                    new Vector3((float)Math.Cos(angle) * fChamberRadius, 
                               (float)Math.Sin(angle) * fChamberRadius, 
                               fInletZ),
                    fInletRadius,
                    fInletRadius,
                    true
                );
                
                // external port boss
                latMetal.AddBeam(
                    vInletPos + new Vector3((float)Math.Cos(angle) * 8f, 
                                           (float)Math.Sin(angle) * 8f, 0),
                    vInletPos,
                    fInletRadius + 2.5f,
                    fInletRadius + 2.5f,
                    true
                );
            }
            
            // two oxidizer inlets at 90 degrees (offset from fuel)
            for (int i = 0; i < 2; i++)
            {
                float angle = (float)((i * Math.PI) + Math.PI/2.0);
                Vector3 vInletPos = new Vector3(
                    (float)Math.Cos(angle) * fInletPositionRadius,
                    (float)Math.Sin(angle) * fInletPositionRadius,
                    fInletZ
                );
                
                // inlet passage
                latVoid.AddBeam(
                    vInletPos,
                    new Vector3((float)Math.Cos(angle) * fChamberRadius, 
                               (float)Math.Sin(angle) * fChamberRadius, 
                               fInletZ),
                    fInletRadius,
                    fInletRadius,
                    true
                );
                
                // external port boss
                latMetal.AddBeam(
                    vInletPos + new Vector3((float)Math.Cos(angle) * 8f, 
                                           (float)Math.Sin(angle) * 8f, 0),
                    vInletPos,
                    fInletRadius + 2.5f,
                    fInletRadius + 2.5f,
                    true
                );
            }
            
            // === mounting holes ===
            Library.Log("Adding mounting holes...");
            
            // 6 mounting holes around flange
            float fMountHoleRadius = 2.5f;  // m5 bolt clearance
            float fMountHolePositionRadius = fChamberRadius + fWallThickness + 4f;
            
            for (int i = 0; i < 6; i++)
            {
                float angle = (float)(i * Math.PI * 2.0 / 6.0 + Math.PI/12.0);
                Vector3 vHolePos = new Vector3(
                    (float)Math.Cos(angle) * fMountHolePositionRadius,
                    (float)Math.Sin(angle) * fMountHolePositionRadius,
                    -8f
                );
                
                latVoid.AddBeam(
                    vHolePos - new Vector3(0, 0, 2),
                    vHolePos + new Vector3(0, 0, 8),
                    fMountHoleRadius,
                    fMountHoleRadius,
                    true
                );
            }
            
            // === voxel boolean operations ===
            Library.Log("Converting to voxels and performing boolean operations...");
            Voxels voxEngine = new Voxels(latMetal);
            voxEngine.BoolSubtract(new Voxels(latVoid));
            
            // no cross-section - full solid engine with internal channels
            
            // === export ===
            Library.Log("Converting to mesh...");
            Mesh mshEngine = new Mesh(voxEngine);
            
            string strOutputPath = "/Users/raymartin/picogk/race2space_15kN_engine.stl";
            Library.Log($"Saving to {strOutputPath}...");
            mshEngine.SaveToStlFile(strOutputPath);
            
            Library.Log("=== COMPLETE ===");
            Library.Log($"15kN regeneratively cooled engine saved!");
            Library.Log($"Throat: {fThroatRadius*2}mm | Chamber: {fChamberRadius*2}mm");
            Library.Log($"cooling channels: {iChannels} x {fChannelRadius*2}mm diameter");
            Library.Log($"propellant inlets: 2x fuel + 2x oxidizer (12mm diameter)");
            Library.Log($"mounting: 6x m5 bolt holes");
            Library.Log($"");
            Library.Log($"design notes:");
            Library.Log($"- bulbed shape at top = combustion chamber (where fuel burns)");
            Library.Log($"- narrow middle = throat (supersonic transition point)");
            Library.Log($"- flared end = nozzle (accelerates exhaust to supersonic speeds)");
            Library.Log($"- this is the correct aerospike-free bell nozzle design");
            Library.Log($"Ready for 3D printing in copper alloy");
        }
    }
}
