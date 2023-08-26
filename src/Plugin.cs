using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using System.Security;
using System.Reflection;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Placek;

[BepInPlugin("evil.placek", "Placek", "1.1.1")]
sealed class Plugin : BaseUnityPlugin
{
    FAtlas mainAtlas;
    FAtlas placekAtlas;
    public bool scavengerKill;
    static bool loaded = false;
    public static readonly object placekSprite = new object();
    const int vertsPerColumn = 64;
    public static RoomSettings.RoomEffect.Type Placek;
    public Texture2D placekTex;

    static MaterialPropertyBlock placekWater;

    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;

        On.ScavengerGraphics.DrawSprites += ScavengerGraphics_DrawSprites;

        On.DaddyGraphics.DrawSprites += DaddyGraphics_DrawSprites;

        On.SpiderGraphics.DrawSprites += SpiderGraphics_DrawSprites;

        On.BigEelGraphics.DrawSprites += BigEelGraphics_DrawSprites;

        On.JetFishGraphics.DrawSprites += JetFishGraphics_DrawSprites;

        On.SnailGraphics.DrawSprites += SnailGraphics_DrawSprites;

        On.Water.DrawSprites += Water_DrawSprites;
        //On.MoreSlugcats.YeekGraphics.DrawSprites += YeekGraphics_DrawSprites;
        //On.MoreSlugcats.YeekGraphics.InitiateSprites += YeekGraphics_InitiateSprites;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        Debug.Log("entered onmods");
        mainAtlas ??= Futile.atlasManager.LoadAtlas("atlases/placek");

        // This chunk and following are unworking placek water shaders, commented for later use (please dont yell at me for commenting this this is a silly)

        if (mainAtlas == null)
        {
            Logger.LogWarning("Placek will not load silly!!! plz reinstall :3");
        }

