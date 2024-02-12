using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TrueEye.NPCs
{
    public class Boss : ModNPC
    {
        private Player player;
        private float speed;
        private int lifeDecrease = -50;
        private bool displayedText = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 3;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 10000;
            NPC.damage = 200;
            NPC.defense = 25;
            NPC.knockBackResist = 0f;
            NPC.width = 38;
            NPC.height = 30;
            NPC.value = 10000;
            NPC.npcSlots = 1f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            Music = MusicID.Boss1;
            NPC.BossBar = ModContent.GetInstance<BossBar.BossBar>();
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 1.625f * bossAdjustment);
            NPC.damage = (int)(NPC.damage * 1.6f);
            NPC.defense = (int)(NPC.defense + numPlayers);
        }

        public override void AI()
        {
            Target();
            DespawnHandler();
            Move(new Vector2(0, -100f));

            NPC.ai[1] -= 1f;
            if (NPC.ai[1] <= 0f)
            {
                Shoot();
            }

            if (NPC.life <= 6000)
            {
                speed = 6f;
                NPC.ai[2] -= 1f;
                if (NPC.ai[2] <= 0f)
                {
                    Teleportation();
                }
            }

            if (NPC.life <= 3000)
            {
                speed = 7f;
                NPC.ai[3] -= 1f;
                if (NPC.ai[3] <= 0f)
                {
                    LastStage();
                }
            }

            if (NPC.life <= 50)
            {
                if (displayedText == false)
                {
                    Main.NewText("Kill me", Color.Red);
                }
                displayedText = true;
                lifeDecrease = 0;
                NPC.life = 1;
                speed = 0;
                NPC.immortal = false;
            }
        }

        private void Target()
        {
            player = Main.player[NPC.target];
        }

        private void Move(Vector2 offset)
        {
            if (NPC.life > 5000)
            {
                speed = 4f;
            }

            Vector2 moveTo = player.Center;
            Vector2 move = moveTo - NPC.Center;
            float magnitude = Magnitude(move);

            if (magnitude > speed)
            {
                move *= speed / magnitude;
            }

            float turnResistance = 10f;
            move = (NPC.velocity * turnResistance + move) / (turnResistance + 1f);
            magnitude = Magnitude(move);

            if (magnitude > speed)
            {
                move *= speed / magnitude;
            }

            NPC.velocity = move;
        }

        private float Magnitude(Vector2 mag)
        {
            return (float)Math.Sqrt(mag.X * mag.X + mag.Y * mag.Y);
        }

        private void DespawnHandler()
        {
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];

                if (!player.active || player.dead)
                {
                    NPC.velocity = new Vector2(0f, -10f);
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                    return;
                }
            }
        }

        private void Shoot()
        {
            int type = ProjectileID.MartianTurretBolt;
            Vector2 velocity = player.Center - NPC.Center;
            float magnitude = Magnitude(velocity);

            if (magnitude > 0)
            {
                velocity *= new Vector2(5f, 5f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(new EntitySource_Parent(NPC), NPC.Center + new Vector2(50, 50), velocity, type, NPC.damage, 2f);
                Projectile.NewProjectile(new EntitySource_Parent(NPC), NPC.Center + new Vector2(-50, 50), velocity, type, NPC.damage, 2f);
            }

            NPC.ai[1] = 50f;
        }

        private void Teleportation()
        {
            Vector2 toPlayer = player.Center - NPC.Center;

            if (toPlayer.Length() > 50)
            {
                if (toPlayer.X > 50)
                {
                    NPC.Center = new Vector2(player.Center.X + 100, player.Center.Y);
                }
                else if (toPlayer.X < -50)
                {
                    NPC.Center = new Vector2(player.Center.X - 100, player.Center.Y);
                }
            }
            NPC.ai[2] = 50f;
        }

        private void LastStage()
        {
            NPC.immortal = true;
            NPC.life += lifeDecrease;
            int minionType = NPCID.Corruptor;
            Vector2 spawnPosition = NPC.Center + new Vector2(50, 0);
            NPC.NewNPC(new EntitySource_Parent(NPC), (int)spawnPosition.X, (int)spawnPosition.Y, minionType);
            NPC.ai[3] = 30f;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1.0f;
            NPC.frameCounter %= 20;
            int frame = (int)(NPC.frameCounter / 2.0);

            if (frame >= Main.npcFrameCount[NPC.type]) frame = 0;

            NPC.frame.Y = frame * frameHeight;
            RotateNPCToTarget();
        }

        public void RotateNPCToTarget()
        {
            if (player == null) return;

            Vector2 direction = NPC.Center - player.Center;
            float rotation = (float)Math.Atan2(direction.Y, direction.X);
            NPC.rotation = rotation + ((float)Math.PI * 0.5f);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.5f;
            return null;
        }
    }
}
