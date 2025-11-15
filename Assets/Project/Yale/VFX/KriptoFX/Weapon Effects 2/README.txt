Version 1.0.0

- My email is "kripto289@gmail.com"
- Discord channel https://discord.gg/GUUZ9D96Uq






-----------------------------------  FIRST STEPS ------------------------------------------------------------------------------------------------------------

1) If you are using URP or HDRP you should import the URP patch from the folder (\Assets\KriptoFX\WeaponEffects 2\HDRP and URP patches)
2) Just drag and drop prefabs of effects and decals on your mesh. 
3) All effects are configured for HDR rendering with bloom postprocessing, so I strongly recommend using this, otherwise the effects will look pale.

Bloom settings for STANDARD (non URP or HDRP) rendering: http://kripto289.com/Shared/Readme/DefaultRenderingSettings.jpg
Bloom settings for URP rendering: http://kripto289.com/Shared/Readme/URPRenderingSettings.jpg
Bloom setting for HDRP rendering: works out of the box. 
--------------------------------------------------------------------------------------------------------------------------------------------------------------------



Notes: 
I use own screenspace decals so that I can add an effect to a specific area of the weapon (instead of additional materials).
Also remember that decals can affect surrounding objects, so try to choose an effective size. (layer mask not supported).

for SCALING just use transform scale (for effect1 you need to set scale in the script ME2_ParticleScale, because unity has bug with new alighned particles and native scaling) 





