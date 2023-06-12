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

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Placek;

[BepInPlugin("evil.placek", "Placek", "1.0")]
sealed class Plugin : BaseUnityPlugin
{
    FAtlas atlas;
    public bool scavengerKill;
    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.ScavengerGraphics.DrawSprites += ScavengerGraphics_DrawSprites;
        On.Scavenger.ctor += Scavenger_ctor;
        On.DaddyGraphics.DrawSprites += DaddyGraphics_DrawSprites;
        On.SpiderGraphics.DrawSprites += SpiderGraphics_DrawSprites;
        //On.MoreSlugcats.YeekGraphics.DrawSprites += YeekGraphics_DrawSprites;
        //On.MoreSlugcats.YeekGraphics.InitiateSprites += YeekGraphics_InitiateSprites;
    }

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

        if (atlas == null)
        {
            return;
        }
        string name = sLeaser.sprites[self.BodySprite]?.element?.name;
        if (name != null && name.StartsWith("Spider") && atlas._elementsByName.TryGetValue("placekDark", out var element))
        {
            sLeaser.sprites[self.BodySprite].element = element;
            sLeaser.sprites[self.BodySprite].color = Color.white;
        }

    }

    private void DaddyGraphics_DrawSprites(On.DaddyGraphics.orig_DrawSprites orig, DaddyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (atlas == null)
        {
            return;
        }
        for (int i = 0; i < self.daddy.bodyChunks.Length; i++)
        {
            string name = sLeaser.sprites[self.BodySprite(i)]?.element?.name;
            if (name != null && name.StartsWith("Futile") && atlas._elementsByName.TryGetValue("placekDark", out var element))
            {
                sLeaser.sprites[self.BodySprite(i)].element = element;
                sLeaser.sprites[self.BodySprite(i)].color = Color.white;
                sLeaser.sprites[self.BodySprite(i)].shader = FShader.Solid;
                sLeaser.sprites[self.BodySprite(i)].scaleX *= .5f;
                sLeaser.sprites[self.BodySprite(i)].scaleY *= .5f;
            }
        }

    }

    private void Scavenger_ctor(On.Scavenger.orig_ctor orig, Scavenger self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
    }

    public void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        //Debug.Log("entered onmods");
        atlas ??= Futile.atlasManager.LoadAtlas("atlases/placek");

        if (atlas == null)
        {
            Logger.LogWarning("Placek will not load silly!!! plz reinstall :3");
        }
    }

    public void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        //Debug.Log("got to check");
        if (atlas == null)
        {
            return;
        }
        //Debug.Log("Passed DrawSprites");
        string name = sLeaser.sprites[3]?.element?.name;
        if (name != null && name.StartsWith("HeadA") && atlas._elementsByName.TryGetValue("gleaming", out var element))
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
        if (atlas == null)
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
            if (name != null && name.StartsWith("Circle") && atlas._elementsByName.TryGetValue("placekLight", out var element))
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
            if (name != null && name.StartsWith("Circle") && atlas._elementsByName.TryGetValue("placekDark", out var element))
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
