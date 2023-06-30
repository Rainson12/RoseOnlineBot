//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Windows.Forms;

using RoseOnlineBot.Business;
using RoseOnlineBot.Models.Logic;
using RoseOnlineBot.Models.Metadata;
using RoseOnlineBot.Utils;
using System.IO.Packaging;
using System.Security.Cryptography.X509Certificates;

namespace RoseOnlineBot.Classes
{
    public class Combat
    {



        public void Loot(float maxDistance = 2000)
        {
            var items = GameData.Player.ItemsToPickup.ToArray();
            var playerX = GameData.Player.PosX;
            var playerY = GameData.Player.PosY;
            foreach (var item in items)
            {
                if (Vector2D.CalculateDistance(playerX, playerY, item.PosX, item.PosY) >= maxDistance) // too far away dont pickup
                {
                    GameData.Player.ItemsToPickup.Remove(GameData.Player.ItemsToPickup.First(x => x.ObjectId == item.ObjectId));
                    continue;
                }

                MoveToCoordinate(item.PosX, item.PosY, true);

                Thread.Sleep(50);
                GameData.Player.PickupItem(item.ObjectId);
                GameData.Player.ItemsToPickup.Remove(GameData.Player.ItemsToPickup.First(x => x.ObjectId == item.ObjectId));
                Thread.Sleep(50);
            }
        }

        public void MoveToCoordinate(float x, float y, bool checkMovement, bool interruptWhenAttacked = false)
        {
            if (Vector2D.CalculateDistance(GameData.Player.PosX, GameData.Player.PosY, x, y) >= 100)
            {
                GameData.Player.Move(x, y);
                while (Vector2D.CalculateDistance(GameData.Player.PosX, GameData.Player.PosY, x, y) >= 100)
                {
                    Thread.Sleep(50);
                    if (checkMovement)
                    {
                        if (GameData.Player.CurrentAnimation != Models.Logic.Animation.Run)
                            GameData.Player.Move(x, y);
                        if (interruptWhenAttacked == true && GameData.Player.Targets.Count > 0)
                            break;
                    }
                    
                }
            }
        }

        public void RestHp(float restUntilPercent = 100)
        {
            while (GameData.Player.CurrentAnimation != Models.Logic.Animation.Stand)
            {
                Thread.Sleep(100);
            }


            GameData.Player.ToggleSit();
            Thread.Sleep(500);
            while (Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < (restUntilPercent / 100) && GameData.Player.Targets.Count == 0)
            {
                if (GameData.Player.CurrentAnimation == Models.Logic.Animation.Stand)
                {
                    GameData.Player.ToggleSit();
                    Thread.Sleep(500);
                }
                Thread.Sleep(100);
            }

            if (GameData.Player.CurrentAnimation == Models.Logic.Animation.Sit)
            {
                GameData.Player.ToggleSit();
            }
            Thread.Sleep(1000);
        }



