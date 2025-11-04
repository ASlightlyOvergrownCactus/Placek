﻿using System;
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
using Random = System.Random;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Placek;

[BepInPlugin("evil.placek", "Placek", "2.0")]
sealed class Plugin : BaseUnityPlugin
{
    FAtlas mainAtlas;
    FAtlas placekAtlas;
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
        
        On.LizardGraphics.DrawSprites += LizardGraphicsOnDrawSprites;
        
        On.CentipedeGraphics.DrawSprites += CentipedeGraphicsOnDrawSprites;
        
        On.TempleGuardGraphics.DrawSprites += TempleGuardGraphicsOnDrawSprites;
        
        On.OracleGraphics.DrawSprites += OracleGraphicsOnDrawSprites;
        
        On.DeerGraphics.DrawSprites += DeerGraphicsOnDrawSprites;
        On.DeerGraphics.ApplyPalette += DeerGraphicsOnApplyPalette;
        
        On.AboveCloudsView.CloseCloud.DrawSprites += CloseCloudOnDrawSprites;
        On.AboveCloudsView.DistantCloud.DrawSprites += DistantCloudOnDrawSprites;
        On.AboveCloudsView.FlyingCloud.DrawSprites += FlyingCloudOnDrawSprites;
        
        On.GoldFlakes.GoldFlake.DrawSprites += GoldFlakeOnDrawSprites;
        On.Ghost.DrawSprites += GhostOnDrawSprites;

