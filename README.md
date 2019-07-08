# KK_CrossEye

This is my first plugin so it might not be the best. Please make issues for any bugs which you find (and are not listed in the compatibility section)

**Patreon page:** https://www.patreon.com/2155X  
**Screenshots of the plugin in action:** [New](https://imgur.com/a/r0J51R4) [Old](https://imgur.com/a/PEKE5rW)  
**Video of the plugin in action:** coming soon  

**Thank you:**
1. *essu#1145* for the start and ideas,  
2. *ManlyMarco#7309* for help and ideas.  

**Works in:**
1. *Studio*  
2. *School Mode*  
3. *Free-H*  
4. *Live*  

**Compatibility:**
1. **Studio**  
   * focus does not work (no colliders, not fixable for now)  

2. **School Mode**  
   * conversations might break 3D mode, just click the bind twice again to reload the cross eye mode  
   * ~~focus does not work~~ Fixed in v1.2  

3. **During H**  
   * touching/kissing will not work with the cross eye mode enabled  

4. **Live**  
   * focus does not work (no colliders, not fixable for now)  

5. **General**  
   * UI is not converted to 3D so clicking space with mods to hide it gives the best experience
   * ~~focus-in is not smooth~~ Fixed in v1.3  
   * focus-out is not smooth

6. **Mods**
   * kPlug | works good so far  


Will try fixing as many issues in compatibility as I can.

This is like a VR plugin but without the need of a VR headset. It works by placing 2 cameras in slightly different positions from eachother and splitting the screen 'in half' with them.

To see the 3D effect you need to cross your eyes (imagine taking your finger and putting it very close to your face) by crossing your eyes, try to move both images into one center image. You should see 3D if you do it correctly.

Here's a random youtube video which explains **how to cross your eyes**: https://www.youtube.com/watch?v=yNpIDSqTJ_Y

**Please don't ruin your eyes and don't play with this without breaks, I recommend breaks every 2 minutes for at least 15 seconds.**

To add more realism, I've also coded in a **focus** mechanism which is not the best but will be improved later. If you get close enough to an object it will 'rotate the camera views' just like your eyes when you look at something very close. This allows you to get even closer to objects and still have them stay on your screen.

**The focus is very alpha:**  
   *Currently it only focuses on characters.*  
   *I will make it a little smoother with the next updates.*  

This plugin might drop your fps **slightly** when activated. 

It is activated/deactivated by clicking **Numpad 1** by default. 
A config is also included for various settings, including the key bind.

I suggest leaving the settings at default, however you could also adjust them to fit your eyes better. If you believe that you made a good preset you can create an 'issue' and I will add it to the list of presets.

Please be careful with the **EXPERIMENTAL** settings as they can completely mess up the focus mode. Refer to the code if you want to mess with them.

**Presets:**  
1. **Default**  
   IPD: 0.18  
   Angle: 2.5  
   Focus In: 0.075  
   Focus Dst: 1.0  
   Focus Mul: 10  
   Focus Ttl: 10  
