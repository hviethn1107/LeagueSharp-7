﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Kindred___YinYang
{
    class Program
    {
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;
        private static Obj_AI_Hero Kindred = ObjectManager.Player;
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;

        public static string[] highChamps =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
                "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
                "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
                "Zed", "Ziggs","Kindred"
            };

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Kindred.ChampionName != "Kindred")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 340);
            W = new Spell(SpellSlot.W, 593);
            E = new Spell(SpellSlot.E, 640);
            R = new Spell(SpellSlot.R, 1100);

            Config = new Menu("Kindred - Yin Yang", "Kindred - Yin Yang", true);
            TargetSelector.AddToMenu(Config.SubMenu("Target Selector Settings"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));
            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.AddItem(new MenuItem("q.combo.style", "Q Style").SetValue(new StringList(new[] {"Kite", "100% Hit [BETA]" })));
                comboMenu.AddItem(new MenuItem("q.combo", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("w.combo", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("e.combo", "Use E").SetValue(true));
                Config.AddSubMenu(comboMenu);
            }
            var eMenu = new Menu("E Settings", "E Settings");
            {
                eMenu.AddItem(new MenuItem("e.whte", "                     E Whitelist"));
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsEnemy))
                {
                    eMenu.AddItem(new MenuItem("enemy." + enemy.CharData.BaseSkinName, string.Format("E: {0}", enemy.CharData.BaseSkinName)).SetValue(highChamps.Contains(enemy.CharData.BaseSkinName)));

                }
                Config.AddSubMenu(eMenu);
            }
            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.AddItem(new MenuItem("q.harass", "Use Q").SetValue(true));
                harassMenu.AddItem(new MenuItem("w.harass", "Use W").SetValue(true));
                harassMenu.AddItem(new MenuItem("e.harass", "Use E").SetValue(true));
                harassMenu.AddItem(new MenuItem("harass.mana", "Mana Manager").SetValue(new Slider(20, 1, 99)));
                Config.AddSubMenu(harassMenu);
            }
            var laneClear = new Menu("Clear Settings", "Clear Settings");
            {
                laneClear.AddItem(new MenuItem("q.clear", "Use Q").SetValue(true));
                laneClear.AddItem(new MenuItem("q.minion.count", "Q Minion Count").SetValue(new Slider(4, 1, 5)));
                laneClear.AddItem(new MenuItem("clear.mana", "Mana Manager").SetValue(new Slider(20, 1, 99)));
                Config.AddSubMenu(laneClear);
            }
            var jungleClear = new Menu("Jungle Settings", "Jungle Settings");
            {
                jungleClear.AddItem(new MenuItem("q.jungle", "Use Q").SetValue(true));
                jungleClear.AddItem(new MenuItem("w.jungle", "Use W").SetValue(true));
                jungleClear.AddItem(new MenuItem("e.jungle", "Use E").SetValue(true));
                jungleClear.AddItem(new MenuItem("jungle.mana", "Mana Manager").SetValue(new Slider(20, 1, 99)));
                Config.AddSubMenu(jungleClear);
            }
            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                drawMenu.AddItem(new MenuItem("q.draw", "Q Range").SetValue(new Circle(true, Color.White)));
                drawMenu.AddItem(new MenuItem("w.draw", "W Range").SetValue(new Circle(true, Color.Gold)));
                drawMenu.AddItem(new MenuItem("e.draw", "E Range").SetValue(new Circle(true, Color.DodgerBlue)));
                drawMenu.AddItem(new MenuItem("r.draw", "R Range").SetValue(new Circle(true, Color.GreenYellow)));
                Config.AddSubMenu(drawMenu);
            }
            Config.AddItem(new MenuItem("e.method", "E Method").SetValue(new StringList(new[] { "Cursor Position" })));
            Config.AddItem(new MenuItem("use.r", "Use R").SetValue(true));
            Config.AddItem(new MenuItem("r.whte", "                          R Whitelist"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsAlly))
            {
                Config.AddItem(new MenuItem("respite." + ally.CharData.BaseSkinName, string.Format("Respite: {0}", ally.CharData.BaseSkinName)).SetValue(highChamps.Contains(ally.CharData.BaseSkinName)));

            }
            Config.AddItem(new MenuItem("min.hp.for.r", "Min. HP Percent for R").SetValue(new Slider(20, 1, 99)));
            Config.AddToMainMenu();
            Game.PrintChat("<font color='#ff3232'>Kindred - Yin Yang: </font> <font color='#d4d4d4'>If you like this assembly feel free to upvote on Assembly DB</font>");
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Config.Item("q.combo").GetValue<bool>() && unit is Obj_AI_Hero && target is Obj_AI_Hero &&
                        Kindred.Distance(target.Position) < Kindred.AttackRange)
            {
                Q.Cast(Game.CursorPos);
                Utility.DelayAction.Add(300, Orbwalking.ResetAutoAttackTimer);
            }      
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    Jungle();
                    break;
            }
            if (Config.Item("use.r").GetValue<bool>() && R.IsReady())
            {
                RLogic();
            }
        }

        private static void CollisionObjectCheckCast(Spell spell, Obj_AI_Hero unit, int count)
        {
            if (spell.GetPrediction(unit).CollisionObjects.Count <= count)
            {
                spell.Cast(Game.CursorPos);
            }        
        }

        private static void QStyleCast(Spell spell, Obj_AI_Hero unit, int count)
        {
            switch (Config.Item("q.combo.style").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    spell.Cast(Game.CursorPos);
                    break;
                case 1:
                    CollisionObjectCheckCast(spell,unit,count);
                    break;
            }
        }
        private static void Combo()
        {
            var useQ = Config.Item("q.combo").GetValue<bool>();
            var useW = Config.Item("w.combo").GetValue<bool>();
            var useE = Config.Item("e.combo").GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o=> o.IsValidTarget(ObjectManager.Player.AttackRange) && !o.IsDead && !o.IsZombie))
                {
                    QStyleCast(Q,enemy,2);
                }
            }
            if (useW && W.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(W.Range) && !o.IsDead && !o.IsZombie))
                {
                    W.Cast();
                } 
            }
            if (useE && E.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie))
                {
                    if (Config.Item("enemy." + enemy.CharData.BaseSkinName).GetValue<bool>())
                    {
                        E.Cast(enemy);
                    }
                } 
            }
        }
        private static void Harass()
        {
            var useQ = Config.Item("q.combo").GetValue<bool>();
            var useW = Config.Item("w.combo").GetValue<bool>();
            var useE = Config.Item("e.combo").GetValue<bool>();
            var harassMana = Config.Item("harass.mana").GetValue<Slider>().Value;

            if (Kindred.ManaPercent > harassMana)
            {
                if (useQ && Q.IsReady())
                {
                    foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(ObjectManager.Player.AttackRange) && !o.IsDead && !o.IsZombie))
                    {
                        Q.Cast(Game.CursorPos);
                    }
                }
                if (useW && W.IsReady())
                {
                    foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(W.Range) && !o.IsDead && !o.IsZombie))
                    {
                        W.Cast();
                    }
                }
                if (useE && E.IsReady())
                {
                    foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie))
                    {
                        if (Config.Item("enemy." + enemy.CharData.BaseSkinName).GetValue<bool>())
                        {
                            E.Cast(enemy);
                        }
                    }
                }
            }
        }
        private static void RLogic()
        {
            var minHP = Config.Item("min.hp.for.r").GetValue<Slider>().Value;
            foreach (var ally in HeroManager.Allies.Where(o=> o.HealthPercent < minHP && !o.IsRecalling() && !o.IsDead && !o.IsZombie
                && Kindred.Distance(o.Position) < R.Range))
            {
                if (Config.Item("respite."+ally.CharData.BaseSkinName).GetValue<bool>())
                {
                    R.Cast(ally);
                }
            }
        }
        private static void Clear()
        {
            var xMinion = MinionManager.GetMinions(Kindred.ServerPosition,Kindred.AttackRange, MinionTypes.All, MinionTeam.Enemy);
            var useQ = Config.Item("q.clear").GetValue<bool>();
            var manaClear = Config.Item("clear.mana").GetValue<Slider>().Value;
            var minCount = Config.Item("q.minion.count").GetValue<Slider>().Value;
            if (Kindred.ManaPercent >= manaClear)
            {
                if (useQ && Q.IsReady() && xMinion.Count >= minCount)
                {
                    Q.Cast(Game.CursorPos);
                }
            }
        }
        private static void Jungle()
        {
            var useQ = Config.Item("q.jungle").GetValue<bool>();
            var useW = Config.Item("w.jungle").GetValue<bool>();
            var useE = Config.Item("e.jungle").GetValue<bool>();
            var manaSlider = Config.Item("jungle.mana").GetValue<Slider>().Value;
            var mob = MinionManager.GetMinions(Kindred.ServerPosition, Orbwalking.GetRealAutoAttackRange(Kindred) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mob == null || (mob != null && mob.Count == 0))
            {
                return;
            }

            if (Kindred.ManaPercent > manaSlider)
            {
                if (Q.IsReady() && useQ)
                {
                    Q.Cast(Game.CursorPos);
                }
                if (W.IsReady() && useW)
                {
                    W.Cast();
                }
                if (E.IsReady() && useE)
                {
                    E.Cast(mob[0]);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem1 = Config.Item("q.draw").GetValue<Circle>();
            var menuItem2 = Config.Item("w.draw").GetValue<Circle>();
            var menuItem3 = Config.Item("e.draw").GetValue<Circle>();
            var menuItem4 = Config.Item("r.draw").GetValue<Circle>();

            if (menuItem1.Active && Q.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Kindred.Position.X, Kindred.Position.Y, Kindred.Position.Z), Q.Range, menuItem1.Color, 5);
            }
            if (menuItem2.Active && W.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Kindred.Position.X, Kindred.Position.Y, Kindred.Position.Z), W.Range, menuItem2.Color, 5);
            }
            if (menuItem3.Active && E.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Kindred.Position.X, Kindred.Position.Y, Kindred.Position.Z), E.Range, menuItem3.Color, 5);
            }
            if (menuItem4.Active && R.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Kindred.Position.X, Kindred.Position.Y, Kindred.Position.Z), R.Range, menuItem4.Color, 5);
            }
        }
    }
}