        public void SingleTargetMode()
        {
            try
            {
                bool looting = true;
                var startingPointX = GameData.Player.PosX;
                var startingPointY = GameData.Player.PosY;
                short maxAOEMobsInRange = 2;
                while (true)
                {
                    if (looting)
                    {
                        Loot();
                    }
                    // run back to starting point if too far away from it
                    if (Vector2D.CalculateDistance(startingPointX, startingPointY, GameData.Player.PosX, GameData.Player.PosY) >= 1000 && GameData.Player.Targets.Count == 0)
                    {
                        MoveToCoordinate(startingPointX, startingPointY, true, true);
                    }

                    // rest hp if too low
                    if (Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 0.5f && GameData.Player.Targets.Count == 0)
                    {
                        RestHp(100);
                    }


                    // find new target if currently doesnt have one
                    if (GameData.Player.Targets.Count == 0 && GameData.Player.FindNextTarget() is NpcEntity newTarget)
                    {
                        GameData.Player.Targets.Add(newTarget);
                    }

                    // kill targets
                    for (int i = 0; i < GameData.Player.Targets.Count; i++)
                    {
                        var target = GameData.Player.Targets[i];
                        if (target == null || !target.Exists)
                        {
                            GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                            i--;
                            continue;
                        }

                        while (target.Exists)
                        {
                            if (GameData.Player.TargetId != target.Id)
                                GameData.Player.TargetId = target.Id;

                            if (GameData.Player.CurrentAnimation == Animation.Stand || GameData.Player.CurrentAnimation == Animation.Sit) // When standing - just attack
                            {
                                GameData.Player.AttackTarget(target.DBId);
                                Thread.Sleep(300);
                                if (GameData.Player.CurrentAnimation == Animation.Stand || GameData.Player.CurrentAnimation == Animation.Sit)
                                {
                                    // cant reach target
                                    for (int x = 0; x < 10; x++)
                                    {
                                        GameData.Player.AttackTarget(target.DBId);
                                        Thread.Sleep(100);
                                        if (GameData.Player.CurrentAnimation != Animation.Stand)
                                        {
                                            break;
                                        }
                                    }
                                    if (GameData.Player.CurrentAnimation == Animation.Stand || GameData.Player.CurrentAnimation == Animation.Sit)
                                    {
                                        // ignore target
                                        GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                                        i--;
                                        break;
                                    }
                                }
                            }

                            foreach (var skill in GameData.Player.Skills)
                            {
                                if (skill.Enabled && skill.ManaCost < GameData.Player.MP && !skill.IsOnCooldown && target.Exists)
                                {
                                    if (skill.IsAOE)
                                    {
                                        var mobs = GameData.Player.GetMobs();
                                        var mobsInRange = mobs.Count(x => x.Exists && Vector2D.CalculateDistance(GameData.Player.PosX, GameData.Player.PosY, x.PosX, x.PosY) < skill.Range);

                                        if (mobsInRange == 0 || mobsInRange > maxAOEMobsInRange || Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 0.5f)
                                            continue;
                                    }
                                    if (skill.IsAOE)
                                        GameData.Player.CastSpellOnMySelf(skill.Slot);
                                    else
                                        GameData.Player.CastSpellOnTarget(target.DBId, skill.Slot);
                                    Thread.Sleep(150); // wait for the packet to be handled by the backend
                                    break;
                                }
                            }
                            while (
                                (GameData.Player.CurrentAnimation == Models.Logic.Animation.ExecutingSkill || // while executing the skill
                                GameData.Player.CurrentAnimation == Models.Logic.Animation.PrepareExecutingSkill) &&
                                (GameData.Player.CurrentAnimation != Models.Logic.Animation.Stand || // and not standing or basic attacking
                                GameData.Player.CurrentAnimation != Models.Logic.Animation.Attack)
                                )
                            {
                                Thread.Sleep(250);
                            }


                            if (!target.Exists)
                            {
                                GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                                i--;
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex) {
            }
        }


        public void PartyModeSingleTarget()
        {
            try
            {
                bool looting = true;
                var startingPointX = GameData.Player.PosX;
                var startingPointY = GameData.Player.PosY;
                short maxAOEMobsInRange = 2;
                while (true)
                {
                    if (looting)
                    {
                        Loot();
                    }
                    // run back to starting point if too far away from it
                    if (Vector2D.CalculateDistance(startingPointX, startingPointY, GameData.Player.PosX, GameData.Player.PosY) >= 1000 && GameData.Player.Targets.Count == 0)
                    {
                        MoveToCoordinate(startingPointX, startingPointY, true, false);
                    }

                    // rest hp if too low
                    if (Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 0.5f && GameData.Player.Targets.Count == 0)
                    {
                        MoveToCoordinate(startingPointX, startingPointY, true, false);
                    }


                    // find new target if currently doesnt have one
                    if (GameData.Player.Targets.Count == 0 && GameData.Player.FindNextTarget() is NpcEntity newTarget)
                    {
                        GameData.Player.Targets.Add(newTarget);
                    }

                    // kill targets
                    for (int i = 0; i < GameData.Player.Targets.Count; i++)
                    {
                        var target = GameData.Player.Targets[i];
                        if (target == null || !target.Exists)
                        {
                            GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                            i--;
                            continue;
                        }

                        while (target.Exists)
                        {
                            if (GameData.Player.TargetId != target.Id)
                                GameData.Player.TargetId = target.Id;
                            
                            
                            // basic attack target
                            if (GameData.Player.CurrentAnimation == Animation.Stand || GameData.Player.CurrentAnimation == Animation.Sit) // When standing - just attack
                            {
                                GameData.Player.AttackTarget(target.DBId);
                                Thread.Sleep(300);
                                if (GameData.Player.CurrentAnimation == Animation.Stand || GameData.Player.CurrentAnimation == Animation.Sit)
                                {
                                    // cant reach target
                                    for (int x = 0; x < 10; x++)
                                    {
                                        GameData.Player.AttackTarget(target.DBId);
                                        Thread.Sleep(100);
                                        if (GameData.Player.CurrentAnimation != Animation.Stand)
                                        {
                                            break;
                                        }
                                    }
                                    if (GameData.Player.CurrentAnimation == Animation.Stand || GameData.Player.CurrentAnimation == Animation.Sit)
                                    {
                                        // ignore target
                                        GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                                        i--;
                                        break;
                                    }
                                }
                            }

                            foreach (var skill in GameData.Player.Skills)
                            {
                                if (skill.Enabled && skill.ManaCost < GameData.Player.MP && !skill.IsOnCooldown && target.Exists)
                                {
                                    if (skill.IsAOE)
                                    {
                                        var mobs = GameData.Player.GetMobs();
                                        var mobsInRange = mobs.Count(x => x.Exists && Vector2D.CalculateDistance(GameData.Player.PosX, GameData.Player.PosY, x.PosX, x.PosY) < skill.Range);

                                        if (mobsInRange == 0 || mobsInRange > maxAOEMobsInRange || Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 0.5f)
                                            continue;
                                    }
                                    if (skill.IsAOE)
                                        GameData.Player.CastSpellOnMySelf(skill.Slot);
                                    else
                                        GameData.Player.CastSpellOnTarget(target.DBId, skill.Slot);
                                    Thread.Sleep(150); // wait for the packet to be handled by the backend
                                    break;
                                }
                            }
                            while (
                                (GameData.Player.CurrentAnimation == Models.Logic.Animation.ExecutingSkill || // while executing the skill
                                GameData.Player.CurrentAnimation == Models.Logic.Animation.PrepareExecutingSkill) &&
                                (GameData.Player.CurrentAnimation != Models.Logic.Animation.Stand || // and not standing or basic attacking
                                GameData.Player.CurrentAnimation != Models.Logic.Animation.Attack)
                                )
                            {
                                Thread.Sleep(250);
                            }


                            if (!target.Exists)
                            {
                                GameData.Player.Targets.Remove(GameData.Player.Targets[i]);
                                i--;
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex) { }
        }

        public void PartyMode()
        {
            try
            {
                bool looting = false;
                var startingPointX = GameData.Player.PosX;
                var startingPointY = GameData.Player.PosY;
                while (true)
                {
                    if (looting)
                    {
                        Loot(1000);
                    }
                    // run back to starting point if too far away from it
                    if (Vector2D.CalculateDistance(startingPointX, startingPointY, GameData.Player.PosX, GameData.Player.PosY) >= 500)
                    {
                        MoveToCoordinate(startingPointX, startingPointY, true, true);
                    }

                    // rest hp if too low
                    //if (Convert.ToSingle(GameData.Player.HP) / Convert.ToSingle(GameData.Player.MAXHP) < 0.5f && GameData.Player.Targets.Count == 0)
                    //{
                    //    RestHp(70);
                    //}



                    // basic attack closest target if exists
                    List<NpcEntity> mobs = new List<NpcEntity>();
                    if (GameData.Player.CurrentAnimation == Animation.Stand || GameData.Player.CurrentAnimation == Animation.Sit) // When standing - just attack
                    {
                        mobs = GameData.Player.GetMobs();
                        var target = mobs.FirstOrDefault(x => x.Exists && Vector2D.CalculateDistance(GameData.Player.PosX, GameData.Player.PosY, x.PosX, x.PosY) < 500);
                        if (target != null)
                        {
                            GameData.Player.TargetId = target.Id;
                            GameData.Player.AttackTarget(target.DBId);
                            Thread.Sleep(300);
                        }
                    }

                    // kill targets in range
                    mobs = GameData.Player.GetMobs();
                    foreach (var skill in GameData.Player.Skills)
                    {
                        if (skill.Enabled && skill.ManaCost < GameData.Player.MP && !skill.IsOnCooldown && skill.IsAOE)
                        {
                            var mobsInRange = mobs.Count(x => x.Exists && Vector2D.CalculateDistance(GameData.Player.PosX, GameData.Player.PosY, x.PosX, x.PosY) < skill.Range);
                            if (mobsInRange > 0)
                                GameData.Player.CastSpellOnMySelf(skill.Slot);
                            Thread.Sleep(150);
                            break;
                        }
                    }
                    while (
                        (GameData.Player.CurrentAnimation == Models.Logic.Animation.ExecutingSkill || // while executing the skill
                        GameData.Player.CurrentAnimation == Models.Logic.Animation.PrepareExecutingSkill) &&
                        (GameData.Player.CurrentAnimation != Models.Logic.Animation.Stand || // and not standing or basic attacking
                        GameData.Player.CurrentAnimation != Models.Logic.Animation.Attack)
                        )
                    {
                        Thread.Sleep(250);
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex) {
            }
        }

    }
}