        Debug.Log("Entered LoadResources");
        if (!loaded)
        {
            Debug.Log("Loading");
            var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("bundles/placek"));
            self.Shaders["Placek"] = FShader.CreateShader("Placek", bundle.LoadAsset<Shader>("Assets/shaders 1.9.03/Placek.shader"));
            Debug.Log("Loaded " + bundle + " and " + bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/placekDark.png"));
            placekTex = bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/placekDark.png");
            Debug.Log("Placek is " + placekTex.height + " and " + placekTex.width);
            placekWater = new();
            placekWater.SetTexture("placekTex", placekTex);
            Shader.SetGlobalTexture("_PlacekTex", placekWater.GetTexture("placekTex"));
            loaded = true;
            Debug.Log("Finished loading");
        }

    }

    private void Water_DrawSprites(On.Water.orig_DrawSprites orig, Water self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        placekWater.SetTexture("_PlacekTex", placekTex);
        sLeaser.sprites[0].shader = self.room.game.rainWorld.Shaders["Placek"];
    }

    // This chunk is currently unworking yeek placek, commented for later use (please dont yell at me for commenting this this is a silly)
    /*
    private void YeekGraphics_InitiateSprites(On.MoreSlugcats.YeekGraphics.orig_InitiateSprites orig, MoreSlugcats.YeekGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
                if (atlas == null)
        {
            return;
        }

        string name = sLeaser.sprites[self.HeadSpritesStart]?.element?.name;
        atlas._elementsByName.TryGetValue("placekLight", out var element);
        sLeaser.sprites[self.BodySpritesStart] = new FSprite(element);
        sLeaser.sprites[self.BodySpritesStart].color = Color.white;

    }

    private void YeekGraphics_DrawSprites(On.MoreSlugcats.YeekGraphics.orig_DrawSprites orig, MoreSlugcats.YeekGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);


        //sLeaser.sprites[sLeaser.sprites.Length - 1].SetPosition(Vector2.Lerp((self.owner as Yeek).bodyChunks[0].pos, (self.owner as Yeek).bodyChunks[0].lastPos, timeStacker));
    }
    */
    private void SpiderGraphics_DrawSprites(On.SpiderGraphics.orig_DrawSprites orig, SpiderGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (mainAtlas == null)
        {
            return;
        }
        string name = sLeaser.sprites[self.BodySprite]?.element?.name;
        if (name != null && name.StartsWith("Spider") && mainAtlas._elementsByName.TryGetValue("placekDark", out var element))
        {
            sLeaser.sprites[self.BodySprite].element = element;
            sLeaser.sprites[self.BodySprite].color = Color.white;
        }

    }

    private void DaddyGraphics_DrawSprites(On.DaddyGraphics.orig_DrawSprites orig, DaddyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (mainAtlas == null)
        {
            return;
        }
        for (int i = 0; i < self.daddy.bodyChunks.Length; i++)
        {
            string name = sLeaser.sprites[self.BodySprite(i)]?.element?.name;
            if (name != null && name.StartsWith("Futile") && mainAtlas._elementsByName.TryGetValue("placekDark", out var element))
            {
                sLeaser.sprites[self.BodySprite(i)].element = element;
                sLeaser.sprites[self.BodySprite(i)].color = Color.white;
                sLeaser.sprites[self.BodySprite(i)].shader = FShader.Solid;
                sLeaser.sprites[self.BodySprite(i)].scaleX *= .5f;
                sLeaser.sprites[self.BodySprite(i)].scaleY *= .5f;
            }
        }

    }

    private void BigEelGraphics_DrawSprites(On.BigEelGraphics.orig_DrawSprites orig, BigEelGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (mainAtlas == null)
        {
            return;
        }

        
        for (int i = 0; i < self.owner.bodyChunks.Length; i++)
        {
            string name = sLeaser.sprites[self.BodyChunksSprite(i)]?.element?.name;
            if (name != null && name.StartsWith("Futile") && mainAtlas._elementsByName.TryGetValue("placekDark", out var element))
            {
                sLeaser.sprites[self.BodyChunksSprite(i)].element = element;
                sLeaser.sprites[self.BodyChunksSprite(i)].shader = FShader.Solid;
                sLeaser.sprites[self.BodyChunksSprite(i)].scaleX *= .5f;
                sLeaser.sprites[self.BodyChunksSprite(i)].scaleY *= .5f;
            }
        }
        placekWater.SetTexture("_PlacekTex", placekTex);
        sLeaser.sprites[self.MeshSprite].shader = rCam.room.game.rainWorld.Shaders["Placek"];

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < self.numberOfScales; j++)
            {
                string name = sLeaser.sprites[self.BodyChunksSprite(i)]?.element?.name;
                if (name != null && name.StartsWith("Lizard") && mainAtlas._elementsByName.TryGetValue("placekLight", out var element))
                {
                    sLeaser.sprites[self.ScaleSprite(i, j)].element = element;
                    sLeaser.sprites[self.ScaleSprite(i, j)].shader = FShader.Solid;
                }
            }
            for (int j = 0; j < self.numberOfEyes; j++)
            {
                string name = sLeaser.sprites[self.EyeSprite(j, i)]?.element?.name;
                if (name != null && name.StartsWith("Cicada") && mainAtlas._elementsByName.TryGetValue("placekLight", out var element))
                {
                    sLeaser.sprites[self.EyeSprite(j, i)].element = element;
                }
            }
        }


    }

    private void SnailGraphics_DrawSprites(On.SnailGraphics.orig_DrawSprites orig, SnailGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (mainAtlas == null)
        {
            return;
        }
        string name = sLeaser.sprites[6]?.element?.name;
        if (name != null && name.StartsWith("Snail") && mainAtlas._elementsByName.TryGetValue("placekLight", out var element))
        {
            sLeaser.sprites[6].element = element;
            sLeaser.sprites[6].color = Color.Lerp(self.snail.shellColor[0], Color.white, .20f);
        }

        sLeaser.sprites[7].scaleX = 0.001f;
        sLeaser.sprites[7].scaleY = 0.001f;
    }
    private void JetFishGraphics_DrawSprites(On.JetFishGraphics.orig_DrawSprites orig, JetFishGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (mainAtlas == null)
        {
            return;
        }

        string name = sLeaser.sprites[self.BodySprite]?.element?.name;
        if (name != null && name.StartsWith("Jet") && mainAtlas._elementsByName.TryGetValue("gleaming", out var element))
        {
            sLeaser.sprites[self.BodySprite].element = element;
            sLeaser.sprites[self.BodySprite].color = Color.white;
        }

        sLeaser.sprites[self.BehindEyeSprite].scaleX = 0.001f;
        sLeaser.sprites[self.BehindEyeSprite].scaleY = 0.001f;

        for (int  i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                sLeaser.sprites[self.EyeSprite(i, j)].scaleX = 0.001f;
                sLeaser.sprites[self.EyeSprite(i, j)].scaleY = 0.001f;
            }
        }
    }

    public void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        //Debug.Log("got to check");
        if (mainAtlas == null)
        {
            return;
        }
        //Debug.Log("Passed DrawSprites");
        string name = sLeaser.sprites[3]?.element?.name;
        if (name != null && name.StartsWith("HeadA") && mainAtlas._elementsByName.TryGetValue("gleaming", out var element))
        {
            //Debug.Log("Entered if statement");
            sLeaser.sprites[3].element = element;
        }
        sLeaser.sprites[9].scaleX = 0.001f;
        sLeaser.sprites[9].scaleY = 0.001f;
    }

    private void ScavengerGraphics_DrawSprites(On.ScavengerGraphics.orig_DrawSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (mainAtlas == null)
        {
            return;
        }
        //Debug.Log("Passed DrawSprites");
        string name = sLeaser.sprites[self.HeadSprite]?.element?.name;
        //Debug.Log(name);
            for (int a = 0; a < (self.owner as Scavenger).abstractCreature.abstractAI.RealAI.tracker.creatures.Count; a++)
            {
                if ((self.owner as Scavenger).abstractCreature.abstractAI.RealAI.tracker.creatures.ElementAt(a) != null && !(self.owner as Creature).inShortcut)
                {
                    if ((self.owner as Scavenger).WantToLethallyAttack((self.owner as Scavenger).abstractCreature.abstractAI.RealAI.tracker.creatures.ElementAt(a)))
                    {
                        scavengerKill = true;
                    break;
                    }
                    else
                    {
                        scavengerKill = false;
                    }
                }
            }
     


        if (!scavengerKill)
        {
            if (name != null && name.StartsWith("Circle") && mainAtlas._elementsByName.TryGetValue("placekLight", out var element))
            {
                sLeaser.sprites[self.HeadSprite].element = element;
                sLeaser.sprites[self.HeadSprite].color = Color.white;
                sLeaser.sprites[self.HeadSprite].rotation = 0;
                sLeaser.sprites[self.HeadSprite].scaleX = 1f;
                sLeaser.sprites[self.HeadSprite].scaleY = 1f;
            }
        }

        else if (scavengerKill)
        {
            if (name != null && name.StartsWith("Circle") && mainAtlas._elementsByName.TryGetValue("placekDark", out var element))
            {
                sLeaser.sprites[self.HeadSprite].element = element;
                sLeaser.sprites[self.HeadSprite].color = Color.white;
                sLeaser.sprites[self.HeadSprite].rotation = 0;
                sLeaser.sprites[self.HeadSprite].scaleX = 1f;
                sLeaser.sprites[self.HeadSprite].scaleY = 1f;
            }
        }
        sLeaser.sprites[self.EyeSprite(0, 0)].scaleX = 0.001f;
        sLeaser.sprites[self.EyeSprite(0, 0)].scaleY = 0.001f;
        sLeaser.sprites[self.EyeSprite(1, 0)].scaleX = 0.001f;
        sLeaser.sprites[self.EyeSprite(1, 0)].scaleY = 0.001f;

    
    }
}