        On.Water.DrawSprites += Water_DrawSprites;
        On.FAtlasManager.GetElementWithName += FAtlasManagerOnGetElementWithName;
        //On.MoreSlugcats.YeekGraphics.DrawSprites += YeekGraphics_DrawSprites;
        //On.MoreSlugcats.YeekGraphics.InitiateSprites += YeekGraphics_InitiateSprites;
    }

    private void GhostOnDrawSprites(On.Ghost.orig_DrawSprites orig, Ghost self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (mainAtlas == null)
        {
            return;
        }

        for (int i = 0; i < sleaser.sprites.Length; i++)
        {
            if (mainAtlas._elementsByName.TryGetValue("glapeCat", out var element))
            {
                sleaser.sprites[i].element = element;
            }
        }
    }

    private void GoldFlakeOnDrawSprites(On.GoldFlakes.GoldFlake.orig_DrawSprites orig, GoldFlakes.GoldFlake self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (mainAtlas == null)
        {
            return;
        }
        string name = sleaser.sprites[0]?.element?.name;
        if (name != null && name.StartsWith("Pebble") && mainAtlas._elementsByName.TryGetValue("glapeCat", out var element))
        {
            sleaser.sprites[0].element = element;
        }
    }

    private void FlyingCloudOnDrawSprites(On.AboveCloudsView.FlyingCloud.orig_DrawSprites orig, AboveCloudsView.FlyingCloud self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (mainAtlas == null)
        {
            return;
        }
        string name = sleaser.sprites[0]?.element?.name;
        if (name != null && name.StartsWith("flying") && mainAtlas._elementsByName.TryGetValue("placekDark", out var element))
        {
            sleaser.sprites[0].element = element;
        }
    }

    private void DistantCloudOnDrawSprites(On.AboveCloudsView.DistantCloud.orig_DrawSprites orig, AboveCloudsView.DistantCloud self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (mainAtlas == null)
        {
            return;
        }
        string name = sleaser.sprites[1]?.element?.name;
        if (name != null && name.StartsWith("clouds") && mainAtlas._elementsByName.TryGetValue("placekDark", out var element))
        {
            sleaser.sprites[1].element = element;
        }
    }

    private void CloseCloudOnDrawSprites(On.AboveCloudsView.CloseCloud.orig_DrawSprites orig, AboveCloudsView.CloseCloud self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (mainAtlas == null)
        {
            return;
        }
        string name = sleaser.sprites[1]?.element?.name;
        if (name != null && name.StartsWith("clouds") && mainAtlas._elementsByName.TryGetValue("placekDark", out var element))
        {
            sleaser.sprites[1].element = element;
        }
    }

    // Written by alduris for making this method
    public static Texture2D GetSpriteFromAtlas(FAtlasElement element)
    {
        Texture2D atlasTex = element.atlas.texture as Texture2D;
        if (element.atlas.texture.name != "")
        {
            var oldRT = RenderTexture.active;

            var rt = new RenderTexture(atlasTex.width, atlasTex.height, 32, RenderTextureFormat.ARGB32);
            Graphics.Blit(atlasTex, rt);
            RenderTexture.active = rt;
            atlasTex = new Texture2D(atlasTex.width, atlasTex.height, TextureFormat.ARGB32, false);
            atlasTex.ReadPixels(new Rect(0, 0, atlasTex.width, atlasTex.height), 0, 0);

            RenderTexture.active = oldRT;
        }

        // Get sprite pos and size
        var pos = element.uvRect.position * element.atlas.textureSize; // sprite.element.sourceRect says the sprite is at (0, 0), it is not
        var size = element.sourceRect.size;

        // Fix size issues
        if (pos.x + size.x > atlasTex.width) size = new Vector2(atlasTex.width - pos.x, size.y);
        if (pos.y + size.y > atlasTex.height) size = new Vector2(size.x, atlasTex.height - pos.y);

        // Get the texture
        var tex = new Texture2D((int)size.x, (int)size.y, atlasTex.format, 1, false);
        Graphics.CopyTexture(atlasTex, 0, 0, (int)pos.x, (int)pos.y, (int)size.x, (int)size.y, tex, 0, 0, 0, 0);
        return tex;
    }

    private void DeerGraphicsOnApplyPalette(On.DeerGraphics.orig_ApplyPalette orig, DeerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, RoomPalette palette)
    {
        orig(self, sleaser, rcam, palette);
        sleaser.sprites[self.BodySprite(0)].color = Color.white;
    }

    private void DeerGraphicsOnDrawSprites(On.DeerGraphics.orig_DrawSprites orig, DeerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        int bodyNum = 0;
        if (mainAtlas == null)
        {
            return;
        }
        string name = sleaser.sprites[self.BodySprite(bodyNum)]?.element?.name;
        if (name != null && name.StartsWith("Futile") && mainAtlas._elementsByName.TryGetValue("chudCat", out var element))
        {
            sleaser.sprites[self.BodySprite(bodyNum)].element = element;
            sleaser.sprites[self.BodySprite(bodyNum)].shader = self.owner.room.game.rainWorld.Shaders["Basic"];
            sleaser.sprites[self.BodySprite(bodyNum)].color = Color.white;
            sleaser.sprites[self.BodySprite(bodyNum)].alpha = 1f;
            for (int i = 0; i < 2; i++)
            {
                sleaser.sprites[self.EyeSprite(i, 0)].scaleX = 0.0001f;
                sleaser.sprites[self.EyeSprite(i, 0)].scaleY = 0.0001f;
                sleaser.sprites[self.EyeSprite(i, 1)].scaleX = 0.0001f;
                sleaser.sprites[self.EyeSprite(i, 1)].scaleY = 0.0001f;
            }
        }
    }

    private void OracleGraphicsOnDrawSprites(On.OracleGraphics.orig_DrawSprites orig, OracleGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        
        if (mainAtlas == null)
        {
            return;
        }
        string name = sleaser.sprites[self.HeadSprite]?.element?.name;
        if ((self.IsMoon || self.IsPastMoon) && name != null && name.StartsWith("Circle") && mainAtlas._elementsByName.TryGetValue("placekLight", out var element))
        {
            for (int i = 0; i < 2; i++)
            {
                sleaser.sprites[self.EyeSprite(i)].scaleX = 0.00001f;
                sleaser.sprites[self.EyeSprite(i)].scaleY = 0.00001f;
            }
            sleaser.sprites[self.ChinSprite].scaleX = 0.00001f;
            sleaser.sprites[self.ChinSprite].scaleY = 0.00001f;
            sleaser.sprites[self.HeadSprite].element = element;
            sleaser.sprites[self.HeadSprite].rotation += 180f;
        }
        else if ((self.IsPebbles || self.IsRottedPebbles || self.IsSaintPebbles) && name != null &&
                 name.StartsWith("Circle") && mainAtlas._elementsByName.TryGetValue("placekDark", out var element2))
        {
            for (int i = 0; i < 2; i++)
            {
                sleaser.sprites[self.EyeSprite(i)].scaleX = 0.00001f;
                sleaser.sprites[self.EyeSprite(i)].scaleY = 0.00001f;
            }
            sleaser.sprites[self.ChinSprite].scaleX = 0.00001f;
            sleaser.sprites[self.ChinSprite].scaleY = 0.00001f;
            sleaser.sprites[self.HeadSprite].element = element2;
            sleaser.sprites[self.HeadSprite].rotation += 180f;
        }
        else if (self.IsStraw && name != null && name.StartsWith("Circle") && mainAtlas._elementsByName.TryGetValue("gleaming", out var element3))
        {
            for (int i = 0; i < 2; i++)
            {
                sleaser.sprites[self.EyeSprite(i)].scaleX = 0.00001f;
                sleaser.sprites[self.EyeSprite(i)].scaleY = 0.00001f;
            }
            sleaser.sprites[self.ChinSprite].scaleX = 0.00001f;
            sleaser.sprites[self.ChinSprite].scaleY = 0.00001f;
            sleaser.sprites[self.HeadSprite].element = element3;
            sleaser.sprites[self.HeadSprite].rotation += 180f;
        }
    }

    private void TempleGuardGraphicsOnDrawSprites(On.TempleGuardGraphics.orig_DrawSprites orig, TempleGuardGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        
        if (mainAtlas == null)
        {
            return;
        }
        string name = sleaser.sprites[self.EyeSprite(0)]?.element?.name;
        if (name != null && name.StartsWith("guard") && mainAtlas._elementsByName.TryGetValue("placekLight", out var element))
        {
            for (int i = 0; i < 2; i++)
            {
                sleaser.sprites[self.EyeSprite(i)].element = element;
            }
        }
    }

    private void CentipedeGraphicsOnDrawSprites(On.CentipedeGraphics.orig_DrawSprites orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        
        if (mainAtlas == null)
        {
            return;
        }
        string name = sleaser.sprites[self.SegmentSprite(0)]?.element?.name;
        if (name != null && name.StartsWith("Centipede") && mainAtlas._elementsByName.TryGetValue("placekDark", out var element))
        {
            for (int i = 0; i < self.owner.bodyChunks.Length; i++)
            {
                for (int j = 0; j < (self.centipede.AquaCenti ? 2 : 1); j++)
                {
                    sleaser.sprites[self.ShellSprite(i, j)].element = element;
                    sleaser.sprites[self.ShellSprite(i, j)].shader = FShader.Solid;
                }
            }
        }
    }

    private void LizardGraphicsOnDrawSprites(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        
        if (mainAtlas == null)
        {
            return;
        }
        string name = sleaser.sprites[self.SpriteHeadStart + 3]?.element?.name;
        if (name != null && name.StartsWith("Lizard") && mainAtlas._elementsByName.TryGetValue("placekDark", out var element))
        {
            sleaser.sprites[self.SpriteHeadStart + 3].element = element;
            sleaser.sprites[self.SpriteHeadStart + 1].scaleX = 0.0001f;
            sleaser.sprites[self.SpriteHeadStart + 1].scaleY = 0.0001f;
            sleaser.sprites[self.SpriteHeadStart + 2].scaleX = 0.0001f;
            sleaser.sprites[self.SpriteHeadStart + 2].scaleY = 0.0001f;
            sleaser.sprites[self.SpriteHeadStart].scaleX = 0.0001f;
            sleaser.sprites[self.SpriteHeadStart].scaleY = 0.0001f;
            //sleaser.sprites[self.BodySprite].color = Color.white;
        }
    }

    private FAtlasElement FAtlasManagerOnGetElementWithName(On.FAtlasManager.orig_GetElementWithName orig, FAtlasManager self, string elementname)
    {
        {
            Random random = new Random();
            int num = random.Next(100);
            int num2 = random.Next(7);

            if (mainAtlas == null)
            {
                return orig(self, elementname);
            }

            if (elementname != null && num == 98)
            {
                switch (num2)
                {
                    case 0 when mainAtlas._elementsByName.TryGetValue("placekDark", out var element):
                        return element;
                    case 1 when mainAtlas._elementsByName.TryGetValue("placekLight", out var element1):
                        return element1;
                    case 2 when mainAtlas._elementsByName.TryGetValue("gleaming", out var element3):
                        return element3;
                    case 3 when mainAtlas._elementsByName.TryGetValue("glapeCat", out var element4):
                        return element4;
                    case 4 when mainAtlas._elementsByName.TryGetValue("denseCat", out var element5):
                        return element5;
                    case 5 when mainAtlas._elementsByName.TryGetValue("chudCat", out var element6):
                        return element6;
                    case 6 when mainAtlas._elementsByName.TryGetValue("evilCat", out var element7):
                        return element7;
                    default:
                        mainAtlas._elementsByName.TryGetValue("placekDark", out var element8);
                        return element8;
                }
            }

            return orig(self, elementname);
        }
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
            //Debug.Log("Loading");
            var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("bundles/placek"));
            self.Shaders["Placek"] = FShader.CreateShader("Placek", bundle.LoadAsset<Shader>("Assets/shaders 1.9.03/Placek.shader"));
            //Debug.Log("Loaded " + bundle + " and " + bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/placekDark.png"));
            placekTex = bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/placekDark.png");
            //Debug.Log("Placek is " + placekTex.height + " and " + placekTex.width);
            placekWater = new();
            placekWater.SetTexture("placekTex", placekTex);
            Shader.SetGlobalTexture("_PlacekTex", placekWater.GetTexture("placekTex"));

            if (mainAtlas._elementsByName.TryGetValue("placekDark", out var element))
            {
                self.apartmentsTex = GetSpriteFromAtlas(element);
            }
            
            loaded = true;
            //Debug.Log("Finished loading");
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
        string name = sLeaser.sprites[self.HeadSprite]?.element?.name;
        string behavior = "";
        float check = 0.65f;
            for (int a = 0; a < (self.owner as Scavenger).abstractCreature.abstractAI.RealAI.tracker.creatures.Count; a++)
            {
                if ((self.owner as Scavenger).abstractCreature.abstractAI.RealAI.tracker.creatures.ElementAt(a) != null && !(self.owner as Creature).inShortcut)
                {
                    if ((self.owner as Scavenger).abstractCreature.personality.aggression > check)
                        behavior = "evilCat";
                    else if ((self.owner as Scavenger).abstractCreature.personality.bravery > check)
                        behavior = "chudCat";
                    else if ((self.owner as Scavenger).abstractCreature.personality.dominance > check)
                        behavior = "denseCat";
                    else if ((self.owner as Scavenger).abstractCreature.personality.nervous > check)
                        behavior = "glapeCat";
                    else if ((self.owner as Scavenger).abstractCreature.personality.energy > check)
                        behavior = "placekDark";
                    else if ((self.owner as Scavenger).abstractCreature.personality.sympathy > check)
                        behavior = "gleaming";
                    else
                        behavior = "placekLight";
                }
            }
            
            if (name != null && name.StartsWith("Circle") && mainAtlas._elementsByName.TryGetValue(behavior, out var element))
            {
                sLeaser.sprites[self.HeadSprite].element = element;
                sLeaser.sprites[self.HeadSprite].color = Color.white;
                sLeaser.sprites[self.HeadSprite].rotation = 0;
                sLeaser.sprites[self.HeadSprite].scaleX = 1f;
                sLeaser.sprites[self.HeadSprite].scaleY = 1f;
            }
        sLeaser.sprites[self.EyeSprite(0, 0)].scaleX = 0.001f;
        sLeaser.sprites[self.EyeSprite(0, 0)].scaleY = 0.001f;
        sLeaser.sprites[self.EyeSprite(1, 0)].scaleX = 0.001f;
        sLeaser.sprites[self.EyeSprite(1, 0)].scaleY = 0.001f;

    
    }
}