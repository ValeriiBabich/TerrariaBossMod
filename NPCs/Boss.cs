using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.UI.BigProgressBar;
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
            //DisplayName.SetDefault("Boss");
            // Встановлюємо кількість кадрів анімації для типу NPC на значення 3
            Main.npcFrameCount[NPC.type] = 3;
        }

        // Параметри нашого босу, зокрема такі як запас здоров'я, захист, імунітети, шкода
        public override void SetDefaults()
        {
            // Встановлюємо стиль штучного інтелекту (AI) для NPC на -1 (власна реалізація).
            NPC.aiStyle = -1;

            // Встановлюємо максимальне значення здоров'я NPC на 10000.
            NPC.lifeMax = 10000;

            // Встановлюємо значення пошкодження, яке наносить NPC, на 200.
            NPC.damage = 200;

            // Встановлюємо значення захисту NPC на рівень 25.
            NPC.defense = 25;

            // Встановлюємо стійкість NPC до відкидання на 0 (немає стійкості до відкидання).
            NPC.knockBackResist = 0f;

            // Встановлюємо ширину NPC на 38 пікселів.
            NPC.width = 38;

            // Встановлюємо висоту NPC на 30 пікселів.
            NPC.height = 30;

            // Встановлюємо вартість NPC на 10000 монет.
            NPC.value = 10000;

            // Встановлюємо кількість слотів NPC для спавну ворогів на 1.
            NPC.npcSlots = 1f;

            // Помічаємо NPC як боса.
            NPC.boss = true;

            // Делаемо NPC стійким до лави.
            NPC.lavaImmune = true;

            // Вимикаємо гравітацію для NPC.
            NPC.noGravity = true;

            // Вимикаємо колізію NPC з блоками на карті.
            NPC.noTileCollide = true;

            // Звук, який відтворюється при ударі по NPC.
            NPC.HitSound = SoundID.NPCHit1;

            // Звук, який відтворюється при смерті NPC.
            NPC.DeathSound = SoundID.NPCDeath1;

            // Музика, що грається під час битви з цим NPC, в даному випадку - Boss1.
            Music = MusicID.Boss1;

            NPC.BossBar = ModContent.GetInstance<BossBar.BossBar>();

        }


        // Перевизначаємо метод ApplyDifficultyAndPlayerScaling з базового класу для налаштування параметрів NPC в залежності від складності та кількості гравців.
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            // Збільшуємо максимальне значення здоров'я NPC, використовуючи коефіцієнти та коригуючий коефіцієнт для балансу боса.
            NPC.lifeMax = (int)(NPC.lifeMax * 1.625f * bossAdjustment);

            // Збільшуємо значення пошкодження, яке наносить NPC, використовуючи коефіцієнт.
            NPC.damage = (int)(NPC.damage * 1.6f);

            // Збільшуємо захист NPC на основі кількості гравців у грі.
            NPC.defense = (int)(NPC.defense + numPlayers);
        }



        // Мозок нашого босу, а точніше його поведінка
        public override void AI()
        {

            Target();
            // Викликаємо метод DespawnHandler() для обробки логіки зникнення NPC.
            DespawnHandler();

            // Рух
            Move(new Vector2(0, -100f));
           

            // Зменшуємо лічильник до наступного пострілу або атаки NPC.
            NPC.ai[1] -= 1f;

            // Перевіряємо, чи лічильник досягнув нуля, і якщо так, викликаємо метод Shoot() для виконання атаки.
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



        // Пошук цілі( в нашому випадку пошук гравця)
        private void Target()
        {
            player = Main.player[NPC.target];
        }


        // Рух боса
        // Метод Move, відповідальний за переміщення NPC з використанням вектора зміщення offset.
        private void Move(Vector2 offset)
        {
            // Встановлюємо швидкість NPC на 4f.
            if (NPC.life > 5000)
            {
                speed = 4f;
            }
            

            // Визначаємо точку, до якої ми хочемо перемістити NPC - центр гравця.
            Vector2 moveTo = player.Center;

            // Визначаємо вектор напрямку руху від поточного положення NPC до точки moveTo.
            Vector2 move = moveTo - NPC.Center;

            // Визначаємо величину вектора move.
            float magnitude = Magnitude(move);

            // Перевіряємо, чи величина вектора більше за швидкість NPC.
            if (magnitude > speed)
            {
                // Зменшуємо вектор move, якщо величина його більше за швидкість, для вирівнювання швидкості NPC.
                move *= speed / magnitude;
            }

            // Визначаємо коефіцієнт опору повороту для зрівнювання руху NPC.
            float turnResistance = 10f;

            // Застосовуємо опір повороту, щоб зрівняти швидкість NPC з поточною швидкістю.
            move = (NPC.velocity * turnResistance + move) / (turnResistance + 1f);

            // Знову визначаємо величину вектора move.
            magnitude = Magnitude(move);

            // Перевіряємо, чи величина вектора більше за швидкість NPC.
            if (magnitude > speed)
            {
                // Знову зменшуємо вектор move, якщо величина його більше за швидкість, для забезпечення сталої швидкості NPC.
                move *= speed / magnitude;
            }

            // Задаємо нову швидкість NPC вектором move.
            NPC.velocity = move;
        }


        private float Magnitude(Vector2 mag)
        {
            // Використовуємо формулу для обчислення довжини вектора: sqrt(x^2 + y^2).
            return (float)Math.Sqrt(mag.X * mag.X + mag.Y * mag.Y);
        }

        // Зникнення босу після смерті гравця
        private void DespawnHandler()
        {
            // Перевіряємо, чи гравець неактивний або мертвий.
            if (!player.active || player.dead)
            {
                // Задаємо NPC нову ціль - найближчого активного гравця.
                NPC.TargetClosest(false);

                // Оновлюємо посилання на гравця після зміни цілі NPC.
                player = Main.player[NPC.target];

                // Перевіряємо, чи новий гравець також неактивний або мертвий.
                if (!player.active || player.dead)
                {
                    // Зупиняємо рух NPC, направляючи його вгору на -10f.
                    NPC.velocity = new Vector2(0f, -10f);

                    // Обмежуємо час життя NPC, якщо він більше за 10.
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }

                    // Завершуємо виконання методу, оскільки NPC вже оброблений при зникненні гравця.
                    return;
                }
            }
        }



        // Атаки дального бою для боса
        private void Shoot()
        {
            // Визначаємо тип снаряду, в даному випадку - FireArrow.
            int type = ProjectileID.MartianTurretBolt;
            

            // Визначаємо вектор швидкості снаряду, направленого в сторону центру гравця.
            Vector2 velocity = player.Center - NPC.Center;

            // Обчислюємо величину вектора швидкості.
            float magnitude = Magnitude(velocity);

            // Перевіряємо, чи величина вектора більше 0, щоб уникнути ділення на нуль.
            if (magnitude > 0)
            {
                velocity *= new Vector2(5f, 5f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(new EntitySource_Parent(NPC), NPC.Center + new Vector2(50, 50), velocity, type, NPC.damage, 2f);
                Projectile.NewProjectile(new EntitySource_Parent(NPC), NPC.Center + new Vector2(-50, 50), velocity, type, NPC.damage, 2f);
            }

            // Встановлюємо лічильник часу до наступної атаки на значення 50f.
            NPC.ai[1] = 50f;
        }


        private void Teleportation()
        {
            Vector2 toPlayer = player.Center - NPC.Center;

            // Перевіряємо, чи відстань між NPC і гравцем більше 50 пікселів
            if (toPlayer.Length() > 50)
            {
                // Телепортуємо NPC тільки якщо гравець достатньо далеко
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



        // Перевизначаємо метод FindFrame з базового класу для визначення кадру анімації NPC.
        public override void FindFrame(int frameHeight)
        {
            // Збільшуємо лічильник кадрів NPC на 1.
            NPC.frameCounter += 1.0f;

            // Зациклюємо лічильник кадрів, якщо він перевищує значення 20.
            NPC.frameCounter %= 20;

            // Визначаємо номер кадру для анімації на основі лічильника кадрів.
            int frame = (int)(NPC.frameCounter / 2.0);

            // Перевіряємо, чи номер кадру не перевищує загальну кількість кадрів для даного типу NPC.
            if (frame >= Main.npcFrameCount[NPC.type]) frame = 0;

            // Встановлюємо властивість Y кадру NPC для відображення вірного кадру анімації.
            NPC.frame.Y = frame * frameHeight;

            // Викликаємо метод RotateNPCToTarget для оновлення орієнтації NPC до цілі.
            RotateNPCToTarget();
        }


        // Напрям руху боса на гравця
        public void RotateNPCToTarget()
        {
            // Перевіряємо, чи гравець існує (не дорівнює null).
            if (player == null) return;

            // Визначаємо вектор напрямку від центру NPC до центру гравця.
            Vector2 direction = NPC.Center - player.Center;

            // Обчислюємо кут обертання NPC, використовуючи арктангенс2 для отримання кута між векторами.
            float rotation = (float)Math.Atan2(direction.Y, direction.X);

            // Встановлюємо властивість rotation NPC, обернуту на 90 градусів (π/2 радіан), щоб правильно відрізняти напрямок.
            NPC.rotation = rotation + ((float)Math.PI * 0.5f);
        }


        // Відображення шкали здоров'я
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.5f;
            return null;
        }


    }
}
