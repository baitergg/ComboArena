using ComboArena.Core;
using ComboArena.Model;
using ComboArena.Model.Abilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ComboArena.View
{
    /// <summary>
    /// Отвечает за отрисовку всех игровых объектов и интерфейса.
    /// Содержит методы для рисования игрока, врагов, дропа,
    /// эффектов способностей, лазеров, UI-панелей и экрана выбора перков.
    /// </summary>
    public class GameView
    {
        private readonly Player _player;

        private readonly List<Enemy> _enemies;

        private readonly List<DropItem> _dropItems;

        private readonly List<Perk> _offeredPerks;

        private bool _isChoosingPerk;

        private PlayerView _playerView;

        private Dictionary<EnemyType, EnemyView> _enemyViews;

        private SpriteFont _font;

        private Texture2D _pixelTexture;

        private Texture2D _expTexture;

        private Texture2D _heartTexture;

        private Texture2D _attackTextureRight;

        private Texture2D _attackTextureLeft;

        // Таймеры для визуальных эффектов
        private float _healthFlashTimer;
        private const float HealthFlashDuration = 0.3f;

        private float _damageFlashTimer;
        private const float DamageFlashDuration = 1f;

        private float _comboPulseTimer;
        private const float ComboPulseDuration = 0.2f;
        private int _lastComboCount;

        private float _levelUpTimer;
        private const float LevelUpDuration = 2f;
        
        public GameView(Player player, List<Enemy> enemies, List<DropItem> dropItems, List<Perk> offeredPerks)
        {
            _player = player;
            _enemies = enemies;
            _dropItems = dropItems;
            _offeredPerks = offeredPerks;
            _enemyViews = new Dictionary<EnemyType, EnemyView>();

            // Подписки на события
            EventBus.Instance.Subscribe("PerkSelection", OnPerkSelection);
            EventBus.Instance.Subscribe("HealthChanged", OnHealthChanged);
            EventBus.Instance.Subscribe("Combo", OnComboChanged);
            EventBus.Instance.Subscribe("LevelUp", OnLevelUp);
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font = null, ContentManager content = null)
        {
            _playerView = new PlayerView();
            _playerView.LoadContent(graphicsDevice, content);

            foreach (var type in Enum.GetValues(typeof(EnemyType)))
            {
                var view = new EnemyView((EnemyType)type);
                view.LoadContent(graphicsDevice, content);
                _enemyViews[(EnemyType)type] = view;
            }

            _font = font;
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            _expTexture = content.Load<Texture2D>("exp");
            _heartTexture = content.Load<Texture2D>("heart");
            _attackTextureRight = content.Load<Texture2D>("attack_right");
            _attackTextureLeft = content.Load<Texture2D>("attack_left");
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_playerView == null || _player == null)
                return;

            // Обновление таймеров визуальных эффектов
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_healthFlashTimer > 0) _healthFlashTimer -= delta;
            if (_damageFlashTimer > 0) _damageFlashTimer -= delta;
            if (_comboPulseTimer > 0) _comboPulseTimer -= delta;
            if (_levelUpTimer > 0) _levelUpTimer -= delta;

            _playerView.Draw(spriteBatch, _player);

            DrawLaserAttacks(spriteBatch);

            foreach (var enemy in _enemies)
            {
                if (_enemyViews.TryGetValue(enemy.Type, out var view))
                {
                    view.Draw(spriteBatch, enemy);
                }
            }

            if (_player.IsAttacking)
            {
                var attackBounds = _player.GetAttackBounds();
                var attackTexture = _player.FacingDirection.X > 0
                    ? _attackTextureRight
                    : _attackTextureLeft;
                spriteBatch.Draw(attackTexture, attackBounds, Color.Orange * 0.7f);
            }

            DrawDrops(spriteBatch, gameTime);
            DrawAbilityEffects(spriteBatch);
        }

        private void DrawLaserAttacks(SpriteBatch spriteBatch)
        {
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsAlive || !enemy.IsShootingLaser) continue;

                var enemyCenter = enemy.Position + new Vector2(enemy.Width / 2, enemy.Height / 2);
                var laserEnd = enemyCenter + enemy.LaserDirection * enemy.LaserRange;

                var startPoint = new Vector2((int)enemyCenter.X, (int)enemyCenter.Y);
                var endPoint = new Vector2((int)laserEnd.X, (int)laserEnd.Y);

                DrawLaserBeam(spriteBatch, startPoint, endPoint, enemy.LaserAnimationTimer);
            }
        }

        private void DrawLaserBeam(SpriteBatch spriteBatch, Vector2 start, Vector2 end, float animationTimer)
        {
            var direction = end - start;
            var length = direction.Length();
            if (length < 1) return;
            direction.Normalize();

            var thickness = 3f + (float)Math.Sin(animationTimer * 20) * 1.5f;

            // Внешнее свечение
            var glowColor = Color.Yellow * (0.3f + animationTimer * 0.5f);
            DrawLine(spriteBatch, start, end, thickness + 4, glowColor);

            // Основной луч
            var beamColor = Color.Lerp(Color.Orange, Color.Red, animationTimer * 2f) * 0.9f;
            DrawLine(spriteBatch, start, end, thickness, beamColor);

            // Яркая сердцевина
            var coreColor = Color.White * (0.5f + animationTimer * 0.3f);
            DrawLine(spriteBatch, start, end, thickness * 0.3f, coreColor);
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, float thickness, Color color)
        {
            var direction = end - start;
            var length = direction.Length();
            if (length < 1) return;
            direction.Normalize();

            var perpendicular = new Vector2(-direction.Y, direction.X);
            var corner = perpendicular * (thickness / 2f);

            var rect = new Rectangle(
                (int)(start.X + corner.X),
                (int)(start.Y + corner.Y),
                (int)length,
                (int)thickness);

            var rotation = (float)Math.Atan2(direction.Y, direction.X);

            spriteBatch.Draw(_pixelTexture, rect, null, color, rotation,
                new Vector2(0, 0.5f), SpriteEffects.None, 0);
        }
        
        private void DrawAbilityEffects(SpriteBatch spriteBatch)
        {
            if (_player.Ability == null) return;

            var playerCenter = _player.Position + new Vector2(_player.Width / 2, _player.Height / 2);

            // Эффект огненного взрыва
            if (_player.Ability is FireBurst fireBurst && fireBurst.AnimationTimer > 0)
            {
                var progress = 1f - (fireBurst.AnimationTimer / 0.4f);
                var currentRadius = fireBurst.Radius * progress;

                DrawBurstRing(spriteBatch, playerCenter, currentRadius + 10, 8f,
                    Color.Orange * (0.3f * (1f - progress)));

                DrawBurstRing(spriteBatch, playerCenter, currentRadius, 6f,
                    Color.Lerp(Color.Yellow, Color.Red, progress) * (0.8f * (1f - progress)));

                DrawBurstRing(spriteBatch, playerCenter, currentRadius * 0.5f, 4f,
                    Color.White * (0.6f * (1f - progress)));
            }

            // Эффект щита
            if (_player.Ability is Barrier barrier && barrier.IsBarrierActive)
            {
                var pulse = 0.8f + (float)Math.Sin(barrier.Timer * 15) * 0.2f;
                var shieldRadius = Math.Max(_player.Width, _player.Height) * 0.7f;

                DrawBurstRing(spriteBatch, playerCenter, shieldRadius + 8, 5f,
                    Color.Cyan * (0.2f * pulse));

                DrawBurstRing(spriteBatch, playerCenter, shieldRadius, 3f,
                    Color.Lerp(Color.Cyan, Color.Blue, pulse) * (0.5f * pulse));

                DrawBurstRing(spriteBatch, playerCenter, shieldRadius * 0.7f, 2f,
                    Color.White * (0.3f * pulse));
            }
        }

        private void DrawBurstRing(SpriteBatch spriteBatch, Vector2 center, float radius, float thickness, Color color)
        {
            const int segments = 32;
            const double step = Math.PI * 2 / segments;

            for (var i = 0; i < segments; i++)
            {
                var angle1 = i * step;
                var angle2 = (i + 1) * step;

                var x1 = center.X + (float)Math.Cos(angle1) * radius;
                var y1 = center.Y + (float)Math.Sin(angle1) * radius;
                var x2 = center.X + (float)Math.Cos(angle2) * radius;
                var y2 = center.Y + (float)Math.Sin(angle2) * radius;

                DrawLine(spriteBatch,
                    new Vector2(x1, y1),
                    new Vector2(x2, y2),
                    thickness, color);
            }
        }

        private void DrawDrops(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var time = (float)gameTime.TotalGameTime.TotalSeconds;

            foreach (var drop in _dropItems)
            {
                var rect = new Rectangle(
                    (int)drop.Position.X,
                    (int)drop.Position.Y,
                    (int)DropItem.Width,
                    (int)DropItem.Height);

                var pulse = (float)(Math.Sin(time * 5 + drop.Position.X * 0.1f) * 0.2 + 0.8);
                var pulseColor = Color.White * pulse;

                var texture = drop.Type == DropType.Experience ? _expTexture : _heartTexture;
                spriteBatch.Draw(texture, rect, pulseColor);
            }
        }

        public void DrawUI(SpriteBatch spriteBatch)
        {
            if (_font == null) return;

            // Красная вспышка на весь экран при получении урона
            if (_damageFlashTimer > 0)
            {
                var flashAlpha = (_damageFlashTimer / DamageFlashDuration) * 0.3f;
                var vp = spriteBatch.GraphicsDevice.Viewport;
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(0, 0, vp.Width, vp.Height),
                    Color.Red * flashAlpha);
            }

            if (_isChoosingPerk)
            {
                DrawPerkSelection(spriteBatch);
                return;
            }

            var viewport = spriteBatch.GraphicsDevice.Viewport;

            // Расчёт высоты фона HUD в зависимости от контента
            const int baseHeight = 115; // HP (35) + Level (20) + XP (20) + XP bar (10) + отступ
            var comboHeight = _player.ComboCount > 0 ? 75 : 0; // Комбо-секция
            var perkCount = Math.Min(_player.ActivePerks.Count, 5);
            var perksHeight = perkCount > 0 ? 20 + perkCount * 18 + 10 : 0; // Заголовок + перки + отступ
            var totalHeight = baseHeight + comboHeight + perksHeight;

            // Фон панели HUD
            var uiBackground = new Rectangle(5, 5, 400, totalHeight);
            spriteBatch.Draw(_pixelTexture, uiBackground, new Color(0, 0, 0, 150));

            // HP
            var hpColor = _healthFlashTimer > 0 ? Color.OrangeRed : Color.Red;
            DrawTextWithShadow(spriteBatch, $"HP: {_player.Health:F0}/{_player.MaxHealth:F0}",
                new Vector2(15, 15), hpColor);

            var hpProgress = _player.Health / _player.MaxHealth;
            DrawProgressBar(spriteBatch, new Vector2(15, 35), 200, 10,
                hpProgress, Color.Red, "Здоровье");

            // Вспышка при получении урона
            if (_healthFlashTimer > 0)
            {
                var flashAlpha = _healthFlashTimer / HealthFlashDuration;
                var flashRect = new Rectangle(13, 33, 204, 14);
                spriteBatch.Draw(_pixelTexture, flashRect, Color.Red * (flashAlpha * 0.5f));
            }

            // Уровень
            DrawTextWithShadow(spriteBatch, $"Уровень: {_player.Level}",
                new Vector2(15, 55), Color.Green);

            // Опыт
            DrawTextWithShadow(spriteBatch, $"Опыт: {_player.Experience:F0}/{_player.ExperienceToNextLevel:F0}",
                new Vector2(15, 75), Color.Blue);

            DrawProgressBar(spriteBatch, new Vector2(15, 95), 200, 10,
                _player.Experience / _player.ExperienceToNextLevel, Color.Blue, "Опыт");

            // Комбо
            if (_player.ComboCount > 0)
            {
                const int comboY = 120;
                var comboText = $"COMBO: {_player.ComboCount}x";
                var comboColor = Color.Lerp(Color.Yellow, Color.Red, _player.ComboCount / 50f);

                // Пульсация при увеличении комбо
                var comboScale = 1f;
                if (_comboPulseTimer > 0)
                {
                    comboScale = 1f + (_comboPulseTimer / ComboPulseDuration) * 0.3f;
                }

                if (comboScale > 1f)
                {
                    var textSize = _font.MeasureString(comboText);
                    var origin = new Vector2(textSize.X / 2, textSize.Y / 2);
                    var position = new Vector2(15 + textSize.X / 2, comboY + textSize.Y / 2);
                    spriteBatch.DrawString(_font, comboText, position + new Vector2(1, 1),
                        Color.Black * 0.5f, 0f, origin, comboScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(_font, comboText, position,
                        comboColor, 0f, origin, comboScale, SpriteEffects.None, 0);
                }
                else
                {
                    DrawTextWithShadow(spriteBatch, comboText, new Vector2(15, comboY), comboColor);
                }

                var bonusText = $"Урон: +{((_player.ComboDamageMultiplier - 1) * 100):F0}%  " +
                    $"Опыт: +{((_player.ComboExperienceMultiplier - 1) * 100):F0}%";
                DrawTextWithShadow(spriteBatch, bonusText, new Vector2(15, comboY + 20), Color.LightGreen);

                // Таймер комбо
                var timerBarPosition = new Vector2(15, comboY + 45);
                const int timerBarWidth = 200;
                const int timerBarHeight = 8;
                var timerFillWidth = (int)(timerBarWidth * (_player.ComboTimer / Player.ComboTimeout));

                spriteBatch.Draw(_pixelTexture,
                    new Rectangle((int)timerBarPosition.X, (int)timerBarPosition.Y, timerBarWidth, timerBarHeight),
                    new Color(60, 60, 60, 200));

                var timerColor = Color.Lerp(Color.Cyan, Color.Magenta, _player.ComboTimer / Player.ComboTimeout);
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle((int)timerBarPosition.X, (int)timerBarPosition.Y, timerFillWidth, timerBarHeight),
                    timerColor);

                spriteBatch.Draw(_pixelTexture,
                    new Rectangle((int)timerBarPosition.X - 1, (int)timerBarPosition.Y - 1,
                        timerBarWidth + 2, timerBarHeight + 2),
                    Color.Black * 0.5f);

                DrawTextWithShadow(spriteBatch, "Таймер комбо",
                    timerBarPosition + new Vector2(timerBarWidth + 10, -2), Color.White * 0.8f);
            }

            // Активные перки (максимум 5)
            var perkY = baseHeight + 5;
            if (_player.ComboCount > 0)
                perkY += comboHeight;

            if (perkCount > 0)
            {
                DrawTextWithShadow(spriteBatch, "Активные улучшения:", new Vector2(15, perkY), Color.Gold);
                perkY += 20;
                var shownPerks = 0;
                foreach (var perk in _player.ActivePerks)
                {
                    if (shownPerks >= 5) break;
                    DrawTextWithShadow(spriteBatch, $"- {perk.Name}", new Vector2(20, perkY), Color.Yellow);
                    perkY += 18;
                    shownPerks++;
                }
            }

            // Счётчик врагов
            var enemyCount = _enemies.Count;
            var enemyText = $"Враги: {enemyCount}";
            var enemyTextSize = _font.MeasureString(enemyText);
            var enemyTextPosition = new Vector2(viewport.Width - enemyTextSize.X - 20, 15);
            DrawTextWithShadow(spriteBatch, enemyText, enemyTextPosition, Color.Orange);

            // LEVEL UP! текст
            if (_levelUpTimer > 0)
            {
                var levelUpAlpha = Math.Min(_levelUpTimer / LevelUpDuration * 2f, 1f);
                var levelUpText = "НОВЫЙ УРОВЕНЬ!";
                var levelUpSize = _font.MeasureString(levelUpText);
                var levelUpPos = new Vector2(
                    (viewport.Width - levelUpSize.X) / 2,
                    viewport.Height * 0.3f);
                var levelUpColor = Color.Gold * levelUpAlpha;
                spriteBatch.DrawString(_font, levelUpText,
                    levelUpPos + new Vector2(2, 2), Color.Black * (levelUpAlpha * 0.5f));
                spriteBatch.DrawString(_font, levelUpText, levelUpPos, levelUpColor);
            }

            // UI способности
            DrawAbilityUI(spriteBatch, viewport);
        }

        private void DrawProgressBar(SpriteBatch spriteBatch, Vector2 position, int width, int height,
            float progress, Color color, string label = "")
        {
            var backgroundRect = new Rectangle((int)position.X, (int)position.Y, width, height);
            spriteBatch.Draw(_pixelTexture, backgroundRect, new Color(40, 40, 40, 200));

            var fillWidth = (int)(width * progress);
            if (fillWidth > 0)
            {
                var fillRect = new Rectangle((int)position.X, (int)position.Y, fillWidth, height);
                spriteBatch.Draw(_pixelTexture, fillRect, color);
            }

            var borderRect = new Rectangle((int)position.X - 1, (int)position.Y - 1, width + 2, height + 2);
            spriteBatch.Draw(_pixelTexture, borderRect, Color.Black * 0.5f);

            if (!string.IsNullOrEmpty(label))
            {
                var labelText = $"{label}: {(progress * 100):F0}%";
                DrawTextWithShadow(spriteBatch, labelText,
                    position + new Vector2(width + 10, -2), Color.White * 0.8f);
            }
        }

        private void DrawAbilityUI(SpriteBatch spriteBatch, Viewport viewport)
        {
            if (_player.Ability == null) return;

            var ability = _player.Ability;
            const int barWidth = 200;
            const int barHeight = 16;
            var x = viewport.Width - barWidth - 20;
            var y = viewport.Height - 60;

            // Фон панели
            var panelRect = new Rectangle(x - 10, y - 10, barWidth + 20, barHeight + 50);
            spriteBatch.Draw(_pixelTexture, panelRect, new Color(0, 0, 0, 150));

            // Название
            DrawTextWithShadow(spriteBatch, $"[E] {ability.Name}", new Vector2(x, y - 5), Color.Gold);

            // Полоска КД
            var cooldownProgress = ability.GetCooldownProgress();
            var barY = y + 20;

            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x, barY, barWidth, barHeight),
                new Color(40, 40, 40, 200));

            if (cooldownProgress > 0)
            {
                var fillWidth = (int)(barWidth * cooldownProgress);
                var fillColor = ability.IsReady ? Color.Lime : Color.Cyan;
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x, barY, fillWidth, barHeight),
                    fillColor * 0.8f);
            }

            // Рамка
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x - 1, barY - 1, barWidth + 2, barHeight + 2),
                Color.White * 0.3f);

            // Статус
            var statusText = ability.IsReady ? "ГОТОВО" : $"Перезарядка: {ability.CurrentCooldown:F1}с";
            var statusColor = ability.IsReady ? Color.Lime : Color.White * 0.7f;
            DrawTextWithShadow(spriteBatch, statusText, new Vector2(x, barY + barHeight + 2), statusColor);
        }

        private void DrawPerkSelection(SpriteBatch spriteBatch)
        {
            if (!_isChoosingPerk) return;

            var viewport = spriteBatch.GraphicsDevice.Viewport;
            var screenWidth = viewport.Width;
            var screenHeight = viewport.Height;

            // Затемнение фона
            var background = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(_pixelTexture, background, new Color(0, 0, 0, 180));

            // Заголовок
            const string title = "ВЫБЕРИТЕ УЛУЧШЕНИЕ";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2((screenWidth - titleSize.X) / 2, screenHeight * 0.1f);
            DrawTextWithShadow(spriteBatch, title, titlePos, Color.Gold);

            // Карточки перков
            const int cardWidth = 320;
            const int cardHeight = 240;
            const int spacing = 40;
            var totalWidth = _offeredPerks.Count * cardWidth +
                (_offeredPerks.Count - 1) * spacing;
            var startX = (screenWidth - totalWidth) / 2;
            var y = screenHeight * 0.3f;

            for (var i = 0; i < _offeredPerks.Count; i++)
            {
                var perk = _offeredPerks[i];
                var x = startX + i * (cardWidth + spacing);
                var cardRect = new Rectangle((int)x, (int)y, cardWidth, cardHeight);

                var cardColor = new Color(40, 40, 60, 240);
                var highlightColor = new Color(80, 80, 100, 255);

                // Фон карточки
                spriteBatch.Draw(_pixelTexture, cardRect, cardColor);

                // Верхняя подсветка
                var highlightRect = new Rectangle(cardRect.X, cardRect.Y, cardRect.Width, 10);
                spriteBatch.Draw(_pixelTexture, highlightRect, highlightColor);

                // Рамка
                var borderRect = new Rectangle(cardRect.X - 2, cardRect.Y - 2,
                    cardRect.Width + 4, cardRect.Height + 4);
                spriteBatch.Draw(_pixelTexture, borderRect, Color.Gold * 0.5f);

                // Название перка
                DrawTextWithShadow(spriteBatch, perk.Name, new Vector2(x + 20, y + 20), Color.Yellow);

                // Описание (с переносом строк)
                var wrappedDescription = WrapText(perk.Description, cardWidth - 40);
                DrawTextWithShadow(spriteBatch, wrappedDescription, new Vector2(x + 20, y + 60), Color.White);

                // Клавиша выбора
                var keyText = $"[{i + 1}]";
                var keySize = _font.MeasureString(keyText);
                var keyPos = new Vector2(x + cardWidth - keySize.X - 20, y + cardHeight - keySize.Y - 20);
                DrawTextWithShadow(spriteBatch, keyText, keyPos, Color.Cyan);
            }

            // Инструкция
            const string instruction = "Нажмите 1-3 для выбора улучшения";
            var instructionSize = _font.MeasureString(instruction);
            var instructionPos = new Vector2(
                (screenWidth - instructionSize.X) / 2, y + cardHeight + 50);
            DrawTextWithShadow(spriteBatch, instruction, instructionPos, Color.LightGray);
        }

        private void DrawTextWithShadow(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(_font, text, position + new Vector2(1, 1), Color.Black * 0.5f);
            spriteBatch.DrawString(_font, text, position, color);
        }

        private string WrapText(string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var words = text.Split(' ');
            var wrappedText = "";
            var line = "";

            foreach (var word in words)
            {
                var testLine = line + (line.Length == 0 ? "" : " ") + word;
                var size = _font.MeasureString(testLine);

                if (size.X <= maxWidth)
                {
                    line = testLine;
                }
                else
                {
                    if (line.Length > 0)
                    {
                        wrappedText += line + "\n";
                        line = word;
                    }
                    else
                    {
                        wrappedText += word + "\n";
                        line = "";
                    }
                }
            }

            if (line.Length > 0)
            {
                wrappedText += line;
            }

            return wrappedText;
        }
        
        private void OnPerkSelection(IEvent evt)
        {
            if (evt is not PerkSelectionEvent perkEvent) return;
            _isChoosingPerk = perkEvent.IsChoosing;
        }

        private void OnHealthChanged(IEvent evt)
        {
            if (evt is not HealthChangedEvent healthEvent) return;
            if (healthEvent.Entity is not Player) return;
            if (healthEvent.OldHealth > healthEvent.NewHealth)
            {
                _healthFlashTimer = HealthFlashDuration;
                _damageFlashTimer = DamageFlashDuration;
            }
        }

        private void OnComboChanged(IEvent evt)
        {
            if (evt is not ComboEvent comboEvent) return;
            if (comboEvent.ComboCount > _lastComboCount)
            {
                _comboPulseTimer = ComboPulseDuration;
            }
            _lastComboCount = comboEvent.ComboCount;
        }

        private void OnLevelUp(IEvent evt)
        {
            if (evt is not LevelUpEvent) return;
            _levelUpTimer = LevelUpDuration;
        }
    }
}
